using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public record class WorkflowPermissionDto
    {
        public int? ProfileId { get; set; }
        public int StepId { get; set; }
        public int PermissionId { get; set; }
    }
}
