using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepToolParameter : BaseEntity
    {
        [Column("StepToolId", TypeName = "int")]
        public int StepToolId { get; private set; }

        [Column("Value", TypeName = "nvarchar(max)")]
        public string Value { get; private set; }

        [Column("RequiredFile", TypeName = "bit")]
        public bool RequiredFile { get; private set; }

        [Column("WorkspaceId", TypeName = "uniqueidentifier")]
        public Guid? WorkspaceId { get; private set; }

        public virtual StepTool? StepTool { get; set; }

        public StepToolParameter(int id, 
                                 DateTime created, 
                                 int stepToolId, 
                                 bool requiredFile,
                                 Guid? workspaceId,
                                 string value) : base(id, created)
        {
            StepToolId = stepToolId;
            Value = value;
            RequiredFile = requiredFile;
            WorkspaceId = workspaceId;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepToolParameter(int id, DateTime created) : base(id, created) { }
    }
}
