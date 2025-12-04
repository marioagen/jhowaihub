using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class QueryResponseModelRefitDto
    {
        public string response { get; set; }
        public JObject metadata { get; set; }
        public JArray metadata_array { get; set; }
        public ICollection<QueryUsageDto> Usage { get; set; } = [];

    }
}
