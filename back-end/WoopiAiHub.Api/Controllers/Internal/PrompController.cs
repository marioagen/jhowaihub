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
        /// Handles HTTP GET requests to retrieve all prompts in basic format.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all prompts in basic format")]
        [ProducesResponseType(typeof(ICollection<PromptBaseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAllInternal()
        {
            var result = await _promptServices.FindAllBasic();
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
        public IActionResult FindInternal(int id)
        {
            var result = _promptServices.FindById(id);
            return Ok(result);
        }
    }
}
