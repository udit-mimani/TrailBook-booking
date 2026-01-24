namespace GoTyolo.Domain.Entities;

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
}