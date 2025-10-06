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
        public string? Input { get; set; } = string.Empty;
        public bool RequiredFile { get; set; }  
        public Guid? WorkspaceId { get; set; }
    }
}
