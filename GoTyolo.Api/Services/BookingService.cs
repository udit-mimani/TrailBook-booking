using GoTyolo.Domain.Entities;
using GoTyolo.Infrastructure.Data;
using GoTyolo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoTyolo.Api.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid tripId, Guid userId, int numSeats);
    Task<Booking> ProcessPaymentWebhookAsync(Guid bookingId, string status, string idempotencyKey);
    Task<Booking> CancelBookingAsync(Guid bookingId);
}

public class BookingService(AppDbContext context, ITripRepository tripRepository, IBookingRepository bookingRepository, ILogger<BookingService> logger) : IBookingService
{
    private AppDbContext _context = context;
    private ITripRepository _tripRepository = tripRepository;
    private IBookingRepository _bookingRepository = bookingRepository;
    private ILogger<BookingService> _logger = logger;

    // To create a new booking, this method first checks if the trip exists, if yes then it locks the trip row to avoid overbooking
    // and if it it possible to make a booking, it reserves seats and create a booking.
    public async Task<Booking> CreateBookingAsync(Guid tripId, Guid userId, int numSeats)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Lock the trip row for update
            var trip = await _tripRepository.GetByIdAsync(tripId, forUpdate: true);

            if (trip == null)
                throw new InvalidOperationException("Trip not found");

            if (!trip.CanBook(numSeats))
                throw new InvalidOperationException($"Cannot book {numSeats} seats. Available: {trip.AvailableSeats}");

            trip.ReserveSeats(numSeats);
            await _tripRepository.UpdateAsync(trip);

            var booking = Booking.Create(tripId, userId, numSeats, trip.Price);
            await _bookingRepository.AddAsync(booking);

            await transaction.CommitAsync();

            _logger.LogInformation("Booking {BookingId} created for trip {TripId}, {NumSeats} seats reserved",
                booking.Id, tripId, numSeats);

            return booking;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // This method first checks the idempotency key to avoid duplicates
    // if status is success, it marks the status of booking as confirmed.
    // otherwise it changes the status of booking as expired and releases the seats and fa
    public async Task<Booking> ProcessPaymentWebhookAsync(Guid bookingId, string status, string idempotencyKey)
    {
        // Check idempotency
        var existingBooking = await _bookingRepository.GetByIdempotencyKeyAsync(idempotencyKey);
        if (existingBooking != null)
        {
            _logger.LogInformation("Duplicate webhook with idempotency key {Key}, returning existing booking", idempotencyKey);
            return existingBooking;
        }

        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeTrip: true);
        if (booking == null)
        {
            _logger.LogWarning("Webhook received for non-existent booking {BookingId}", bookingId);
            throw new InvalidOperationException("Booking not found");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (status.ToLower() == "success")
            {
                booking.ConfirmPayment(idempotencyKey);
                _logger.LogInformation("Booking {BookingId} confirmed via webhook", bookingId);
            }
            else
            {
                booking.FailPayment();

                var trip = await _tripRepository.GetByIdAsync(booking.TripId, forUpdate: true);
                if (trip != null)
                {
                    trip.ReleaseSeats(booking.NumSeats);
                    await _tripRepository.UpdateAsync(trip);
                }

                _logger.LogInformation("Booking {BookingId} failed via webhook, seats released", bookingId);
            }

            // Store idempotency key
            booking.IdempotencyKey = idempotencyKey;
            await _bookingRepository.UpdateAsync(booking);

            await transaction.CommitAsync();

            return booking;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Booking> CancelBookingAsync(Guid bookingId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, includeTrip: true);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            if (booking.Trip == null)
                throw new InvalidOperationException("Trip not found");

            var refundAmount = booking.Cancel(booking.Trip);

            // Release seats if eligible
            var daysUntilTrip = (booking.Trip.StartDate - DateTime.UtcNow).TotalDays;
            if (daysUntilTrip > booking.Trip.RefundPolicy.RefundableUntilDaysBefore)
            {
                var trip = await _tripRepository.GetByIdAsync(booking.TripId, forUpdate: true);
                if (trip != null)
                {
                    trip.ReleaseSeats(booking.NumSeats);
                    await _tripRepository.UpdateAsync(trip);
                }
            }

            await _bookingRepository.UpdateAsync(booking);
            await transaction.CommitAsync();

            _logger.LogInformation("Booking {BookingId} cancelled, refund: ${RefundAmount}", bookingId, refundAmount);

            return booking;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}