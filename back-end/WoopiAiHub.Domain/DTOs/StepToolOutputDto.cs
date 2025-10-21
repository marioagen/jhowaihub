using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs
{
    public record class StepToolOutputDto(int Id, int StepToolId, int CardId, string value, StepTool? StepTool, Card? Card);
}
