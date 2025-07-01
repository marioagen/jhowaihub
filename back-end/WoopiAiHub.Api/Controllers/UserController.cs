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
        public IActionResult Create([FromQuery] UserCreateDto userCreateDto,
                                    [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _userServices.Create(userCreateDto,
                                                  headersDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(Create)} method");
                return BadRequest("Error when creating user: " + ex);
            }

        }
    }
}
