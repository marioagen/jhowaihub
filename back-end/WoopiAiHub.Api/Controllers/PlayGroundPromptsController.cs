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
    public class PlayGroundPromptsController : ControllerBase
    {
        private readonly IPlaygroundServices _playgroundServices;

        public PlayGroundPromptsController(IPlaygroundServices playgroundServices)
        {
            _playgroundServices = playgroundServices;
        }

        /// <summary>
        /// Executes the given prompt against optional context via AI Gateway (synchronous). Token usage is recorded; nothing is persisted.
        /// </summary>
        [HttpPost("test")]
        [SwaggerOperation("Tests a prompt with context using AI Gateway and returns the model output")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Test(
            [FromBody] PromptTestRequestDto request,
            [FromHeader] HeadersDto headersDto)
        {
            var result = await _playgroundServices.TestPromptWithContextAsync(
                request.PromptText,
                request.ContextText,
                headersDto.Tenant,
                headersDto.EmailCreator);
            return Ok(result);
        }
    }
}
