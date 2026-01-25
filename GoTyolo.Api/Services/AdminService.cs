using GoTyolo.Domain.Entities;
using GoTyolo.Infrastructure.Repositories;

namespace GoTyolo.Api.Services;

public interface IAdminService
{
    Task<TripMetrics> GetTripMetricsAsync(Guid tripId);
    Task<List<AtRiskTrip>> GetAtRiskTripsAsync();
}

public class AdminService(ITripRepository tripRepository, IBookingRepository bookingRepository) : IAdminService
{
    private ITripRepository _tripRepository = tripRepository;
    private IBookingRepository _bookingRepository = bookingRepository;

    public async Task<TripMetrics> GetTripMetricsAsync(Guid tripId)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
            throw new InvalidOperationException("Trip not found");

        var bookings = await _bookingRepository.GetByTripIdAsync(tripId);

        var bookedSeats = trip.MaxCapacity - trip.AvailableSeats;
        var occupancyPercent = trip.MaxCapacity > 0
            ? (decimal)bookedSeats / trip.MaxCapacity * 100
            : 0;

        var bookingSummary = bookings.GroupBy(b => b.State)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var grossRevenue = bookings
            .Where(b => b.State == BookingState.Confirmed || b.State == BookingState.Cancelled)
            .Sum(b => b.PriceAtBooking);

        var refundsIssued = bookings
            .Where(b => b.State == BookingState.Cancelled && b.RefundAmount.HasValue)
            .Sum(b => b.RefundAmount!.Value);

        return new TripMetrics
        {
            TripId = trip.Id,
            Title = trip.Title,
            OccupancyPercent = occupancyPercent,
            TotalSeats = trip.MaxCapacity,
            BookedSeats = bookedSeats,
            AvailableSeats = trip.AvailableSeats,
            BookingSummary = bookingSummary,
            Financial = new FinancialMetrics
            {
                GrossRevenue = grossRevenue,
                RefundsIssued = refundsIssued,
                NetRevenue = grossRevenue - refundsIssued
            }
        };
    }

    public async Task<List<AtRiskTrip>> GetAtRiskTripsAsync()
    {
        var trips = await _tripRepository.GetAtRiskTripsAsync(daysThreshold: 7, occupancyThreshold: 50);

        return trips.Select(t => new AtRiskTrip
        {
            TripId = t.Id,
            Title = t.Title,
            DepartureDate = t.StartDate,
            OccupancyPercent = t.MaxCapacity > 0
                ? (decimal)(t.MaxCapacity - t.AvailableSeats) / t.MaxCapacity * 100
                : 0,
            Reason = "Low occupancy with imminent departure"
        }).ToList();
    }
}

public class TripMetrics
{
    public Guid TripId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal OccupancyPercent { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public int AvailableSeats { get; set; }
    public Dictionary<string, int> BookingSummary { get; set; } = new();
    public FinancialMetrics Financial { get; set; } = new();
}

public class FinancialMetrics
{
    public decimal GrossRevenue { get; set; }
    public decimal RefundsIssued { get; set; }
    public decimal NetRevenue { get; set; }
}

public class AtRiskTrip
{
    public Guid TripId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public decimal OccupancyPercent { get; set; }
    public string Reason { get; set; } = string.Empty;
}