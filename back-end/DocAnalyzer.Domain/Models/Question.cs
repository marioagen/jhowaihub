using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Models
{
    public class Question : BaseEntity
    {
        [Column("Description", TypeName = "varchar(max)")]
        public string Description { get; private set; } = string.Empty;
        [Column("EmailCreator", TypeName = "varchar(50)")]
        public string EmailCreator { get; set; } = string.Empty;
        public virtual ICollection<QuestionQuestionnaire> QuestionQuestionnaire { get; set; }
        
        public Question(string description,
                        string emailCreator,    
                        int id,
                        DateTime created) : base(id, created)
        {
            this.Description = description;
            this.EmailCreator = emailCreator;
        }
        /// <summary>
        /// Use to EF context
        /// </summary>
        private Question(int id, DateTime created) : base(id, created) { }
    }
}
