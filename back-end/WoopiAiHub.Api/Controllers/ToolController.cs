using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly ILogger<TypeDocController> _logger;
        //private readonly IToolServices _toolServices;

        public ToolController(ILogger<TypeDocController> logger)
        {
            _logger = logger;
            //IToolServices toolServices
            //_toolServices = toolServices;
        }

        /// <summary>
        /// Endpoint that receives the request to return all tools paginated
        /// </summary>
        /// <param name="typeDocPagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all tools paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FindAllPaged()
        {
            return Ok();
        }

        /// <summary>
        /// Endpoint that receives the request to return tool types
        /// </summary>
        /// <param name="typeDocPagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Types")]
        [SwaggerOperation("Endpoint that receives the request to return tool types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FindAllToolTypes()
        {
            return Ok();
        }

        /// <summary>
        /// Endpoint that receives the request to return tool data
        /// </summary>
        /// <param name="typeDocPagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Data")]
        [SwaggerOperation("Endpoint that receives the request to return tool data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FindAllToolData()
        {
            return Ok();
        }

        /// <summary>
        /// Endpoint that receives the request to create a tool in the database
        /// </summary>
        /// <param name="toolCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a tool in the database")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Create()
        {
            return Ok();
        }

        /// <summary>
        /// EndPoint that update a tool
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a tool")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Update()
        {
            return Ok();
        }

        /// <summary>
        /// EndPoint that delete tools by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("Delete")]
        [SwaggerOperation("EndPoint that delete tools by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteByIds()
        {
            return Ok();
        }
    }
}
