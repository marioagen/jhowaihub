using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTemplateRequestTestsController(IApiTemplateRequestTestsHandler apiTemplateRequestTestsHandler) : ControllerBase
    {
        private readonly IApiTemplateRequestTestsHandler _apiTemplateRequestTestsHandler = apiTemplateRequestTestsHandler;

        /// <summary>
        /// Executes a simulated HTTP call from a template definition plus variable values. Send <see cref="ApiTemplateRequestTestsRequestDto.Draft"/>
        /// (current editor state) or <see cref="ApiTemplateRequestTestsRequestDto.TemplateId"/> to load from storage. The ASP.NET response is always 200 OK;
        /// the downstream HTTP status is in <see cref="ApiTemplateRequestTestsResponseDto.StatusCode"/>.
        /// </summary>
        [HttpPost("execute")]
        [SwaggerOperation(Summary = "Proxy HTTP genérico para testes de pedido de template de API")]
        [ProducesResponseType(typeof(ApiTemplateRequestTestsResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Execute([FromBody] ApiTemplateRequestTestsRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _apiTemplateRequestTestsHandler.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
