using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepToolDependency
    {
        [Column("DependencyStepToolId", TypeName = "int")]
        public int DependencyStepToolId { get; set; }

        [Column("DependentStepToolId", TypeName = "int")]
        public int DependentStepToolId { get; set; }

        public virtual StepTool DependencyStepTool { get; set; } = null!;
        public virtual StepTool DependentStepTool { get; set; } = null!;

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepToolDependency() { }
    }
}
