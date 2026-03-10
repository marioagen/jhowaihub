using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentMetadataController : ControllerBase
    {
        private readonly IDocumentMetadataServices _documentMetadataServices;
        private readonly IDocumentNormalizedServices _documentNormalizedServices;

        public DocumentMetadataController(
            IDocumentMetadataServices documentMetadataServices,
            IDocumentNormalizedServices documentNormalizedServices)
        {
            _documentMetadataServices = documentMetadataServices;
            _documentNormalizedServices = documentNormalizedServices;
        }

        /// <summary>
        /// Receive a id and return the DocumentNormalized text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Normalized/{id}")]
        [SwaggerOperation("EndPoint that returns the normalized text of an Document by id")]
        [ProducesResponseType(typeof(DocumentNormalized), StatusCodes.Status200OK)]
        public IActionResult FindDocumentNormalizedText(int id)
        {
            var result = _documentNormalizedServices.FindById(id);
            return Ok(result);
        }

        /// <summary>
        /// It receives an id and returns a FindByIdAnalyzeDto with the document's information.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Analyze/{id}")]
        [SwaggerOperation("It receives an id and returns a FindByIdAnalyzeDto with the document's information")]
        [ProducesResponseType(typeof(FindByIdAnalyzeDto), StatusCodes.Status200OK)]
        public IActionResult FindByIdAnalyze(int id,
                                             [FromHeader] HeadersDto headersDto)
        {
            var result = _documentMetadataServices.FindByIdAnalyze(id, headersDto);
            return Ok(result);
        }

        /// <summary>
        /// Retrieve the OCR text for a document if available.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>OCR text response with concatenated text from all pages</returns>
        [HttpGet("OcrText/{id}")]
        [SwaggerOperation(Summary = "Retrieve the OCR text for a document if available")]
        [ProducesResponseType(typeof(OcrTextResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindOcrText(int id)
        {
            var result = await _documentMetadataServices.FindOcrTextByDocumentId(id);
            return Ok(result);
        }
    }
}