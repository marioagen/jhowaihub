using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
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
        [SwaggerOperation("Endpoint that receives the request to create a user in the database")]
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
                _logger.LogError(ex, $"An exception occurred in the {nameof(UserController)} in the {nameof(Create)} method");
                return BadRequest("Error when creating user: " + ex);
            }

        }

        /// <summary>
        /// Endpoint that receives the request to remove users from the database
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("DeactivateByEmails")]
        [SwaggerOperation("Endpoint that receives the request to remove questions from the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task <IActionResult> DeactivateByEmails([FromBody] List<Guid> ids)
        {
            try
            {
                var result = await _userServices.DeactivateRange(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(DeactivateByEmails)} method");
                return BadRequest("Error while deleting question: " + ex);
            }
        }

        /// <summary>
        /// EndPoint that update an user by passing an UserUpdateDto
        /// </summary>
        /// <param name="UserUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a question by passing an UpdateQuestionDto")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody]UserUpdateDto UserUpdateDto,
                                                [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = await _userServices.Update(userUpdateDto, headersDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(UserController)} in the {nameof(Update)} method");
                return Conflict(new { message = "Duplicated user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(UserController)} in the {nameof(Update)} method");
                return BadRequest("Error while updating user: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all users paginated.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all teams paginated")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public ActionResult<UserPagedResultDto> FindAllPaged([FromQuery] PagedDataDto pagedDataDto)
        {
            var result = _userServices.FindAllPaged(pagedDataDto);
            return Ok(result);
        }

    }
}
