using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.DTOs
{
    public record ChatCompletionQueryDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public string ApplicationKey { get; set; } = string.Empty;
        public MetaDataAutomationDto Data { get; set; }
        public ChatCompletionDto ChatCompletion { get; set; } = new();
        public string ResponseQueue { get; set; } = string.Empty;
    }
}
