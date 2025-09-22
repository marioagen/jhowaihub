using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Tool : BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; } = string.Empty;
        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; private set; }
        [Column("ToolTypeId", TypeName = "int")]
        public int ToolTypeId { get; private set; }
        [Column("InputDataId", TypeName = "int")]
        public int InputDataId { get; private set; }
        [Column("OutputDataId", TypeName = "int")]
        public int OutputDataId { get; private set; }

        public virtual ToolType? ToolType { get; set; }
        public virtual ToolData? InputData { get; set; }
        public virtual ToolData? OutputData { get; set; }
        public virtual ICollection<StepTool> StepTools { get; set; } = new List<StepTool>();

        public Tool(int id, DateTime created, string name, bool isActive, int toolTypeId, int inputDataId, int outputDataId) 
            : base(id, created)
        {
            Name = name;
            IsActive = isActive;
            ToolTypeId = toolTypeId;
            InputDataId = inputDataId;
            OutputDataId = outputDataId;
        }
        public Tool(int id, DateTime created) : base(id, created) { }

        public void Update(string name, int toolTypeId, int inputDataId, int outputDataId)
        {
            Name = name;
            ToolTypeId = toolTypeId;
            InputDataId = inputDataId;
            OutputDataId = outputDataId;
        }
    }
}
