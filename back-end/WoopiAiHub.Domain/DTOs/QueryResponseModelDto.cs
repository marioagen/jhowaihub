using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class QueryResponseModelDto
    {
        public string response { get; set; }
        public JsonNode metadata { get; set; }
        public JsonNode metadata_array { get; set; }
    }
}
