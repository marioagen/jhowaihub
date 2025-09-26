using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class StepToolDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StepId { get; set; }
        public int ToolId { get; set; }
        public int Order { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public int? DependsOnStepToolId { get; set; }
        public virtual StepToolDto? DependsOnStepTool { get; set; }
        public virtual Step Step { get; set; }
        public virtual ToolDto Tool { get; set; }
        public virtual ICollection<StepToolParameterDto> Parameters { get; set; } = new List<StepToolParameterDto>();
    }
}
