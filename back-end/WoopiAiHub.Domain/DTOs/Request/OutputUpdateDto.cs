using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class OutputUpdateDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
