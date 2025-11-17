using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardServices _dashboardServices;
        private readonly ILogger<AccountController> _logger;
        public DashboardController(IDashboardServices dashboardServices,
                                 ILogger<AccountController> logger)
        {
            _dashboardServices = dashboardServices;
            _logger = logger;
        }

        /// <summary>
        /// Returns dashboard basic data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns dashboard basic data")]
        public IActionResult FindDashboardData()
        {
            var result = _dashboardServices.FindDashboardData();
            return Ok(result);
        }

        /// <summary>
        /// Returns dashboard tokens data
        /// </summary>
        /// <returns></returns>
        [HttpGet("Tokens")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns dashboard tokens data")]
        public IActionResult FindTokensData()
        {
            var result = _dashboardServices.FindTokensData();
            return Ok(result);
        }

        /// <summary>
        /// Returns dashboard ocrs data
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Ocr")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns dashboard ocr data")]
        public IActionResult FindOcrData()
        {
            var result = _dashboardServices.FindOCRData();
            return Ok(result);
        }

        /// <summary>
        /// Returns dashboard workflows data
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Workflows")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns dashboard workflows data")]
        public IActionResult FindWorkflowsData()
        {
            var result = _dashboardServices.FindWorkflowsData();
            return Ok(result);
        }
    }
}
