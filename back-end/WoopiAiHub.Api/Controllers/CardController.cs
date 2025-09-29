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
        public async Task<IActionResult> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto,
                                                             [FromHeader] HeadersDto headersDto)
        {
            var result = await _cardServices.UpdateStepAndStatus(updateCardStepStatusDto, headersDto.Tenant, headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Updates the card assigning user
        /// </summary>
        /// <param name="updateAssignedUserDto"></param>
        /// <returns></returns>
        [HttpPut("AssignUser")]
        [SwaggerOperation("EndPoint that update assinged user of card")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignUser(UpdateAssignedUserDto updateAssignedUserDto)
        {
            var result = await _cardServices.AssignUser(updateAssignedUserDto);
            return Ok(result);
        }

        /// <summary>
        /// Updates the card unassigning user
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [HttpPut("UnassignUser/{cardId}")]
        [SwaggerOperation("EndPoint that update assinged user of card")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnassignUser(int cardId)
        {
            var result = await _cardServices.UnassignUser(cardId);
            return Ok(result);
        }
    }
}
