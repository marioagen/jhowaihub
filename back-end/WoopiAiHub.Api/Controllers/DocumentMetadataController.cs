using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentMetadataController : ControllerBase
    {
        private const string PdfContentType = "application/pdf";
        private readonly IDocumentMetadataServices _documentMetadataServices;

        public DocumentMetadataController(IDocumentMetadataServices documentMetadataServices)
        {
            _documentMetadataServices = documentMetadataServices;
        }
    }
}