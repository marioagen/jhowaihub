using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.Domain.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DocAnalyzer.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> _logger;
        private readonly IQuestionServices _questionServices;

        public QuestionController(ILogger<QuestionController> logger,
                                 IQuestionServices questionServices)
        {
            _logger = logger;
            _questionServices = questionServices;
        }

        /// <summary>
        /// Endpoint that receives the request to create a question in the database
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a question in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Create([FromQuery] QuestionCreateDto questionCreateDto,
                                    [FromHeader] HeadersDto headersDto)
        {
            try
            {
                var result = _questionServices.CreateUniqueQuestion(questionCreateDto,
                                                                    headersDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionController)} in the {nameof(Create)} method");
                return Conflict(new { message = "Duplicated question" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(Create)} method");
                return BadRequest("Error when creating question: " + ex);
            }

        }

        /// <summary>
        /// Endpoint that receives the request to return all questions paginated by email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all questions paginated")]
        [ProducesResponseType(typeof(QuestionPagedResultDto), StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] QuestionPagedDataDto questionPagedDataDto)
        {
            try
            {
                var result = _questionServices.FindAllPaged(questionPagedDataDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("The number of pages must be greater than 0");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("Error returning paginated questions by email: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all questions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("FindAll")]
        [SwaggerOperation("Endpoint that receives the request to return all questions")]
        [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
        public IActionResult FindAll()
        {
            try
            {
                var result = _questionServices.FindAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(FindAll)} method");
                return BadRequest("Error when returning questions: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to remove questions from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteByIds")]
        [SwaggerOperation("Endpoint that receives the request to remove questions from the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult DeleteByIds(List<int> ids)
        {
            try
            {
                var result = _questionServices.DeleteByIds(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(DeleteByIds)} method");
                return BadRequest("Error while deleting question: " + ex);
            }
        }

        /// <summary>
        /// EndPoint that update a question by passing an UpdateQuestionDto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a question by passing an UpdateQuestionDto")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Update(QuestionUpdateDto updatequestionDto)
        {
            try
            {
                var result = _questionServices.Update(updatequestionDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(QuestionController)} in the {nameof(Update)} method");
                return Conflict(new { message = "Duplicated questions" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(QuestionController)} in the {nameof(Update)} method");
                return BadRequest("Error while updating question: " + ex);
            }
        }
    }
}
