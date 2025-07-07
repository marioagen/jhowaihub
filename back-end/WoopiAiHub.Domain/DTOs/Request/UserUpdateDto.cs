namespace WoopiAiHub.Domain.DTOs.Request
{
    public class UserUpdateDto
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string Name { get;  set; } = string.Empty;

        public string Email { get;  set; } = string.Empty;

        public ICollection<int>? TeamIds { get; set; }

    }
}
