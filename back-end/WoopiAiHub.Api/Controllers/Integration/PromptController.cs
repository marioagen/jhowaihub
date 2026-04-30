using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Api.Attributes;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers.Internal
{
    [KeyExternalAccessAuthorization]
    [Route("api/integration/[controller]")]
    [ApiController]
    public class PromptController(
        IPromptServices promptServices
    ) : ControllerBase
    {
        private readonly IPromptServices _promptServices = promptServices;

        /// <summary>
        /// Handles HTTP GET requests to retrieve all prompts in basic format.
        /// </summary>
        /// <returns>A collection of prompts.</returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all prompts in basic format")]
        [ProducesResponseType(typeof(ICollection<PromptIntegrationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll()
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
        public IActionResult FindById(int id)
        {
            var result = _promptServices.FindById(id);
            return Ok(result);
        }

        /// <summary>
        /// Create a new prompt from integration, this method is used by external clients to create a prompt in the database. It receives a PromptIntegrationCreateDto object and returns the created prompt when the operation is successful.
        /// </summary>
        /// <param name="promptIntegrationCreateDto"></param>
        /// <returns>The prompt created when success</returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a Prompt in the database")]
        [ProducesResponseType(typeof(PromptIntegrationDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(
            [FromBody] PromptIntegrationCreateDto promptIntegrationCreateDto,
            [FromHeader] HeadersDto headersDto)
        {

            var result = await _promptServices.CreateUniquePromptFromIntegration(
                            promptIntegrationCreateDto,
                            headersDto.EmailCreator);
            return Ok(result);
        }
    }
}
