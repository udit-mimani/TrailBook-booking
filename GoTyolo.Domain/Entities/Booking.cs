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

    public static Booking Create(Guid tripId, Guid userId, int numSeats, decimal pricePerSeat)
    {
        var now = DateTime.UtcNow;
        return new Booking
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            UserId = userId,
            NumSeats = numSeats,
            State = BookingState.PendingPayment,
            PriceAtBooking = pricePerSeat * numSeats,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(15),
            UpdatedAt = now,
            IdempotencyKey = Guid.NewGuid().ToString()
        };
    }

    public void ConfirmPayment(string paymentReference)
    {
        if (State != BookingState.PendingPayment)
            throw new InvalidOperationException($"Cannot confirm payment for booking in state {State}");

        State = BookingState.Confirmed;
        PaymentReference = paymentReference;
        UpdatedAt = DateTime.UtcNow;
    }

    public void FailPayment()
    {
        if (State != BookingState.PendingPayment)
            throw new InvalidOperationException($"Cannot fail payment for booking in state {State}");

        State = BookingState.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (State != BookingState.PendingPayment)
            return;

        State = BookingState.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal Cancel(Trip trip)
    {
        if (State != BookingState.Confirmed)
            throw new InvalidOperationException($"Cannot cancel booking in state {State}");

        var refundAmount = CalculateRefund(trip);

        State = BookingState.Cancelled;
        CancelledAt = DateTime.UtcNow;
        RefundAmount = refundAmount;
        UpdatedAt = DateTime.UtcNow;

        return refundAmount;
    }

    public decimal CalculateRefund(Trip trip)
    {
        var daysUntilTrip = (trip.StartDate - DateTime.UtcNow).TotalDays;

        if (daysUntilTrip <= trip.RefundPolicy.RefundableUntilDaysBefore)
            return 0;

        var feePercent = trip.RefundPolicy.CancellationFeePercent / 100m;
        return PriceAtBooking * (1 - feePercent);
    }

    public bool IsExpired() => State == BookingState.PendingPayment && DateTime.UtcNow > ExpiresAt;
}