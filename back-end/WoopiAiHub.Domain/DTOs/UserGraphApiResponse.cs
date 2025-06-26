using Newtonsoft.Json;

namespace WoopiAiHub.Domain.DTOs
{
    public class UserGraphApiResponse
    {
        [JsonProperty("mail")]
        public string Mail { get; set; }
        
        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }
    }
}
