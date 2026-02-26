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
        private readonly IDocumentQuestionnaireServices _documentQuestionnaireServices;

        public DocumentQuestionnarireController(IDocumentQuestionnaireServices documentQuestionnaireServices)
        {
            _documentQuestionnaireServices = documentQuestionnaireServices;
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
            var result = await _documentQuestionnaireServices.InputDocument(documentInputDto, headersDto);
            return Ok(result);
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
            var result = await _documentQuestionnaireServices.InputQuestionnaire(documentQuestionnaireDto, headersDto);
            return Ok(result);
        }
    }
}