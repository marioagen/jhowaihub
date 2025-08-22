using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class ProcessOcrResultDto
    {
        public string Tenant { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public AnalyzeResultCustomDto AnalyzeResult { get; set; } = new AnalyzeResultCustomDto();
    }
}
