namespace WoopiAiHub.Domain.DTOs.Request
{
    public class AuthenticateDto
    {
        public string Login { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
    }
}
