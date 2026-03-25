using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.ThirdParty;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/third-party")]
    [ApiController]
    public class ThirdPartyHttpController(IThirdPartyApiHandler thirdPartyApiHandler) : ControllerBase
    {
        private readonly IThirdPartyApiHandler _thirdPartyApiHandler = thirdPartyApiHandler;

        /// <summary>
        /// Executes a generic HTTP call to a third-party URL. The ASP.NET response is always 200 OK;
        /// the downstream HTTP status is in <see cref="ThirdPartyApiResponseDto.StatusCode"/>.
        /// </summary>
        [HttpPost("execute")]
        [SwaggerOperation(Summary = "Proxy HTTP genérico para APIs de terceiros")]
        [ProducesResponseType(typeof(ThirdPartyApiResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Execute([FromBody] ThirdPartyApiRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _thirdPartyApiHandler.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
