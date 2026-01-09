using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public record class RequestWorkFlowByDocumentDTO
    {

        public string Login { get; set; }
        public int DocumentId { get; set; }
        public string Search { get; set; }
        
    }
}