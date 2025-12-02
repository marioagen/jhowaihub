namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record class TenantListDto
    {
        public string Name { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string BillingId { get; set; } = string.Empty;
    }
}
