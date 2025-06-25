using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Models
{
    public class QuestionQuestionnaire
    {
        [Column("Questionnaire_Id", TypeName = "int")]
        public int QuestionnaireId { get; private set; }
        public Questionnaire Questionnaire { get; set; } = null!;
        [Column("Question_Id", TypeName = "int")]
        public int QuestionId { get; private set; }
        public Question Question { get; set; } = null!;

        public QuestionQuestionnaire(int questionId, int questionnaireId)
        {
            this.QuestionId = questionId;
            this.QuestionnaireId = questionnaireId;
        }
        public QuestionQuestionnaire( Question question, Questionnaire questionnaire)
        {
            this.Question = question;
            this.Questionnaire = questionnaire;
        }
        /// <summary>
        /// Use to EF context
        /// </summary>
        private QuestionQuestionnaire() { }
    }
}
