using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
        [HttpGet("{usageType}")]
        [SwaggerOperation("Endpoint that receives the request to list usage month by usage type")]
        [ProducesResponseType(typeof(ICollection<DashboardUsageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<DashboardUsageDto>>> FindByUsageType(ColTypeUsage usageType)
        {
            var result = await _usageMonthServices.FindDataByUsageType(usageType);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to list usage month by model embeddings
        /// </summary>
        /// <param name="modelEmbeddingId"></param>
        /// <returns></returns>
        [HttpGet("FindByModel/{modelEmbeddingId}")]
        [SwaggerOperation("Endpoint that receives the request to list usage month by model embeddings")]
        [ProducesResponseType(typeof(ICollection<DashboardUsageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<DashboardUsageDto>>> FindByModelEmbedding(int modelEmbeddingId)
        {
            var result = await _usageMonthServices.FindDataByModelEmbedding(modelEmbeddingId);
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
    }
}
