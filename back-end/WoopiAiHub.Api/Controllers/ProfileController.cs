using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileServices _profileServices;

        public ProfileController(IProfileServices profileServices)
        {
            _profileServices = profileServices;
        }


        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all profiles paginated")]
        [ProducesResponseType(typeof(ProfilePagedResultDto), StatusCodes.Status200OK)]
        public ActionResult<ProfilePagedResultDto> FindAllPaged([FromQuery] PagedDataDto pagedDataDto)
        {
            var result = _profileServices.FindAllPaged(pagedDataDto);
            return Ok(result);
        }
    }
}
