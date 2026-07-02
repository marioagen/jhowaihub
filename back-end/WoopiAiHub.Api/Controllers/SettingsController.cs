using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ILlmModelsSettingsServices _llmModelsSettingsServices;
        private readonly ICurrentUserService _currentUserService;

        public SettingsController(
            ILlmModelsSettingsServices llmModelsSettingsServices,
            ICurrentUserService currentUserService)
        {
            _llmModelsSettingsServices = llmModelsSettingsServices;
            _currentUserService = currentUserService;
        }

        [HttpGet("llm-models")]
        [SwaggerOperation("Returns effective LLM model settings and available models for the tenant.")]
        [ProducesResponseType(typeof(LlmModelsSettingsResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<LlmModelsSettingsResponseDto>> GetLlmModels([FromHeader] HeadersDto headersDto)
        {
            var response = await _llmModelsSettingsServices.GetAsync(
                headersDto.Tenant,
                _currentUserService.IsAdmin);
            return Ok(response);
        }

        [HttpPut("llm-models")]
        [SwaggerOperation("Updates tenant LLM model settings by scope. Requires admin user.")]
        [ProducesResponseType(typeof(LlmModelsSettingsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LlmModelsSettingsResponseDto>> PutLlmModels(
            [FromBody] UpdateLlmModelsSettingsDto request,
            [FromHeader] HeadersDto headersDto)
        {
            if (!_currentUserService.IsAdmin)
            {
                return Forbid();
            }

            var response = await _llmModelsSettingsServices.UpdateAsync(
                headersDto.Tenant,
                headersDto.EmailCreator,
                request);
            return Ok(response);
        }
    }
}
