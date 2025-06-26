using WoopiAiHub.Domain.Utils;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WoopiAiHub.Domain.DTOs
{
    public class HeaderRequestDto
    {
        [FromHeader(Name = HeaderNames.XTenant)]
        [Required(ErrorMessage = "The Tenant header is required.")]
        public string? TenantName { get; set; }

        [FromHeader(Name = HeaderNames.XEmail)]
        public string? Email { get; set; }

        [FromHeader(Name = HeaderNames.ApiKey)]
        public string? ApiKey { get; set; }
    }
}
