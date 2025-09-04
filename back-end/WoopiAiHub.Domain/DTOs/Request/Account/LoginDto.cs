namespace WoopiAiHub.Domain.DTOs.Request.Account
{
    public record class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}