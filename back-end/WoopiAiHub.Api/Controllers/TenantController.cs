using WoopiAiHub.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        [HttpGet("FindAllByUserEmail/{email}")]
        [SwaggerOperation("EndPoint that finds a tenants by an email")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAllByUserEmail(string email)
        {
            try
            {
                var result = await _tenantServices.FindAllByUserEmail(email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(TenantController)} in the {nameof(FindAllByUserEmail)} method.");
                return BadRequest("Error when searching for tenants in the Marketplace");
            }
        }

        /// <summary>
        /// Searches for the tenants that the user has enabled in the Marketplace.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("FindPlanByName/{tenant}")]
        [SwaggerOperation("EndPoint that finds a tenants by an email")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
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
                string keyMongoAccess = await _tenantServices.InitializeTenant(tenant);

                return Ok(keyMongoAccess);
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