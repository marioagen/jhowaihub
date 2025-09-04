using WoopiAiHub.Domain.Utils;
using Microsoft.AspNetCore.Mvc;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class HeadersDto
    {
        [FromHeader(Name = HeaderNames.XEmail)]
        public string EmailCreator { get; set; } = string.Empty;

        [FromHeader(Name = HeaderNames.XTenant)]
        public string Tenant { get; set; } = string.Empty;

        [FromHeader(Name = HeaderNames.XKeyMongoAccess)]
        public string KeyMongoAccess { get; set; } = string.Empty;

        [FromHeader(Name = HeaderNames.XLanguage)]
        public string Language { get; set; } = string.Empty;
    }
}
