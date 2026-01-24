using GoTyolo.Domain.Entities;
using GoTyolo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoTyolo.Infrastructure.Repositories;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid id, bool forUpdate = false);
    Task<List<Trip>> GetAllPublishedAsync();
    Task<Trip> AddAsync(Trip trip);
    Task UpdateAsync(Trip trip);
    Task<List<Trip>> GetAtRiskTripsAsync(int daysThreshold, int occupancyThreshold);
}

public class TripRepository(AppDbContext context) : ITripRepository
{
    private AppDbContext _context = context;

    public async Task<Trip?> GetByIdAsync(Guid id, bool forUpdate = false)
    {
        if (forUpdate)
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT * FROM \"Trips\" WHERE \"Id\" = {0} FOR UPDATE", id);
        }

        return await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Trip>> GetAllPublishedAsync()
    {
        return await _context.Trips
            .Where(t => t.Status == TripStatus.Published)
            .OrderBy(t => t.StartDate)
            .ToListAsync();
    }

    public async Task<Trip> AddAsync(Trip trip)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task UpdateAsync(Trip trip)
    {
        trip.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Trip>> GetAtRiskTripsAsync(int daysThreshold, int occupancyThreshold)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);

        return await _context.Trips
            .Where(t => t.Status == TripStatus.Published &&
                       t.StartDate <= thresholdDate &&
                       t.StartDate > DateTime.UtcNow &&
                       (t.MaxCapacity - t.AvailableSeats) * 100 / t.MaxCapacity < occupancyThreshold)
            .ToListAsync();
    }
}