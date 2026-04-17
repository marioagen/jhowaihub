using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Repository.Util.Records
{
    public record ExecutionProjection(
              int CardId,
              int StepToolId,
              StatusExecution Status,
              int StepId,
              string ToolName);
}
