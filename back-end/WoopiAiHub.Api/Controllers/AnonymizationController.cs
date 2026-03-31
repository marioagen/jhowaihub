using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        [AllowAnonymous]
        [SwaggerOperation("Webhook to processes the anonymization result for a document using the specified request data.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task AnonymizationResult([FromBody] AnonymizationResultDto result)
        {
            await _anonymizationServices.ProcessAnonymizationResult(result);
        }
    }
}
