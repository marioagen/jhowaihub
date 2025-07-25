namespace WoopiAiHub.Domain.DTOs.Response.Account
{
    public class LoginDataDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public Array? Permissions { get; set; }
    }
}