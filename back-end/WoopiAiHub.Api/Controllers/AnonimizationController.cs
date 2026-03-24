using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AnonimizationController(IAnonimizationServices anonimizationServices) : ControllerBase
    {
        readonly IAnonimizationServices _anonimizationServices = anonimizationServices;

        /// <summary>
        /// Processes the anonymization of a document identified by the specified document ID.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to be anonymized. Must be a valid, existing document ID.</param>
        /// <param name="headersDto">The headers containing additional request metadata required for processing the anonymization.</param>
        /// <returns>An IActionResult containing the result of the anonymization process.</returns>
        [HttpPost("document/{documentId}")]
        [SwaggerOperation("Processes the anonymization of a document identified by the specified document ID")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessAnonimization([FromRoute] int documentId, [FromHeader] HeadersDto headersDto)
        {
            var result = await _anonimizationServices.ProcessAnonimization(documentId, headersDto);
            return Ok(result);
        }
    }
}
