using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiPromptLibBackEnd.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class PromptController : ControllerBase
    {
        private readonly IPromptServices _promptServices;
        private readonly IValidatePrompt _validatePrompt;

        public PromptController(IPromptServices promptServices,
                                IValidatePrompt validatePrompt)
        {
            _promptServices = promptServices;
            _validatePrompt = validatePrompt;
        }

        /// <summary>
        /// Create a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a Prompt in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]

        public IActionResult Create([FromBody] PromptCreateDto promptCreateDto,
                                    [FromHeader] HeadersDto headersDto)
        {
            var result = _promptServices.CreateUniquePrompt(promptCreateDto, headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Update a prompt
        /// </summary>
        /// <param name="promptUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("Endpoint that receives the request to update a Prompt in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Update([FromBody] PromptUpdateDto promptUpdateDto,
                                    [FromHeader] HeadersDto headersDto)
        {
            var result = _promptServices.Update(promptUpdateDto,
                                                headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Return prompts paginated selected by logged-in user 
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PagedByUser")]
        [SwaggerOperation("Endpoint that receives an email and returns its prompts paginated")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public IActionResult FindByIdUserPaged([FromQuery] PagedDataDto pagedDataDto,
                                               [FromHeader] HeadersDto headersDto)
        {
            var result = _promptServices.FindByIdUserPaged(pagedDataDto,
                                                           headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Return prompts selected by id
        /// </summary>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("Endpoint that receive an id and return a valid prompt")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public IActionResult FindById(int id)
        {
            var result = _promptServices.FindById(id);
            return Ok(result);
        }

        /// <summary>
        /// Return all prompts paginated
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all prompts paginated")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] PagedDataDto pagedDataDto,
                                          [FromHeader] HeadersDto headersDto)
        {
            var result = _promptServices.FindAllPaged(pagedDataDto,
                                                      headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Delete list of prompts
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("DeleteByIds")]
        [SwaggerOperation("Endpoint that receives the request to remove prompts from the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Delete(List<int> ids)
        {
            var result = _promptServices.DeleteByIds(ids);
            return Ok(result);
        }

        /// <summary>
        /// Receives the request to verify that the user is the owner of the prompt
        /// </summary>
        /// <param name="id"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet("{id}/validate-ownership")]
        [SwaggerOperation("Validates whether the current user is the owner of the prompt")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult ValidateOwnership(int id,
                                               [FromHeader] HeadersDto headersDto)
        {
            _validatePrompt.ValidateOwnership(id,
                                              headersDto.EmailCreator);
            return NoContent();
        }

        /// <summary>
        /// Return all prompts
        /// </summary>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all prompts")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public IActionResult FindAllByEmail([FromHeader] HeadersDto headersDto)
        {
            var result = _promptServices.FindAll(headersDto.EmailCreator);
            return Ok(result);
        }
    }
}
