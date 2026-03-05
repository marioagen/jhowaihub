namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowInternalDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
