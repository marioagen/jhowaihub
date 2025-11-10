namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class UserCreateDto
    {
        public string Name { get;  set; } = string.Empty;

        public string Email { get;  set; } = string.Empty;

        public string Password { get;  set; } = string.Empty;

        public ICollection<int> TeamIds { get; set; } = new List<int>();
    }
}

