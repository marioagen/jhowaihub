using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ResponseWorkflowByDocumentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CardId { get; set; }
        public int DocumentId { get; set; }
        public string AssignedUserEmail { get; set; } = string.Empty;
    }
}