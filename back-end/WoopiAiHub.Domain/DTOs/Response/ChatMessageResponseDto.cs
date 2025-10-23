using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record ChatMessageResponseDto
    {
        public string Role { get; set; } = "system";
        public string Content { get; set; } = string.Empty;
    }
}
