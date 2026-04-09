using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentAnalysisRejectionController : ControllerBase
    {
        private readonly IDocumentAnalysisRejectionServices _services;

        public DocumentAnalysisRejectionController(
            IDocumentAnalysisRejectionServices services)
        {
            _services = services;
        }

        [HttpPost]
        [SwaggerOperation("Creates a document analysis rejection")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRejection(
            [FromBody] CreateDocumentAnalysisRejectionDto dto,
            [FromHeader] HeadersDto headersDto)
        {
            var result = await _services.CreateRejectionAsync(dto, headersDto.EmailCreator);
            return Ok(result);
        }

        [HttpPost("range")]
        [SwaggerOperation("Creates document analysis rejections for multiple cards (strict list, no batch expansion)")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRejectionRange(
            [FromBody] CreateDocumentAnalysisRejectionRangeDto dto,
            [FromHeader] HeadersDto headersDto)
        {
            var result = await _services.CreateRejectionRangeAsync(dto, headersDto.EmailCreator);
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("Retrieves document analysis rejections by card ID")]
        [ProducesResponseType(typeof(List<DocumentAnalysisRejectionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindRejections([FromQuery] int cardId)
        {
            var result = await _services.FindRejectionsByCardIdAsync(cardId);
            return Ok(result);
        }

        [HttpGet("WorkflowPreviousSteps/{workflowId}")]
        [SwaggerOperation("Retrieves workflow previous steps by workflow ID")]
        [ProducesResponseType(typeof(List<StepDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindWorkflowPreviousSteps(int workflowId, [FromQuery] int cardId)
        {
            var result = await _services.FindWorkflowPreviousStepsAsync(workflowId, cardId);
            return Ok(result);
        }
    }
}
