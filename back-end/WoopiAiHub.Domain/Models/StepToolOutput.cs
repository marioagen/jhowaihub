using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepToolOutput : BaseEntity
    {
        [Column("StepToolId", TypeName = "int")]
        public int StepToolId { get; private set; }

        [Column("CardId", TypeName = "int")]
        public int CardId { get; private set; }

        [Column("Value", TypeName = "nvarchar(max)")]
        public string Value { get; private set; }

        public virtual StepTool StepTool { get; set; }
        public virtual Card Card { get; set; }

        public StepToolOutput(int id, 
                              DateTime created,
                              int stepToolId, 
                              int cardId, 
                              string value) : base(id, created)
        {
            StepToolId = stepToolId;
            CardId = cardId;
            Value = value;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepToolOutput(int id, DateTime created) : base(id, created) { }
    }

}
