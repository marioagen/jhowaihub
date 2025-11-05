using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class StepToolUpdateDto
    {
        public int? Id { get; set; }
        public int? StepId { get; set; }
        public int ToolId { get; set; }
        public int Order { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public int? DependsOnStepToolId { get; set; }
        public ICollection<int> DependsOnStepToolIds { get; set; } = [];
        public ICollection<StepToolParameterUpdateDto> Parameters { get; set; } = [];
        public ICollection<StepToolOutputDependencyDto> Dependencies { get; set; } = [];
    }
}
