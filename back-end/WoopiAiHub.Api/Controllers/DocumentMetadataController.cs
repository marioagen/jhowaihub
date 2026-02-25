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
        private const string PdfContentType = "application/pdf";
        private readonly IDocumentMetadataServices _documentMetadataServices;
        private readonly IDocumentNormalizedServices _documentNormalizedServices;
        private readonly IDocumentServices _documentServices;
        private readonly ILogger<DocumentMetadataController> _logger;

        public DocumentMetadataController(
            IDocumentMetadataServices documentMetadataServices,
            IDocumentNormalizedServices documentNormalizedServices,
            IDocumentServices documentServices,
            ILogger<DocumentMetadataController> logger)
        {
            _documentMetadataServices = documentMetadataServices;
            _documentNormalizedServices = documentNormalizedServices;
            _documentServices = documentServices;
            _logger = logger;
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
            try
            {
                var result = _documentNormalizedServices.FindById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentMetadataController)} in the {nameof(FindDocumentNormalizedText)} method");
                return BadRequest("Error while finding history" + ex);
            }
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
            try
            {
                var result =  _documentServices.FindByIdAnalyze(id,
                                                                headersDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentMetadataController)} in the {nameof(FindByIdAnalyze)} method");
                return BadRequest("Error while finding documents by id" + ex);
            }
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
            try
            {
                var result = await _documentServices.FindOcrTextByDocumentId(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in {Controller}.{Method} method for documentId: {id}.",
                    nameof(DocumentMetadataController), nameof(FindOcrText), id);
                return StatusCode(500, "An unexpected error occurred while retrieving OCR text. Please try again or contact support.");
            }
        }
    }
}