using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ICardServices _cardServices;
        public CardController(ICardServices cardServices)
        {
            _cardServices = cardServices;
        }

        /// <summary>
        /// Updates the step and status of a card
        /// </summary>
        /// <param name="updateCardStepStatusDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a step and status of card")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto)
        {
            var result = await _cardServices.UpdateStepAndStatus(updateCardStepStatusDto);
            return Ok(result);
        }
    }
}
