namespace WoopiAiHub.Domain.DTOs
{
    public class SubscriptionPeriodDto
    {
        public string Tenant { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
