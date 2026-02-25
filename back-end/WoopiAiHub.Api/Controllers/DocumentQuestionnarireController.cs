using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentQuestionnarireController : ControllerBase
    {
        private const string PdfContentType = "application/pdf";
        private readonly IDocumentQuestionnaireServices _documentQuestionnaireServices;
        private readonly IDocumentServices _documentServices;
        private readonly ILogger<DocumentQuestionnarireController> _logger;

        public DocumentQuestionnarireController(
            IDocumentQuestionnaireServices documentQuestionnaireServices,
            IDocumentServices documentServices,
            ILogger<DocumentQuestionnarireController> logger)
        {
            _documentQuestionnaireServices = documentQuestionnaireServices;
            _documentServices = documentServices;
            _logger = logger;
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
                _logger.LogError(ex, $"An FileNotFoundException occurred in the {nameof(DocumentQuestionnarireController)} in the {nameof(InputDocument)} method");
                return NotFound("The file was not found in the llmindexer weavite" + ex);
            }
            catch (ApplicationException aex)
            {
                _logger.LogError(aex, $"An ApplicationException occurred in the {nameof(DocumentQuestionnarireController)} in the {nameof(InputDocument)} method");
                return UnprocessableEntity(aex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentQuestionnarireController)} in the {nameof(InputDocument)} method");
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
                _logger.LogError(ex, $"An FileNotFoundException occurred in the {nameof(DocumentQuestionnarireController)} in the {nameof(InputDocumentQuestionnaire)} method");
                return NotFound("The file was not found in the llmindexer weavite" + ex);
            }
            catch (HttpException hex)
            {
                _logger.LogError(hex, $"An HttpException occurred in the {nameof(DocumentQuestionnarireController)} in the {nameof(InputDocumentQuestionnaire)} method");
                return UnprocessableEntity(hex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentQuestionnarireController)} in the {nameof(InputDocumentQuestionnaire)} method");
                return BadRequest("Error while processing input" + ex);
            }
        }

    }
}