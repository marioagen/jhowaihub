using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Models
{
    public class PromptVariable : BaseEntity
    {
        [Column("Label", TypeName = "varchar(50)")]
        public string Label { get; set; } = string.Empty;

        [Column("Variable", TypeName = "varchar(50)")]
        public string Variable { get; set; } = string.Empty;

        [Column("Description", TypeName = "varchar(150)")]
        public string Description { get; set; } = string.Empty;

        [Column("Order", TypeName = "int")]
        public int Order { get; set; }

        [Column("Prompt_Id", TypeName = "int")]
        public int PromptId { get; set; }

        public virtual Prompt Prompt { get; set; }

        public PromptVariable(int id, DateTime created, string label, string variable, string description, int order, int promptId)
            : base(id, created)
        {
            Label = label;
            Variable = variable;
            Description = description;
            Order = order;
            PromptId = promptId;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private PromptVariable(int id, DateTime created) : base(id, created) { }
    }
}
