namespace TrailBook.Domain.Entities
{
    public enum TripStatus
    {
        Draft,
        Published
    }

    public enum BookingState
    {
        PendingPayment,
        Confirmed,
        Cancelled,
        Expired
    }
}
