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
    public class StatusController : ControllerBase
    {
        private readonly IStatusServices _statusServices;

        public StatusController(IStatusServices statusServices)
        {
            _statusServices = statusServices;
        }

        [HttpGet("FindAll")]
        [SwaggerOperation("Endpoint that receives the request to return all status")]
        [ProducesResponseType(typeof(ICollection<StatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll()
        {
            var result = await _statusServices.FindAll();
            return Ok(result);
        }
    }
}
