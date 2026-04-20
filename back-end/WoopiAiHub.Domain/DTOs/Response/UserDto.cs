namespace WoopiAiHub.Domain.DTOs.Response
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<TeamDto>? Teams { get; set; } = [];
        public List<ProfileDto>? Profiles { get; set; } = [];
        public DateTime Created { get; set; }
    }
}
