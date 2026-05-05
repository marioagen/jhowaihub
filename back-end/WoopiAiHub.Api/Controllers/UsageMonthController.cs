using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsageMonthController : ControllerBase
    {
        private readonly IUsageMonthServices _usageMonthServices;

        public UsageMonthController(IUsageMonthServices usageMonthServices)
        {
            _usageMonthServices = usageMonthServices;
        }

        /// <summary>
        /// Endpoint that receives the request to list usage month by usage type
        /// </summary>
        /// <param name="usageType"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to list usage month by usage type")]
        [ProducesResponseType(typeof(ICollection<DashboardUsageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<DashboardUsageDto>>> FindByUsageType([FromQuery] UsageTypeFilterDto usageMonthFilterDto)
        {
            var result = await _usageMonthServices.FindDataByUsageType(usageMonthFilterDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to list usage month by model embeddings
        /// </summary>
        /// <param name="modelEmbeddingId"></param>
        /// <returns></returns>
        [HttpGet("FindByModel")]
        [SwaggerOperation("Endpoint that receives the request to list usage month by model embeddings")]
        [ProducesResponseType(typeof(ICollection<DashboardUsageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<DashboardUsageDto>>> FindByModelEmbedding([FromQuery] ModelEmbeddingFilterDto modelEmbeddingFilterDto)
        {
            var result = await _usageMonthServices.FindDataByModelEmbedding(modelEmbeddingFilterDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to list model embeddings
        /// </summary>
        /// <returns></returns>
        [HttpGet("FindUsedModels")]
        [SwaggerOperation("Endpoint that receives the request to list model embeddings")]
        [ProducesResponseType(typeof(ICollection<ModelEmbeddingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<ModelEmbeddingDto>>> FindUsedModelEmbeddings()
        {
            var result = await _usageMonthServices.FindUsedModelEmbeddings();
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to find total usage cost
        /// </summary>
        /// <param name="filterDto"></param>
        /// <returns></returns>
        [HttpGet("FindTotalUsageCost")]
        [SwaggerOperation("Endpoint that receives the request to find total usage cost")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> FindTotalUsageCost([FromQuery] TotalUsageCostFilterDto filterDto)
        {
            var result = await _usageMonthServices.FindTotalUsageCostAsync(filterDto);
            return Ok(result);
        }
    }
}
