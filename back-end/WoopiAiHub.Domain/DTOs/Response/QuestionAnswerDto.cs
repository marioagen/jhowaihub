using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class QuestionAnswerDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public ICollection<QueryUsageDto> Usage { get; set; } = [];
    }
}