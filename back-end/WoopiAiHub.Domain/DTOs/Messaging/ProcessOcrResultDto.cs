using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;

namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class ProcessOcrResultDto
    {
        public string Tenant { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public MetaDataAutomationDto Data { get; set; }
        public AnalyzeResultCustomDto AnalyzeResult { get; set; } = new AnalyzeResultCustomDto();
    }
}
