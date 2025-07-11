namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class RequestCheckIsAdminDto
    {
        public string Email { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
    }
}
