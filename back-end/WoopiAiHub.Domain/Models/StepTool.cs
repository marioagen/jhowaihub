using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepTool : BaseEntity
    {
        [Column("StepId", TypeName = "int")]
        public int StepId { get; private set; }

        [Column("ToolId", TypeName = "int")]
        public int ToolId { get; private set; }

        [Column("StepOrder", TypeName = "int")]
        public int Order { get; private set; }

        [Column("PositionX", TypeName = "decimal(9,2)")]
        public decimal PositionX { get; private set; }

        [Column("PositionY", TypeName = "decimal(9,2)")]
        public decimal PositionY { get; private set; }

        [Column("DependsOnStepToolId", TypeName = "int")]
        public int? DependsOnStepToolId { get; private set; }

        public virtual StepTool? DependsOnStepTool { get; private set; }
        public virtual required Step Step { get; set; }
        public virtual required Tool Tool { get; set; }
        public virtual ICollection<StepToolOutput> Outputs { get; private set; } = new List<StepToolOutput>();

        public StepTool(int id, 
                       DateTime created, 
                       int stepId, 
                       int toolId, 
                       int order, 
                       decimal positionX, 
                       decimal positionY) : base(id, created)
        {
            StepId = stepId;
            ToolId = toolId;
            Order = order;
            PositionX = positionX;
            PositionY = positionY;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepTool(int id, DateTime created) : base(id, created) { }
    }
}
