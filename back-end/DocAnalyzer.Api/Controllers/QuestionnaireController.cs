using DocAnalyzer.Application.Dto;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DocAnalyzer.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionnaireController : Controller
    {
        private readonly ILogger<QuestionnaireController> _logger;
        private readonly IQuestionnaireServices _questionnaireServices;

        public QuestionnaireController(ILogger<QuestionnaireController> logger,
                                       IQuestionnaireServices questionnaireServices)
        {
            _logger = logger;
            _questionnaireServices = questionnaireServices;
        }

        /// <summary>
        /// EndPoint that create a questionnaire by an CreateQuestionnaireDto
        /// </summary>
        /// <param name="createQuestionnaireDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("EndPoint that create a questionnaire by an CreateQuestionnaireDto")]
        [ProducesResponseType(typeof(Boolean), StatusCodes.Status200OK)]
        public IActionResult Create(CreateQuestionnaireDto createQuestionnaireDto,
                                   [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _questionnaireServices.CreateUniqueQuestionnaire(createQuestionnaireDto,
                                                                              headersDto.EmailCreator);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionnaireController)} in the {nameof(Create)} method");
                return Conflict(new { message = "Duplicated questionnaire" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionnaireController)} in the {nameof(Create)} method");
                return BadRequest("Error while creating questionnaire: " + ex);
            }

        }

        /// <summary>
        /// EndPoint that find a questionnaire by id
        /// </summary>
        /// <param name="questionnaireIdDto"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("EndPoint that find a questionnaire by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FindById(int id)
        {
            try
            {
                var result = _questionnaireServices.FindById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionnaireController)} in the {nameof(FindById)} method");
                return BadRequest("Error while finding a questionnaire by id: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all the questionnaires
        /// <returns></returns>
        [HttpGet]
        [Route("FindAll")]
        [SwaggerOperation("Endpoint that receives the request to return all questionnaires")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FindAll()
        {
            try
            {
                var result = _questionnaireServices.FindAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionnaireController)} in the {nameof(FindAll)} method");
                return BadRequest("Error while finding questionnaire by email: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all questionnaires paginated by email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all questionnaires")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] QuestionnairePagedDataDto questionnairePagedDataDto)
        {
            try
            {
                var result = _questionnaireServices.FindAllPaged(questionnairePagedDataDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionnaireController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("The number of pages must be greater than 0");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionnaireController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("Error returning paginated questionnaires by email: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to remove questionnaires from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteByIds")]
        [SwaggerOperation("Endpoint that receives the request to remove questionnaires from the database")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteByIds([FromBody] List<int> ids)
        {
            try
            {
                var result = _questionnaireServices.DeleteByIds(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionnaireController)} in the {nameof(DeleteByIds)} method");
                return BadRequest("Error while deleting questionnaire: " + ex);
            }
        }

        /// <summary>
        /// EndPoint that update a questionnaire by an UpdateQuestionnaireDto
        /// </summary>
        /// <param name="updateQuestionnaireDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a questionnaire by an UpdateQuestionnaireDto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Update(UpdateQuestionnaireDto updateQuestionnaireDto)
        {
            try
            {
                var result = _questionnaireServices.Update(updateQuestionnaireDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionnaireController)} in the {nameof(Update)} method");
                return Conflict(new { message = "Duplicated questionnaire" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionnaireController)} in the {nameof(Update)} method");
                return BadRequest("Error while updating questionnaire: " + ex);
            }
        }
    }
}
