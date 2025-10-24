using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Application.Utils
{
    public class ChatCompletionSettings
    {
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
    }
}
