using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Api.Attributes;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnonymizationController(IAnonymizationServices anonymizationServices) : ControllerBase
    {
        readonly IAnonymizationServices _anonymizationServices = anonymizationServices;

        /// <summary>
        /// Processes the anonymization of a document based on the specified request data.
        /// </summary>
        /// <param name="request">The request data containing information required to perform document anonymization.</param>
        /// <param name="headersDto">The headers containing metadata or authentication information for the request.</param>
        /// <returns>An IActionResult indicating the result of the anonymization process. Returns a status code 200 (OK) if the
        /// operation is successful.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [SwaggerOperation("Processes the anonymization of a document based on the specified request data.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessAnonymization([FromBody] ProcessAnonymizationRequestDto request, [FromHeader] HeadersDto headersDto)
        {
            await _anonymizationServices.ProcessAnonymization(request, headersDto);
            return Ok();
        }

        /// <summary>
        /// Webhook to processes the anonymization result for a document using the specified request data.
        /// </summary>
        /// <param name="result">An object containing the anonymization result data to be processed. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [HttpPost("ready")]
        [KeyExternalAccessAuthorization]
        [SwaggerOperation("Webhook to processes the anonymization result for a document using the specified request data.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task AnonymizationResult([FromBody] AnonymizationResultDto result)
        {
            await _anonymizationServices.ProcessAnonymizationResult(result);
        }

        /// <summary>
        /// Finds all anonymized documents associated with the specified document identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document for which to retrieve associated anonymized documents.</param>
        /// <returns>An IActionResult containing a collection of anonymized documents related to the specified document. Returns
        /// a 200 OK response with the result.</returns>
        [HttpGet("document/{documentId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [SwaggerOperation("Finds all anonymized documents associated with the specified document identifier.")]
        [ProducesResponseType(typeof(ICollection<DocumentAnonymizationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAnonymizedDocumentsByDocument([FromRoute] int documentId)
        {
            var result = await _anonymizationServices.FindAnonymizedDocumentsByDocument(documentId);
            return Ok(result);
        }
    }
}
