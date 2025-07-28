using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WoopiAiHub.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {

        private readonly IPermissionServices _permissionServices;

        public PermissionController(IPermissionServices permissionServices)
        {
            _permissionServices = permissionServices;
        }

        /// <summary>
        /// Endpoint that receives the request to return all permissions paginated.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("FindAll")]
        [SwaggerOperation("Endpoint that receives the request to return all permissions paginated")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public IActionResult FindAll()
        {
            var result = _permissionServices.FindAll();
            return Ok(result);
        }
    }
}
