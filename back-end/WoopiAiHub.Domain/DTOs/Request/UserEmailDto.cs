namespace WoopiAiHub.Domain.DTOs.Request
{
    public record UserEmailDto
    {        
        public string Email { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}
