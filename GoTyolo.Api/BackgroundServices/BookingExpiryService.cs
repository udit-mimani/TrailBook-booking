using GoTyolo.Infrastructure.Data;
using GoTyolo.Infrastructure.Repositories;

namespace GoTyolo.Api.BackgroundServices;

public class BookingExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingExpiryService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public BookingExpiryService(IServiceProvider serviceProvider, ILogger<BookingExpiryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Expiry Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredBookingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired bookings");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessExpiredBookingsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var tripRepo = scope.ServiceProvider.GetRequiredService<ITripRepository>();

        var expiredBookings = await bookingRepo.GetExpiredPendingBookingsAsync();

        foreach (var booking in expiredBookings)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                booking.Expire();

                var trip = await tripRepo.GetByIdAsync(booking.TripId, forUpdate: true);
                if (trip != null)
                {
                    trip.ReleaseSeats(booking.NumSeats);
                    await tripRepo.UpdateAsync(trip);
                }

                await bookingRepo.UpdateAsync(booking);
                await transaction.CommitAsync();

                _logger.LogInformation("Expired booking {BookingId}, released {NumSeats} seats",
                    booking.Id, booking.NumSeats);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to expire booking {BookingId}", booking.Id);
            }
        }
    }
}