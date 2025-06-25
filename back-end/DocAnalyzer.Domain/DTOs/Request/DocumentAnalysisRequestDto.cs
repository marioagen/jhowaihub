using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.DTOs.Request
{
    public class DocumentAnalysisRequestDto
    {
        public int Id { get; set; }

        public string Embeddings_model_name { get; set; } = string.Empty;
    }
}
