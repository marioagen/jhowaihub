using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Models
{
    public class Prompt : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Description", TypeName = "varchar(95)")]
        public string Description { get; private set; } = string.Empty;

        [Column("Text", TypeName = "varchar(max)")]
        public string Text { get; private set; } = string.Empty;

        [Column("EmailCreator", TypeName = "varchar(100)")]
        public string EmailCreator { get; private set; } = string.Empty;

        public virtual ICollection<PromptVariable> Variables { get; set; }

        public virtual ICollection<Document> Documents { get; set; }

        public Prompt(int id, DateTime created, string name, string description, string text, string emailCreator)
            : base(id, created)
        {
            Name = name;
            Description = description;
            Text = text;
            EmailCreator = emailCreator;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Prompt(int id, DateTime created) : base(id, created) { }
    }
}
