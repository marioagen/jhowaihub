using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class PromptVariableUpdateDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Variable { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
