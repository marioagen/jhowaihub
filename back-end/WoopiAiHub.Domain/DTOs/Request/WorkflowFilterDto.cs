using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WorkflowFilterDto
    {
        public string Input { get; set; } = string.Empty;
        public Boolean IsAllUsers { get; set; } = false;
    }
}