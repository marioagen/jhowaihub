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
    public class StatusController(IStatusServices statusServices) : ControllerBase
    {
        private readonly IStatusServices _statusServices = statusServices;

        /// <summary>
        /// Retrieves all status entries available in the system.
        /// </summary>
        /// <remarks>This asynchronous method returns an empty collection if no statuses are found. Use
        /// this endpoint when a complete list of status information is required.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="StatusDto"/> objects. Returns a 200 OK
        /// response with the collection if successful.</returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all status")]
        [ProducesResponseType(typeof(ICollection<StatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll()
        {
            var result = await _statusServices.FindAll();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the current status of all workflow steps.
        /// </summary>
        /// <remarks>This asynchronous endpoint returns a 200 OK response with the status data. Use this
        /// method to obtain up-to-date information about the progress or state of workflow steps in scenarios where
        /// monitoring or reporting is required.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="StatusDto"/> objects that represent the
        /// statuses of each workflow step.</returns>
        [HttpGet("Steps")]
        [SwaggerOperation("Endpoint that receives the request to return all status")]
        [ProducesResponseType(typeof(ICollection<StatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindStatusForWorkflowSteps()
        {
            var result = await _statusServices.FindStatusForWorkflowSteps();
            return Ok(result);
        }
    }
}
