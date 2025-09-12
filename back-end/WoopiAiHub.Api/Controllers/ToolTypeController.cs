using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ToolTypeController : ControllerBase
    {
        private readonly IToolTypeServices _toolTypeServices;
        public ToolTypeController(IToolTypeServices toolTypeServices)
        {
            _toolTypeServices = toolTypeServices;
        }

        /// <summary>
        /// Retrieves all available tool types.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all tool types")]
        [ProducesResponseType(typeof(ICollection<ToolTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll()
        {
            var result = await _toolTypeServices.FindAllAsync();
            return Ok(result);
        }
    }
}
