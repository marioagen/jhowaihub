using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Application.Services;
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
            var result = await _userServices.Create(userCreateDto, headersDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to deactivate users from the database
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("DeactivateByIds")]
        [SwaggerOperation("Endpoint that receives the request to remove questions from the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeactivateByIds([FromBody] List<Guid> ids)
        {

            var result = await _userServices.DeactivateRange(ids);
            return Ok(result);
        }

        /// <summary>
        /// EndPoint that update an user by passing an UserUpdateDto
        /// </summary>
        /// <param name="UserUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a question by passing an UserUpdateDto")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] UserUpdateDto userUpdateDto,
                                                [FromHeader] HeadersDto headersDto)
        {
            var result = await _userServices.Update(userUpdateDto, headersDto);
            return Ok(result);
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

        /// <summary>
        /// Checks if an email is already in use.
        /// </summary>
        /// <param name="userEmailDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("IsEmailInUse")]
        [SwaggerOperation("Endpoint that receives the request to check if an email is already in use")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> IsEmailInUse([FromBody] UserEmailDto userEmailDto)
        {
            var result = await _userServices.IsEmailInUseAsync(userEmailDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to return team's users.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        [HttpGet("Team/{id}")]
        [SwaggerOperation("Endpoint that receives the request to return all team's users")]
        [ProducesResponseType(typeof(ICollection<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<UserDto>>> FindByTeamId(int id)
        {
            var result = await _userServices.FindByTeamId(id);
            return Ok(result);
        }

        [HttpPost("Team/Query")]
        [SwaggerOperation("Endpoint that receives the request to return all users from multiple teams")]
        [ProducesResponseType(typeof(ICollection<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ICollection<UserDto>>> FindByTeamIds([FromBody] FindByTeamIdsDto findByTeamIdsDto)
        {
            var result = await _userServices.FindByTeamIds(findByTeamIdsDto.TeamIds);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to return a user by its email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("{email}")]
        [SwaggerOperation("Endpoint that receive an email and return a valid user")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDto>> FindUserByEmail(string email)
        {
            var result = await _userServices.FindUserByEmail(email);
            return Ok(result);
        }
    }
}
