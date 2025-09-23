using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class StepToolCreateDto
    {
        public int StepId { get; set; }
        public int ToolId { get; set; }
        public int Order { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public int? DependsOnStepToolId { get; set; }

    }
}
