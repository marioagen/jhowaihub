using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;

namespace WoopiAiHub.Api.Controllers
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
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation("Authenticates the user and returns the token if he has user permission")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var authData = await _accountServices.Login(loginDto);
            return Ok(authData);
        }

        /// <summary>
        /// Authenticates the user and returns the token if he has user permission
        /// </summary>
        /// <param name="authenticateDto"></param>
        /// <param name="authenticateHeaderDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login-sso")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation("Authenticates the user and returns the token if he has user permission")]
        public async Task<IActionResult> LoginSSO([FromHeader] AuthenticateHeaderDto authenticateHeaderDto, [FromBody] AuthenticateDto authenticateDto)
        {
            var authData = await _accountServices.LoginSSO(authenticateDto, authenticateHeaderDto);
            return Ok(authData);
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
            var tokenApi = _accountServices.AuthenticateApi(key);

            return Ok(tokenApi);
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
            var clientId = _accountServices.FindClientId();

            return Ok(clientId);
        }

        /// <summary>
        /// Refreshes the access token using the refresh token stored in the request cookies.
        /// </summary>
        /// <remarks>This method retrieves the refresh token from the request cookies and attempts to
        /// generate a new access token. If the refresh token is missing or invalid, the appropriate HTTP status code
        /// and error message are returned.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the new access token if the operation is successful, or an error
        /// response if the refresh token is missing or invalid.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return BadRequest("Refresh token missing.");

            var accessToken = await _accountServices.RefreshTokenAsync(refreshToken);

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Invalid refresh token.");

            return Ok(new { token = accessToken });
        }

        /// <summary>
        /// Logs out the current user by revoking their refresh token.
        /// </summary>
        /// <remarks>This method checks for the presence of a refresh token in the request cookies and
        /// revokes it  using the account services. If the refresh token is missing, a <see cref="BadRequestResult"/> 
        /// is returned. Otherwise, the result of the revocation is returned in an <see
        /// cref="OkObjectResult"/>.</remarks>
        /// <returns>An <see cref="IActionResult"/> indicating the outcome of the logout operation. Returns  <see
        /// cref="BadRequestResult"/> if the refresh token is missing, or <see cref="OkObjectResult"/>  with the
        /// revocation result if successful.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return BadRequest("Refresh token missing.");

            var result = await _accountServices.RevokeTokenAsync(refreshToken);

            return Ok(result);
        }
    }
}
