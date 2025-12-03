using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUsageUnitServices _usageUnitServices;
        private readonly IUsageAggregationService _usageAggregationService;

        public DashboardController(IUsageUnitServices usageUnitServices, 
                                   IUsageAggregationService usageAggregationService)
        {
            _usageUnitServices = usageUnitServices;
            _usageAggregationService = usageAggregationService;
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

        /// <summary>
        /// Update usage month data
        /// </summary>
        /// <returns></returns>
        [HttpPut("ProcessMetricsByTenant")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [SwaggerOperation("Update usage month data")]
        public async Task<IActionResult> ProcessUnprocessedUsageByTenantAsync([FromHeader] HeadersDto headersDto)
        {
            await _usageAggregationService.ProcessUnprocessedUsageByTenantAsync(headersDto.Tenant);
            return Ok(true);
        }
    }
}
