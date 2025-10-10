using Newtonsoft.Json.Linq;
using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.DTOs.Response.Automation
{
    public class AutomationOutputDto
    {
        public string? Tenant { get; set; }
        public string? Email { get; set; }
        public string ReferenceFile { get; set; } = string.Empty;
        public string ResponseQueue { get; set; } = string.Empty;
        public MetaDataAutomationDto Data { get; set; } = new MetaDataAutomationDto();
        public object? Content { get; set; }
    }
}
