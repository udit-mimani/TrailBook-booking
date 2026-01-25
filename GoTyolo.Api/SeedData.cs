using GoTyolo.Domain.Entities;
using GoTyolo.Infrastructure.Data;

namespace GoTyolo.Api;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Trips.Any())
            return;

        var now = DateTime.UtcNow;

        var trips = new List<Trip>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Paris City Tour",
                Destination = "Paris, France",
                StartDate = now.AddDays(10),
                EndDate = now.AddDays(13),
                Price = 100m,
                MaxCapacity = 20,
                AvailableSeats = 20,
                Status = TripStatus.Published,
                RefundPolicy = new RefundPolicy { RefundableUntilDaysBefore = 7, CancellationFeePercent = 10 },
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Tokyo Adventure",
                Destination = "Tokyo, Japan",
                StartDate = now.AddDays(5),
                EndDate = now.AddDays(12),
                Price = 200m,
                MaxCapacity = 15,
                AvailableSeats = 15,
                Status = TripStatus.Published,
                RefundPolicy = new RefundPolicy { RefundableUntilDaysBefore = 14, CancellationFeePercent = 15 },
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "New York Express",
                Destination = "New York, USA",
                StartDate = now.AddDays(30),
                EndDate = now.AddDays(35),
                Price = 150m,
                MaxCapacity = 25,
                AvailableSeats = 25,
                Status = TripStatus.Published,
                RefundPolicy = new RefundPolicy { RefundableUntilDaysBefore = 7, CancellationFeePercent = 10 },
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        context.Trips.AddRange(trips);
        await context.SaveChangesAsync();
    }
}