using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs
{
    public record class StepToolExecutionDto(int Id,
                                             int StepToolId,
                                             int CardId,
                                             DateTime Started,
                                             DateTime? Completed,
                                             StatusExecution Status,
                                             StepTool? StepTool = null,
                                             Card? Card = null);
}
