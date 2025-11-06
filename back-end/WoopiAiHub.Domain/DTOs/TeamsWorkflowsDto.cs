namespace WoopiAiHub.Domain.DTOs
{
    public class TeamsWorkflowsDto
    {
        public int TeamId { get; set; }
        public List<int> Workflows { get; set; } = new List<int>();
    }
}
