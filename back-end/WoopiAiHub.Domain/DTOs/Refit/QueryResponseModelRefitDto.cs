using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class QueryResponseModelRefitDto
    {
        public string response { get; set; }
        public JObject metadata { get; set; }
        public JArray metadata_array { get; set; }
    }
}
