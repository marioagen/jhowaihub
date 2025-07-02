using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly ILogger<UserController> _logger;


        public UserController(IUserServices userServices,
                              ILogger<UserController> logger)
        {
            _userServices = userServices;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint that receives the request to create an user in the database
        /// </summary>
        /// <param name="userCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a question in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] UserCreateDto userCreateDto,
                                                [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = await _userServices.Create(userCreateDto,
                                                  headersDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(Create)} method");
                return BadRequest("Error when creating user: " + ex);
            }

        }

        /// <summary>
        /// Endpoint that receives the request to remove questions from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteByIds")]
        [SwaggerOperation("Endpoint that receives the request to remove questions from the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult DeleteByIds(List<Guid> ids)
        {
            try
            {
                var result = _userServices.DeleteByIds(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(DeleteByIds)} method");
                return BadRequest("Error while deleting question: " + ex);
            }
        }

        /// <summary>
        /// EndPoint that update a question by passing an UpdateQuestionDto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a question by passing an UpdateQuestionDto")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Update(QuestionUpdateDto updatequestionDto)
        {
            try
            {
                var result = _userServices.Update(updatequestionDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionController)} in the {nameof(Update)} method");
                return Conflict(new { message = "Duplicated questions" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(Update)} method");
                return BadRequest("Error while updating question: " + ex);
            }
        }

    }
}
