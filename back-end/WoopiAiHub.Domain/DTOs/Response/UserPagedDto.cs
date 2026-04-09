namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class UserPagedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public IEnumerable<TeamDto> Teams { get; set; } = Enumerable.Empty<TeamDto>();
        public IEnumerable<ProfileDto> Profiles { get; set; } = Enumerable.Empty<ProfileDto>();
    }
}
