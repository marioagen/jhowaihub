using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class ChatCompletionResponseDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<ChatChoiceDto> Choices { get; set; } = new();
        public ChatUsageDto Usage { get; set; } = new();
        public JObject Data { get; set; }
    }
}
