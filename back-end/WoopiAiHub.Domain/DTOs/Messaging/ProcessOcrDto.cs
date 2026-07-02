using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class ProcessOcrDto
    {
        public string Tenant { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ResponseQueue { get; set; } = string.Empty;
        public string ExtractionMode { get; set; } = DocumentExtractionModes.Auto;
        public MetaDataAutomationDto Data { get; set; }
    }
}
