using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
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
            var result = await _cardServices.UpdateStepAndStatus(updateCardStepStatusDto,
                headersDto.Tenant,
                headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Updates only the status of a card, keeping the same step.
        /// </summary>
        /// <param name="updateCardStatusDto"></param>
        /// <returns></returns>
        [HttpPut("UpdateStatus")]
        [SwaggerOperation("Endpoint that updates only the card status")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(UpdateCardStatusDto updateCardStatusDto)
        {
            var result = await _cardServices.UpdateStatus(updateCardStatusDto);
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

        /// <summary>
        /// Assigns a user to multiple cards (one assignment per distinct card id).
        /// </summary>
        /// <param name="request">User id and card ids to assign.</param>
        /// <returns><see langword="true"/> if the assignment completed successfully.</returns>
        [HttpPut("AssignRange")]
        [SwaggerOperation("Assigns a user to multiple cards (AssignRange per distinct card id)")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRange([FromBody] AssignRangeDto request)
        {
            var result = await _cardServices.AssignRangeAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// It receives a card id and returns document information grouped by processing steps.
        /// </summary>
        /// <param name="id">Card ID</param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet("AnalyzeSteps/{id}")]
        [SwaggerOperation(
            "It receives a card id and returns a DocumentAnalyzeStepsDto with the document's information grouped by steps")]
        [ProducesResponseType(typeof(DocumentAnalyzeStepsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByIdAnalyzeWithSteps(int id,
            [FromHeader] HeadersDto headersDto)
        {
            var result = await _cardServices.FindByIdAnalyzeWithSteps(id,
                headersDto);
            return Ok(result);
        }

        /// <summary>
        /// It receives a card id and returns header information (card name, workflow name).
        /// </summary>
        /// <param name="id">Card ID</param>
        /// <returns></returns>
        [HttpGet("HeaderInfo/{id}")]
        [SwaggerOperation("It receives a card id and returns a CardHeaderDto with the card and workflow names")]
        [ProducesResponseType(typeof(CardHeaderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindHeaderInfoAsync(int id)
        {
            var result = await _cardServices.FindHeaderInfoAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the collection of card batches associated with the specified document batch identifier.
        /// </summary>
        /// <remarks>This method is asynchronous and may involve I/O operations. Ensure that the
        /// documentBatchId parameter is valid to avoid exceptions.</remarks>
        /// <param name="documentBatchId">The unique identifier of the document batch for which to retrieve associated card batches. Must be a
        /// positive integer.</param>
        /// <returns>An IActionResult containing a collection of CardBatchDto objects that represent the card batches linked to
        /// the specified document batch identifier. Returns an empty collection if no card batches are found.</returns>
        [HttpGet("Batch/{documentBatchId}")]
        [SwaggerOperation("Retrieves the collection of card batches associated with the specified document batch identifier.")]
        [ProducesResponseType(typeof(ICollection<CardBatchDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindCardsByDocumentBatchId(int documentBatchId)
        {
            var result = await _cardServices.FindCardsByDocumentBatchId(documentBatchId);
            return Ok(result);
        }

        /// <summary>
        /// Initiates a reprocessing operation for the specified card.
        /// </summary>
        /// <remarks>This endpoint requires valid tenant and creator email information in the request
        /// headers. The operation returns <see langword="true"/> if the card was successfully reprocessed; otherwise,
        /// <see langword="false"/>.</remarks>
        /// <param name="id">The unique identifier of the card to reprocess.</param>
        /// <param name="headersDto">The headers containing tenant and creator email information required for authorization and auditing.</param>
        /// <returns>An <see cref="IActionResult"/> containing a boolean value indicating whether the reprocessing operation was
        /// successful.</returns>
        [HttpPut("{id}/Reprocess")]
        [SwaggerOperation("Initiates a reprocessing operation for the specified card.")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReprocessCard(int id, [FromHeader] HeadersDto headersDto)
        {
            var result = await _cardServices.ReprocessCard(id, headersDto.Tenant, headersDto.EmailCreator);
            return Ok(result);
        }
    }
}
