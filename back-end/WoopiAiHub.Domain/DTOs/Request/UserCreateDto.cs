namespace WoopiAiHub.Domain.DTOs.Request
{
    public class UserCreateDto
    {
        public string Name { get;  set; } = string.Empty;

        public string Email { get;  set; } = string.Empty;

        public ICollection<int> TeamIds { get; set; } 
    }

}

