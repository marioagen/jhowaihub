using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentHistoryController : ControllerBase
    {
        private readonly IDocumentHistoryServices _documentHistoryServices;

        public DocumentHistoryController(IDocumentHistoryServices documentHistoryServices)
        {
            _documentHistoryServices = documentHistoryServices;
        }

        /// <summary>
        /// Receive a id and return the Document history.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("EndPoint that returns the history of an Document by id")]
        [ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
        public IActionResult FindDocumentHistory(int id,
                                                [FromHeader] HeadersDto headersDto)
        {
            var result = _documentHistoryServices.FindById(id, headersDto.EmailCreator);
            return Ok(result);
        }

        /// <summary>
        /// Returns the first N document history entries by document id (cumulative load: pass take=10, then 20, then 30...).
        /// Optional query: search (filter Input/Output), order (asc/desc), orderBy (created).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="take"></param>
        /// <param name="search"></param>
        /// <param name="order"></param>
        /// <param name="orderBy"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("{id}/batch")]
        [SwaggerOperation("Returns document history entries for a document, limited by take (load more: 10, 20, 30...)")]
        [ProducesResponseType(typeof(IEnumerable<DocumentHistoryDto>), StatusCodes.Status200OK)]
        public IActionResult FindDocumentHistoryBatch(int id,
                                                     [FromQuery] int take = 10,
                                                     [FromQuery] string? search = null,
                                                     [FromQuery] string? order = null,
                                                     [FromQuery] string? orderBy = null,
                                                     [FromQuery] Guid? user = null)
        {
            var result = _documentHistoryServices.FindByIdWithTake(id, take, search, order, orderBy, user);
            return Ok(result);
        }

        /// <summary>
        /// receive a dto and update the history output of an Document.
        /// </summary>
        /// <param name="updateHistoryDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that returns the history of an Document by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult UpdateDocumentHistory(UpdateHistoryDto updateHistoryDto,
                                                  [FromHeader] HeadersDto headersDto)
        {
            var result = _documentHistoryServices.UpdateHistory(updateHistoryDto, headersDto.EmailCreator);
            if (result)
                return Ok();
            return BadRequest("Error while deleting from database");
        }

        /// <summary>
        /// Receive an Document id and delete Document history.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [SwaggerOperation("EndPoint that delete an Document history by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteDocumentHistory(int id,
                                                  [FromHeader] HeadersDto headersDto)
        {
            var result = _documentHistoryServices.Delete(id, headersDto.EmailCreator);
            if (result)
                return Ok();
            return BadRequest("Error while deleting from database");
        }

    }
}
