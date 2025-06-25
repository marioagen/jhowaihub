using DocAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string EmailCreator { get; set; } = string.Empty;
        public virtual ICollection<Questionnaire> Questionnaires { get; set; }
        public DateTime Created { get; set; }

    }
}
