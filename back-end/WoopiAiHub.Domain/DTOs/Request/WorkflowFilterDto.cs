using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WorkflowFilterDto
    {
        public string? Input { get; set; } = null;
        public string? Login { get; set; } = null;
        public Boolean? IsAllUsers { get; set; } = true;
    }
}