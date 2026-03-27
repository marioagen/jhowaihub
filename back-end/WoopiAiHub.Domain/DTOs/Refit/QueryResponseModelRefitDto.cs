using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class QueryResponseModelRefitDto
    {
        public string response { get; set; } = string.Empty;
        public ICollection<QueryUsageDto> Usage { get; set; } = [];

    }
}
