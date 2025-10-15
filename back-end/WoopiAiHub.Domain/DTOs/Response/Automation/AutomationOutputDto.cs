using Newtonsoft.Json.Linq;
using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.DTOs.Response.Automation
{
    public class AutomationOutputDto
    {
        public string? Tenant { get; set; }
        public string? Email { get; set; }
        public int ExecutionId { get; set; }
        public object? Content { get; set; }
    }
}
