using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.DTOs.Request
{
    public class DocumentQuestionnaireDto
    {
        public int IdDocument { get; set; }

        public int IdQuestionnaire { get; set; }
    }
}
