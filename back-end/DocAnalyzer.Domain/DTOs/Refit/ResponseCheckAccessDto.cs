
namespace DocAnalyzer.Domain.DTOs.Refit
{
    public class ResponseCheckAccessDto
    {
        public bool HasAccess { get; set; }
        public string Tenant { get; set; } = string.Empty;
    }
}
