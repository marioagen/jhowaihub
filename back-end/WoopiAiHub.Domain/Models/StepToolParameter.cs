using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepToolParameter : BaseEntity
    {
        [Column("StepToolId", TypeName = "int")]
        public int StepToolId { get; private set; }

        [Column("Value", TypeName = "nvarchar(max)")]
        public string Value { get; private set; }

        public virtual StepTool? StepTool { get; set; }

        public StepToolParameter(int id, 
                                 DateTime created, 
                                 int stepToolId, 
                                 string value) : base(id, created)
        {
            StepToolId = stepToolId;
            Value = value;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepToolParameter(int id, DateTime created) : base(id, created) { }
    }
}
