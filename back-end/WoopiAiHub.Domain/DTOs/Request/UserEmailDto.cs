namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class UserEmailDto
    {        
        public string Email { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}
