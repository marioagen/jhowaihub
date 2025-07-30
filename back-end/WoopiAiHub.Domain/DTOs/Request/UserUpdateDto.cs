namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class UserUpdateDto
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string Name { get;  set; } = string.Empty;

        public string Email { get;  set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public ICollection<int>? TeamIds { get; set; }
        public ICollection<int>? ProfileIds { get; set; }
    }
}
