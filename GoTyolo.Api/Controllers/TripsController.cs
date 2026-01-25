using GoTyolo.Api.Services;
using GoTyolo.Domain.Entities;
using GoTyolo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GoTyolo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripRepository _tripRepository;
    private readonly IBookingService _bookingService;

    public TripsController(ITripRepository tripRepository, IBookingService bookingService)
    {
        _tripRepository = tripRepository;
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Trip>>> GetTrips()
    {
        var trips = await _tripRepository.GetAllPublishedAsync();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> GetTrip(Guid id)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
            return NotFound();

        return Ok(trip);
    }

    [HttpPost]
    public async Task<ActionResult<Trip>> CreateTrip(CreateTripRequest request)
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Destination = request.Destination,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Price = request.Price,
            MaxCapacity = request.MaxCapacity,
            AvailableSeats = request.MaxCapacity,
            Status = TripStatus.Published,
            RefundPolicy = request.RefundPolicy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tripRepository.AddAsync(trip);
        return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, trip);
    }

    [HttpPost("{id}/book")]
    public async Task<ActionResult<BookingResponse>> BookTrip(Guid id, BookTripRequest request)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(id, request.UserId, request.NumSeats);

            return Ok(new BookingResponse(
            booking.Id,
            booking.State.ToString(),
            booking.PriceAtBooking,
            booking.ExpiresAt,
            $"https://payment-provider.example.com/pay/{booking.Id}"
        ));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}

public record CreateTripRequest(
    string Title,
    string Destination,
    DateTime StartDate,
    DateTime EndDate,
    decimal Price,
    int MaxCapacity,
    RefundPolicy RefundPolicy
);

public record BookTripRequest(Guid UserId, int NumSeats);

public record BookingResponse(
    Guid BookingId,
    string State,
    decimal PriceAtBooking,
    DateTime ExpiresAt,
    string PaymentUrl
);