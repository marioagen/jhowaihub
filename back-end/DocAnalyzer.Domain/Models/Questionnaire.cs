using DocAnalyzer.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Models
{
    public class Questionnaire : BaseEntity
    {
        [Column("Title", TypeName = "varchar(50)")]
        public string Title { get; set; }
        [Column("TypeDoc_Id", TypeName = "int")]
        public int TypeDocId { get; set; }
        [Column("EmailCreator", TypeName = "varchar(50)")]
        public string EmailCreator { get; set; }
        public TypeDoc TypeDoc { get; set; }

        public virtual ICollection<QuestionQuestionnaire> QuestionQuestionnaire { get; set; } = new Collection<QuestionQuestionnaire>();

        public Questionnaire(string title,
                                int typeDocId,
                                string emailCreator,
                                int id,
                                DateTime created) : base(id, created)
        {
            this.Title = title;
            this.TypeDocId = typeDocId;
            this.EmailCreator = emailCreator;
        }
        /// <summary>
        /// Use to EF context
        /// </summary>
        private Questionnaire(int id, DateTime created) : base(id, created) { }
    }
}
