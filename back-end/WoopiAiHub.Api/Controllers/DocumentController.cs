using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentServices _documentServices;
        private readonly IDocumentHistoryServices _documentHistoryServices;
        private readonly ILogger<DocumentController> _logger;
        private readonly IDocumentNormalizedServices _documentNormalizedServices;
        private const string PdfContentType = "application/pdf";


        public DocumentController(IDocumentServices documentServices,
                                  IDocumentHistoryServices documentHistoryServices,
                                  ILogger<DocumentController> logger,
                                  IDocumentNormalizedServices documentNormalizedServices)
        {
            _documentServices = documentServices;
            _documentHistoryServices = documentHistoryServices;
            _logger = logger;
            _documentNormalizedServices = documentNormalizedServices;
        }

        /// <summary>
        /// Receive a page number or a search data and return
        /// documents (with pagination)
        /// </summary>
        /// <param name="DocumentPagedDataDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("EndPoint that  returns all documents with pagination")]
        [ProducesResponseType(typeof(DocumentPagedResultDto), StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] DocumentPagedDataDto documentPagedDataDto,
                                          [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var DocumentList = _documentServices.FindAllPaged(documentPagedDataDto,
                                                                  headersDto.EmailCreator);
                return Ok(DocumentList);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(DocumentController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("Invalid Page");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("Error While Finding documents");
            }
        }

        /// <summary>
        /// Create an Document after uploading the file to fileRepositoryApi
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <returns></returns>
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [SwaggerOperation("EndPoint async that create an Document after uploading file by chunks")]
        [HttpPost("UploadByChunks")]
        public async Task<IActionResult> UploadByChunks([FromForm] RequestCreateDocumentDto requestCreateDocumentDto,
                                                        [FromHeader] HeadersDto headersDto)
        {
            try
            {
                await _documentServices.ProcessChunks(requestCreateDocumentDto,
                                                      headersDto.Tenant);

                return requestCreateDocumentDto.IsLast ? Ok() :Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(UploadByChunks)} method");
                return BadRequest("Error when uploading Document: " + ex);
            }
        }

        /// <summary>
        /// Receive multiple ids to delete
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("Delete")]
        [SwaggerOperation("EndPoint that delete an Document by id")]
        public async Task<IActionResult> Delete([FromBody] List<int> ids,
                                                [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = await _documentServices.Delete(ids,
                                                            headersDto);

                if (result)
                    return Ok();
                else
                    return BadRequest("Error while deleting from database");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(Delete)} method");
                return BadRequest("Id not found" + ex);
            }
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
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(FindDocumentNormalizedText)} method");
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
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(FindByIdAnalyze)} method");
                return BadRequest("Error while finding documents by id" + ex);
            }
        }

        /// <summary>
        /// Receive the status to check exceeded pages
        /// </summary>
        /// <returns></returns>
        [HttpGet("CheckExceededPages")]
        [SwaggerOperation("EndPoint async that check exceeded pages")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckExceededPages([FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = await _documentServices.CheckerExceededPages(headersDto.EmailCreator);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(CheckExceededPages)} method");
                return BadRequest("Error while check exceeded pages" + ex);
            }
        }

        /// <summary>
        /// Retrieve a document based on id document.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        [HttpGet("FindDocument/{id}")]
        [SwaggerOperation(Summary = "Retrieve a document based on id document")]
        [ProducesResponseType(typeof(File), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindDocumentById(int id,
                                                          [FromHeader] HeadersDto headersDto)
        {
            try
            {
                FindDocumentDto result = await _documentServices.FindDocumentById(id,
                                                                                  headersDto.Tenant);

                return File(result.BytesDocument, 
                            PdfContentType, 
                            $"{result.ReferenceFile}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in {Controller}.{Method} method for documentId: {id} and tenant: {Tenant}.",
                    nameof(DocumentController), nameof(FindDocumentById), id, headersDto.Tenant);
                return StatusCode(500, "An unexpected error occurred while retrieving the document. Please try again or contact support.");
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
                    nameof(DocumentController), nameof(FindOcrText), id);
                return StatusCode(500, "An unexpected error occurred while retrieving OCR text. Please try again or contact support.");
            }
        }
    }
}
