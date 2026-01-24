namespace GoTyolo.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid UserId { get; set; }
    public int NumSeats { get; set; }
    public BookingState State { get; set; }
    public decimal PriceAtBooking { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public Trip? Trip { get; set; }
}