
namespace WoopiAiHub.Application.Utils
{
    public class OpenAiSettings
    {
        public int MaxToolCalls { get; set; }
        public double Temperature { get; set; }
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
    }
}
