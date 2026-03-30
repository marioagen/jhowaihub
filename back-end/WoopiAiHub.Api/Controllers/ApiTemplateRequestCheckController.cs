using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestCheck;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTemplateRequestCheckController(IApiTemplateRequestCheckHandler apiTemplateRequestCheckHandler) : ControllerBase
    {
        private readonly IApiTemplateRequestCheckHandler _apiTemplateRequestCheckHandler = apiTemplateRequestCheckHandler;

        /// <summary>
        /// Executes a simulated HTTP call from a template definition plus variable values. Send <see cref="ApiTemplateRequestCheckRequestDto.Draft"/>
        /// (current editor state) or <see cref="ApiTemplateRequestCheckRequestDto.TemplateId"/> to load from storage. The ASP.NET response is always 200 OK;
        /// the downstream HTTP status is in <see cref="ApiTemplateRequestCheckResponseDto.StatusCode"/>.
        /// </summary>
        [HttpPost("execute")]
        [SwaggerOperation(Summary = "Proxy HTTP genérico para testes de pedido de template de API")]
        [ProducesResponseType(typeof(ApiTemplateRequestCheckResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Execute([FromBody] ApiTemplateRequestCheckRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _apiTemplateRequestCheckHandler.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
