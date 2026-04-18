namespace TrailBook.Domain.Entities;

public class Trip
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int MaxCapacity { get; set; }
    public int AvailableSeats { get; set; }
    public TripStatus Status { get; set; }
    public RefundPolicy RefundPolicy { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool CanBook(int numSeats)
    {
        return Status == TripStatus.Published && AvailableSeats >= numSeats;
    }

    public void ReserveSeats(int numSeats)
    {
        if (!CanBook(numSeats))
            throw new InvalidOperationException("Cannot reserve seats");

        AvailableSeats -= numSeats;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseSeats(int numSeats)
    {
        AvailableSeats += numSeats;
        if (AvailableSeats > MaxCapacity)
            AvailableSeats = MaxCapacity;

        UpdatedAt = DateTime.UtcNow;
    }
}