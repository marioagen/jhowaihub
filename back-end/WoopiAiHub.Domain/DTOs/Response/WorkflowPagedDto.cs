using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class WorkflowPagedDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public String? Login { get; set; } = null;
        public bool IsAllUsers { get; set; } = true;
    }
}
