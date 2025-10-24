using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.DTOs
{
    public record ChatChoiceDto
    {
        public ChatMessageResponseDto Message { get; set; } = new();
    }
}
