using Microsoft.AspNetCore.Mvc;
using DocAnalyzer.Domain.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using DocAnalyzer.Domain.DTOs.Request;

namespace DocAnalyzer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountServices _accountServices;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountServices accountServices,
                                 ILogger<AccountController> logger)
        {
            _accountServices = accountServices;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates the user and returns the token if he has user permission
        /// </summary>
        /// <param name="authenticateDto"></param>
        /// <param name="authenticateHeaderDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("authenticate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation("Authenticates the user and returns the token if he has user permission")]
        public async Task<IActionResult> Authenticate([FromHeader] AuthenticateHeaderDto authenticateHeaderDto,
                                                      [FromBody] AuthenticateDto authenticateDto)
        {
            try
            {
                var authData = await _accountServices.Authenticate(authenticateDto,
                                                                   authenticateHeaderDto);

                return Ok(authData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An exception occurred in {Controller} in the {Method} method. Login: {LoginSanitized}",
                    nameof(AccountController), nameof(Authenticate), authenticateDto.Login?.Replace('\n', '_').Replace('\r', '_'));
                return Unauthorized();
            }
        }

        /// <summary>
        /// Returns an access token if the internal key passed is valid
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost("authenticateApi")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation("Returns an access token if the internal key passed is valid")]
        public IActionResult AuthenticateApi(string key)
        {
            try
            {
                var tokenApi = _accountServices.AuthenticateApi(key);

                return Ok(tokenApi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AccountController)} in the {nameof(AuthenticateApi)} method.");
                return Unauthorized();
            }
        }

        /// <summary>
        /// Returns a client id
        /// </summary>
        /// <returns></returns>
        [HttpGet("clientId")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns a client id")]
        public IActionResult FindClientId()
        {
            try
            {
                var clientId = _accountServices.FindClientId();

                return Ok(clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AccountController)} in the {nameof(FindClientId)} method.");
                return Unauthorized();
            }
        }
    }
}
