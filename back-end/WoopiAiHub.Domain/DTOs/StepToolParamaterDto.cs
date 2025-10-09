using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class StepToolParameterDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
