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
    public class AnonymizationController(IAnonymizationServices AnonymizationServices) : ControllerBase
    {
        readonly IAnonymizationServices _AnonymizationServices = AnonymizationServices;

        /// <summary>
        /// Processes the anonymization of a document based on the specified request data.
        /// </summary>
        /// <param name="request">The request data containing information required to perform document anonymization.</param>
        /// <param name="headersDto">The headers containing metadata or authentication information for the request.</param>
        /// <returns>An IActionResult indicating the result of the anonymization process. Returns a status code 200 (OK) if the
        /// operation is successful.</returns>
        [HttpPost]
        [SwaggerOperation("Processes the anonymization of a document based on the specified request data.")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessAnonymization([FromBody] ProcessAnonymizationRequestDto request, [FromHeader] HeadersDto headersDto)
        {
            await _AnonymizationServices.ProcessAnonymization(request, headersDto);
            return Ok();
        }
    }
}
