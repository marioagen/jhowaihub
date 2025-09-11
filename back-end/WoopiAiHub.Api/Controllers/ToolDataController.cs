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
    public class ToolDataController : ControllerBase
    {
        private readonly IToolDataServices _toolDataServices;

        public ToolDataController(IToolDataServices toolDataServices)
        {
            _toolDataServices = toolDataServices;
        }

        /// <summary>
        /// Retrieves all available tool's data.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all tool's data")]
        [ProducesResponseType(typeof(ICollection<ToolDataDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll()
        {
            var result = await _toolDataServices.FindAllAsync();
            return Ok(result);
        }
    }
}
