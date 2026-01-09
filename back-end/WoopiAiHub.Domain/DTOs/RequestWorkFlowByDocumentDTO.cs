using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public record class RequestWorkFlowByDocumentDto
    {
        public string Login { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string Search { get; set; } = string.Empty;
    }
}