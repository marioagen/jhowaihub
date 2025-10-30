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

        public virtual StepTool? DependsOnStepTool { get; set; }
        public virtual Step? Step { get; set; }
        public virtual Tool? Tool { get; set; }
        public virtual ICollection<StepToolOutput> Outputs { get; set; } = new List<StepToolOutput>();
        public virtual ICollection<StepToolExecution> Executions { get; private set; } = new List<StepToolExecution>();
        public virtual ICollection<StepToolParameter> Parameters { get; private set; } = new List<StepToolParameter>();
        public virtual ICollection<StepToolDependency> Dependencies { get; private set; } = new List<StepToolDependency>();

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

        public void UpdateDependencyStepTool(StepTool stepTool)
        {
            ArgumentNullException.ThrowIfNull(stepTool);
            if (Id == stepTool.Id)
            {
                return;
            }
            DependsOnStepTool = stepTool;
        }

        public void UpdateDependencyStepToolId(int stepToolId)
        {
            DependsOnStepToolId = stepToolId;
        }

        public void Update(int toolId, int order, decimal positionX, decimal positionY, int? dependsOnStepToolId)
        {
            ToolId = toolId;
            Order = order;
            PositionX = positionX;
            PositionY = positionY;
            if (dependsOnStepToolId.HasValue)
                DependsOnStepToolId = dependsOnStepToolId;
        }

        public void RemoveDependency()
        {
            DependsOnStepToolId = null;
            DependsOnStepTool = null;
        }

        public void AddDependency(StepToolDependency dependency)
        {
            ArgumentNullException.ThrowIfNull(dependency);
            Dependencies.Add(dependency);
        }

        public void RemoveDependencies()
        {
            Dependencies.Clear();
        }

        public void UpdateDependencies(ICollection<int> dependsOnStepToolIds)
        {
            ArgumentNullException.ThrowIfNull(dependsOnStepToolIds);
            
            // Remove existing dependencies
            Dependencies.Clear();
            
            // Add new dependencies
            foreach (var dependsOnId in dependsOnStepToolIds)
            {
                var dependency = new StepToolDependency(0, DateTime.UtcNow, Id, dependsOnId);
                Dependencies.Add(dependency);
            }
        }
    }
}
