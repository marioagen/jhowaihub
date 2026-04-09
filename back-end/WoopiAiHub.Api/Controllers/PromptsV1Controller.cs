using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/prompts")]
    [ApiController]
    public class PromptsV1Controller : ControllerBase
    {
        private readonly IPromptServices _promptServices;

        public PromptsV1Controller(IPromptServices promptServices)
        {
            _promptServices = promptServices;
        }

        /// <summary>
        /// Executes the given prompt against optional context via AI Gateway (synchronous). Token usage is recorded; nothing is persisted.
        /// </summary>
        [HttpPost("test")]
        [SwaggerOperation("Tests a prompt with context using AI Gateway and returns the model output")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Test(
            [FromBody] PromptTestRequestDto? request,
            [FromHeader] HeadersDto headersDto)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var result = await _promptServices.TestPromptWithContextAsync(
                request.PromptText ?? string.Empty,
                request.ContextText ?? string.Empty,
                headersDto.Tenant,
                headersDto.EmailCreator);
            return Ok(result);
        }
    }
}
