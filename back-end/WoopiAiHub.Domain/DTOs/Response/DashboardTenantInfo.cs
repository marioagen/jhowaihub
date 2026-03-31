namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class DashboardTenantInfo
    {
        public int WtcIncluded { get; set; }
        public string Plan { get; set; } = string.Empty;
    }
}
