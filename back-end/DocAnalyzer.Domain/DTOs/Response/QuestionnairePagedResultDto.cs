using DocAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.DTOs.Response
{
    public class QuestionnairePagedResultDto
    {
        public IEnumerable<QuestionnaireDto> Content { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}
