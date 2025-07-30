namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public IEnumerable<PermissionDto> Permissions { get; set; } = Enumerable.Empty<PermissionDto>();
        public IEnumerable<UserDto> Users { get; set; } = Enumerable.Empty<UserDto>();
    }
}
