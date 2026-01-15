using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Api.Attributes;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers.Internal
{
    [KeyExternalAccessAuthorization]
    [Route("api/internal/[controller]")]
    [ApiController]
    public class PromptController(
        IPromptServices promptServices
    ) : ControllerBase
    {
        private readonly IPromptServices _promptServices = promptServices;

        /// <summary>
        /// Handles HTTP GET requests to retrieve all internal prompts.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all internal prompts")]
        [ProducesResponseType(typeof(ICollection<PromptDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllInternal()
        {
            var result = await _promptServices.FindAllInternal();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the prompt associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the prompt to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("Endpoint that receives a prompt Id to return the correspondent prompt")]
        [ProducesResponseType(typeof(PromptDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInternal(int id)
        {
            var result = await _promptServices.FindInternalById(id);
            return Ok(result);
        }
    }
}
