using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class ChatCompletionQueryDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public ChatCompletionDto ChatCompletion { get; set; } = new();
    }
}
