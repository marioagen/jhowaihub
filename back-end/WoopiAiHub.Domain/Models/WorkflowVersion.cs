using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class WorkflowVersion : BaseEntity
    {
        [Column("WorkflowId", TypeName = "int")]
        public int WorkflowId { get; private set; }

        [Column("VersionNumber", TypeName = "int")]
        public int VersionNumber { get; private set; }

        [Column("ConfigSnapshot", TypeName = "nvarchar(max)")]
        public string ConfigSnapshot { get; private set; } = string.Empty;

        [Column("TriggerToolId", TypeName = "int")]
        public int TriggerToolId { get; private set; }

        [Column("TriggerToolName", TypeName = "varchar(255)")]
        public string TriggerToolName { get; private set; } = string.Empty;

        public virtual Workflow? Workflow { get; set; }

        public WorkflowVersion(int id, DateTime created, int workflowId, int versionNumber, string configSnapshot, int triggerToolId, string triggerToolName)
            : base(id, created)
        {
            WorkflowId = workflowId;
            VersionNumber = versionNumber;
            ConfigSnapshot = configSnapshot;
            TriggerToolId = triggerToolId;
            TriggerToolName = triggerToolName;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private WorkflowVersion(int id, DateTime created) : base(id, created) { }
    }
}
