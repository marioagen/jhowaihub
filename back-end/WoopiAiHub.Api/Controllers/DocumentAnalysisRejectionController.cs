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

        /// <summary>
        /// Creates a document analysis rejection for the specified card.
        /// </summary>
        /// <param name="request">Rejection payload (justification, card id, step id).</param>
        /// <param name="headersDto">Request headers including the creator email.</param>
        /// <returns><see langword="true"/> if the rejection was created successfully.</returns>
        [HttpPost]
        [SwaggerOperation("Creates a document analysis rejection")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRejection(
            [FromBody] CreateDocumentAnalysisRejectionDto request,
            [FromHeader] HeadersDto headersDto)
        {
            var result = await _services.CreateRejectionAsync(request, headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Creates document analysis rejections for multiple cards using the supplied card id list (no batch expansion).
        /// </summary>
        /// <param name="request">Range rejection payload (justification, step id, card ids, optional assign user).</param>
        /// <param name="headersDto">Request headers including the creator email.</param>
        /// <returns><see langword="true"/> if the rejections were created successfully.</returns>
        [HttpPost("Range")]
        [SwaggerOperation("Creates document analysis rejections for multiple cards (strict list, no batch expansion)")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRejectionRange(
            [FromBody] CreateDocumentAnalysisRejectionRangeDto request,
            [FromHeader] HeadersDto headersDto)
        {
            var result = await _services.CreateRejectionRangeAsync(request, headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all document analysis rejections for the specified card.
        /// </summary>
        /// <param name="cardId">Card identifier.</param>
        /// <returns>A list of rejections; empty when none exist.</returns>
        [HttpGet]
        [SwaggerOperation("Retrieves document analysis rejections by card ID")]
        [ProducesResponseType(typeof(List<DocumentAnalysisRejectionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindRejections([FromQuery] int cardId)
        {
            var result = await _services.FindRejectionsByCardIdAsync(cardId);
            return Ok(result);
        }

        /// <summary>
        /// Returns workflow steps that occur before the current step of the given card, ordered by step order.
        /// </summary>
        /// <param name="workflowId">Workflow identifier.</param>
        /// <param name="cardId">Card whose current step determines which previous steps are returned.</param>
        /// <returns>Previous steps in the workflow relative to the card's step.</returns>
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
