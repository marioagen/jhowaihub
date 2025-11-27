namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class ChargeRequestDto
    {
        public string TenantName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalUsage { get; set; }
    }
}
