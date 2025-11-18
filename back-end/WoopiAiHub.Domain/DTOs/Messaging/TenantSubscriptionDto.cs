namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class TenantSubscriptionDto
    {
        public Guid MarketplaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string DataBaseName { get; set; } = string.Empty;
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateTime? DateRenew { get; set; }
    }
}
