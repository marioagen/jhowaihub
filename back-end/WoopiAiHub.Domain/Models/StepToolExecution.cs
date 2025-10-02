using System.ComponentModel.DataAnnotations.Schema;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Models
{
    public class StepToolExecution : BaseEntity
    {
        [Column("StepToolId", TypeName = "int")]
        public int StepToolId { get; private set; }

        [Column("CardId", TypeName = "int")]
        public int CardId { get; private set; }

        [Column("Started", TypeName = "datetime")]
        public DateTime Started { get; private set; }

        [Column("Completed", TypeName = "datetime")]
        public DateTime? Completed { get; private set; }

        [Column("Status", TypeName = "int")]
        public StatusExecution Status { get; private set; }

        public virtual StepTool? StepTool { get; set; }
        public virtual Card? Card { get; set; }

        public StepToolExecution(int id, 
                                 DateTime created, 
                                 int stepToolId,
                                 StatusExecution status,
                                 int cardId) : base(id, created)
        {
            StepToolId = stepToolId;
            CardId = cardId;
            Started = DateTime.Now;
            Status = status;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private StepToolExecution(int id, DateTime created) : base(id, created) { }

        public void UpdateStatusExecution(StatusExecution status)
        {
            Status = status;
            if (status.Equals(StatusExecution.Ready))
            {
                Completed = DateTime.Now;
            }
        }
    }
}
