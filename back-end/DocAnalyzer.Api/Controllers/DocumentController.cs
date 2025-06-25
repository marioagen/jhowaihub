using DocAnalyzer.Application.Dto;
using DocAnalyzer.Domain;
using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.Domain.Models;
using DocAnalyzer.Domain.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

namespace DocAnalyzer.Api.Controllers
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

                return requestCreateDocumentDto.IsLast ?
                                               Ok() :
                                               Accepted();
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
        /// Receive a user question and get an answer.
        /// </summary>
        /// <param name="DocumentInputDto"></param>
        /// <returns></returns>
        [HttpPost("Input")]
        [SwaggerOperation("EndPoint async that receives the id of the Document and the question in the body to return an answer")]
        public async Task<IActionResult> InputDocument([FromBody] DocumentInputDto documentInputDto,
                                                       [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = await _documentServices.InputDocument(documentInputDto,
                                                                   headersDto);
                return Ok(result);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, $"An FileNotFoundException occurred in the {nameof(DocumentController)} in the {nameof(InputDocument)} method");
                return NotFound("The file was not found in the llmindexer weavite" + ex);
            }
            catch (ApplicationException aex)
            {
                _logger.LogError(aex, $"An ApplicationException occurred in the {nameof(DocumentController)} in the {nameof(InputDocument)} method");
                return UnprocessableEntity(aex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(InputDocument)} method");
                return BadRequest("Error while processing input" + ex);
            }
        }

        /// <summary>
        /// Receive a user questionnaire and get an answer.
        /// </summary>
        /// <param name="documentQuestionnaireDto"></param>
        /// <returns></returns>
        [HttpPost("InputQuestionnaire")]
        [SwaggerOperation("EndPoint async that receives the id of the Document and the questionnaire in the body to return an answer")]
        public async Task<IActionResult> InputDocumentQuestionnaire([FromBody] DocumentQuestionnaireDto documentQuestionnaireDto,
                                                                    [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = await _documentServices.InputQuestionnaire(documentQuestionnaireDto,
                                                                        headersDto);
                return Ok(result);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, $"An FileNotFoundException occurred in the {nameof(DocumentController)} in the {nameof(InputDocumentQuestionnaire)} method");
                return NotFound("The file was not found in the llmindexer weavite" + ex);
            }
            catch (HttpException hex)
            {
                _logger.LogError(hex, $"An HttpException occurred in the {nameof(DocumentController)} in the {nameof(InputDocumentQuestionnaire)} method");
                return UnprocessableEntity(hex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(InputDocumentQuestionnaire)} method");
                return BadRequest("Error while processing input" + ex);
            }
        }



        /// <summary>
        /// Receive a id and return the Document history.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("History/{id}")]
        [SwaggerOperation("EndPoint that returns the history of an Document by id")]
        [ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
        public IActionResult FindDocumentHistory(int id,
                                                [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _documentHistoryServices.FindById(id,
                                                               headersDto.EmailCreator);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(FindDocumentHistory)} method");
                return BadRequest("Error while finding history" + ex);
            }
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
            try
            {
                var result = _documentHistoryServices.UpdateHistory(updateHistoryDto,
                                                                    headersDto.EmailCreator);
                if (result)
                    return Ok();
                else
                    return BadRequest("Error while deleting from database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(UpdateDocumentHistory)} method");
                return BadRequest("Error while updating history" + ex);
            }
        }

        /// <summary>
        /// Receive an Document id and delete Document history.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("History/{id}")]
        [SwaggerOperation("EndPoint that delete an Document history by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteDocumentHistory(int id,
                                                  [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _documentHistoryServices.Delete(id,
                                                             headersDto.EmailCreator);
                if (result)
                    return Ok();
                else
                    return BadRequest("Error while deleting from database");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(DeleteDocumentHistory)} method");
                return BadRequest("Error while deleting Document history" + ex);
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
        public IActionResult FindDocumentNormalizedText(int id,
                                                       [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _documentNormalizedServices.FindById(id,
                                                                  headersDto.EmailCreator);
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
        /// Receive a dto and normalize the Document text (OCR + embeddings).
        /// </summary>
        /// <param name="documentAnalysisRequestDto"></param>
        /// <returns></returns>
        [HttpPost("Analyze")]
        [SwaggerOperation("EndPoint async that normalize the Document text and returns a boolean")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DocumentAnalysis(DocumentAnalysisRequestDto documentAnalysisRequestDto,
                                                         [FromHeader] HeadersDto headersDto)
        {
            var documentAnalysisResponseDto = new DocumentAnalysisResponseDto
            {
                Id = documentAnalysisRequestDto.Id,
                EmailCreator = headersDto.EmailCreator,
                Tenant = headersDto.Tenant,
                KeyMongoAcess = headersDto.KeyMongoAccess,
                Embeddings_model_name = documentAnalysisRequestDto.Embeddings_model_name
            };

            try
            {
                var result = await _documentServices.DocumentAnalysis(documentAnalysisResponseDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(DocumentAnalysis)} method");
                return BadRequest("Error while analyzing documents" + ex);
            }
        }

        /// <summary>
        /// Receive a id and return the status and name of the Document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Status/{id}")]
        [SwaggerOperation("EndPoint that returns the status and name of an Document by id")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult FindStatusAndName(int id,
                                              [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _documentServices.FindStatusAndName(id,
                                                                 headersDto.EmailCreator);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentController)} in the {nameof(FindStatusAndName)} method");
                return BadRequest("Id not found" + ex);
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
    }
}