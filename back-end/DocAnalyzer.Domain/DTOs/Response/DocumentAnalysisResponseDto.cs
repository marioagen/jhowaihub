using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.DTOs.Response
{
    public class DocumentAnalysisResponseDto
    {
        public int Id { get; set; }

        public string EmailCreator { get; set; } = string.Empty;

        public string Tenant { get; set; } = string.Empty;

        public string KeyMongoAcess { get; set; } = string.Empty;

        public string Embeddings_model_name { get; set; } = string.Empty;
    }
}
