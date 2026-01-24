namespace GoTyolo.Domain.Entities
{
    public class RefundPolicy
    {
        public int RefundableUntilDaysBefore { get; set; }
        public int CancellationFeePercent { get; set; }
    }
}
