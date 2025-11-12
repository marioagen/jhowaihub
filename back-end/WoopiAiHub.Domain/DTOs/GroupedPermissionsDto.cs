namespace WoopiAiHub.Domain.DTOs
{
    public record class GroupedPermissionsDto
    {
        public string Group { get; set; }
        public List<PermissionDto> Permissions { get; set; }
    }
}
