
namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class ResponseCheckAccessDto
    {
        public bool HasAccess { get; set; }
        public ICollection<string> Tenants { get; set; } = [];
    }
}
