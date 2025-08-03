namespace WoopiAiHub.Domain.DTOs.Response.Account
{
    public record class AccessDataAuthDto
    {
        public string Token { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
