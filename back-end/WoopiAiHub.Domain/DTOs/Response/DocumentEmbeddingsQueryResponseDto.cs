using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class DocumentEmbeddingsQueryResponseDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string KeyMongoAccess { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public JObject Data { get; set; } = new JObject();
        public IEnumerable<QuestionAnswerDto> QuestionsAnswers { get; set; } = Enumerable.Empty<QuestionAnswerDto>();
    }
}