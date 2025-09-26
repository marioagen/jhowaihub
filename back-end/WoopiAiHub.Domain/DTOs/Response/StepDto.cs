using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class StepDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public int Order { get; set; }
        public ProfileDto Profile { get; set; } = new();
        public StatusDto Status { get; set; } = new();
        public ICollection<CardDto> Cards { get; set; } = [];
        public ICollection<StepToolDto> StepTools { get; set; } = [];
    }
}
