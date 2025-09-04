namespace WoopiAiHub.Domain.DTOs.Response
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public IEnumerable<UserDto> Users { get; set; } = Enumerable.Empty<UserDto>();
        public WorkflowDto? Workflow { get; set; }
    }
}
