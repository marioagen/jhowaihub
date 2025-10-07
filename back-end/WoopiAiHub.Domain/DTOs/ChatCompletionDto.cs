using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class ChatCompletionDto
    {
        public List<ChatMessageDto> Messages { get; set; } = new();
        public double Temperature { get; set; }
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
        public bool Stream { get; set; } = false;
    }
}
