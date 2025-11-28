using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUsageUnitServices _usageUnitServices;
        public DashboardController(IUsageUnitServices usageUnitServices)
        {
            _usageUnitServices = usageUnitServices;
        }

        /// <summary>
        /// Returns usage units
        /// </summary>
        /// <returns></returns>
        [HttpGet("UsageUnits")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns usage units")]
        public async Task<IActionResult> FindAllUsageUnits()
        {
            var result = await _usageUnitServices.FindAllAsync();
            return Ok(result);
        }
    }
}
