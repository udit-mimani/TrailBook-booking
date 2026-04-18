using TrailBook.Domain.Entities;
using TrailBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TrailBook.Infrastructure.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, bool includeTrip = false);
    Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<Booking> AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task<List<Booking>> GetExpiredPendingBookingsAsync();
    Task<List<Booking>> GetByTripIdAsync(Guid tripId);
}

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private AppDbContext _context = context;

    public async Task<Booking?> GetByIdAsync(Guid id, bool includeTrip = false)
    {
        var query = _context.Bookings.AsQueryable();

        if (includeTrip)
            query = query.Include(b => b.Trip);

        return await query.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey);
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task UpdateAsync(Booking booking)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Booking>> GetExpiredPendingBookingsAsync()
    {
        return await _context.Bookings
            .Where(b => b.State == BookingState.PendingPayment && b.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByTripIdAsync(Guid tripId)
    {
        return await _context.Bookings
            .Where(b => b.TripId == tripId)
            .ToListAsync();
    }
}