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
    public class TenantController : ControllerBase
    {
        private readonly ITenantServices _tenantServices;
        private readonly ILogger<TenantController> _logger;

        public TenantController(ITenantServices tenantServices,
                                ILogger<TenantController> logger)
        {
            _tenantServices = tenantServices;
            _logger = logger;
        }

        /// <summary>
        /// Searches for the tenants that the user has enabled in the Marketplace.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("FindPlanByName/{tenant}")]
        [SwaggerOperation("EndPoint that finds a tenants by an email")]
        [ProducesResponseType(typeof(DashboardTenantInfo), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindPlanByName(string tenant)
        {
            var result = await _tenantServices.FindPlanByName(tenant);
            return Ok(result);
        }

        /// <summary>
        /// Obtains the tenant's mongo key and applies migrations if necessary
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        [HttpGet("InitializeTenant/{tenant}")]
        [SwaggerOperation("EndPoint that initialize a tenant")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> InitializeTenant(string tenant)
        {
            try
            {
                await _tenantServices.InitializeTenant(tenant);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing tenant '{Tenant}' in {Controller}.{Method}.",
                                 tenant, nameof(TenantController), nameof(InitializeTenant));
                return BadRequest($"Failed to initialize tenant '{tenant}'. Please check the tenant identifier and try again.");
            }
        }
    }
}
