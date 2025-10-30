using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    /// <summary>
    /// Represents a dependency relationship between two StepTools.
    /// This allows a StepTool to depend on outputs from multiple other StepTools.
    /// </summary>
    public class StepToolDependency : BaseEntity
    {
        [Column("StepToolId", TypeName = "int")]
        public int StepToolId { get; private set; }

        [Column("DependsOnStepToolId", TypeName = "int")]
        public int DependsOnStepToolId { get; private set; }

        public virtual StepTool StepTool { get; set; }
        public virtual StepTool DependsOnStepTool { get; set; }

        public StepToolDependency(int id, 
                                  DateTime created,
                                  int stepToolId, 
                                  int dependsOnStepToolId) : base(id, created)
        {
            StepToolId = stepToolId;
            DependsOnStepToolId = dependsOnStepToolId;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepToolDependency(int id, DateTime created) : base(id, created) { }
    }
}
