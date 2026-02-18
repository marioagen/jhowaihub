using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class StepToolOutputAnalysesDto
    {
        public int Id { get; set; }
        public int StepToolId { get; set; }
        public int CardId { get; set; }
        public string Value { get; set; } = string.Empty;
        public StepToolDto? StepTool { get; set; }
    }
}
