using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomationController : Controller
    {
        private readonly IAutomationServices _automationServices;
        private readonly ILogger<AutomationController> _logger;

        public AutomationController(IAutomationServices automationServices,
                                    ILogger<AutomationController> logger)
        {
            _automationServices = automationServices;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a question in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromQuery] StepToolCreateDto stepToolCreateDto)
        {
            var result =  await _automationServices.CreateAsync(stepToolCreateDto);
            return Ok(result);
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
                var result = _automationServices.FindAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AutomationController)} in the {nameof(FindAll)} method");
                return BadRequest("Error when returning questions: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all questions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("Endpoint that receives the request to return all questions")]
        [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
        public IActionResult FindById(int id)
        {
            try
            {
                var result = _automationServices.FindById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AutomationController)} in the {nameof(FindAll)} method");
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
                var result = _automationServices.DeleteByIds(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AutomationController)} in the {nameof(DeleteByIds)} method");
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
        public IActionResult Update(int id,
                                    string input)
        {
            try
            {
                var result = _automationServices.Update(id,input);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(AutomationController)} in the {nameof(Update)} method");
                return Conflict(new { message = "Duplicated questions" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AutomationController)} in the {nameof(Update)} method");
                return BadRequest("Error while updating question: " + ex);
            }
        }
    }
}
