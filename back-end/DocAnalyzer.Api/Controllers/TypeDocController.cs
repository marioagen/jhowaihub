using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DocAnalyzer.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TypeDocController : ControllerBase
    {
        private readonly ILogger<TypeDocController> _logger;
        private readonly ITypeDocServices _typeDocServices;

        public TypeDocController(ILogger<TypeDocController> logger,
                                 ITypeDocServices typeDocServices)
        {
            _logger = logger;
            _typeDocServices = typeDocServices;
        }

        /// <summary>
        /// Endpoint that receives the request to create a type of document in the database
        /// </summary>
        /// <param name="typeDocCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a type of document in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Create([FromQuery] TypeDocCreateDto typeDocCreateDto,
                                    [FromHeader] HeadersDto typeDocHeaderDto)
        {
            try
            {
                var result = _typeDocServices.CreateUniqueTypeDoc(typeDocCreateDto,
                                                                  typeDocHeaderDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(TypeDocController)} in the {nameof(Create)} method");
                return Conflict(new { message = "Duplicated TypeDoc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(TypeDocController)} in the {nameof(Create)} method");
                return BadRequest("Error while creating type doc: " + ex);
            }

        }

        /// <summary>
        /// EndPoint that delete documents type by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("DeleteByIds")]
        [SwaggerOperation("EndPoint that delete documents types by id")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult DeleteByIds(List<int> ids)
        {
            try
            {
                var result = _typeDocServices.DeleteByIds(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(TypeDocController)} in the {nameof(DeleteByIds)} method");
                return BadRequest("Error while deleting type doc : " + ex);
            }
        }

        /// <summary>
        /// EndPoint that update a type of document
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a type of document")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Update(TypeDocUpdateDto updateTypeDoc)
        {
            try
            {
                var result =  _typeDocServices.Update(updateTypeDoc);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(TypeDocController)} in the {nameof(Update)} method");
                return Conflict(new { message = "Duplicated types" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(TypeDocController)} in the {nameof(Update)} method");
                return BadRequest("Error while updating type doc: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all type of documents
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("FindAll")]
        [SwaggerOperation("Endpoint that receives the request to return all type of documents")]
        [ProducesResponseType(typeof(ICollection<TypeDoc>), StatusCodes.Status200OK)]
        public IActionResult FindAll()
        {
            try
            {
                var result = _typeDocServices.FindAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(TypeDocController)} in the {nameof(FindAll)} method");
                return BadRequest("Error while finding type docs: " + ex);
            }
        }

        /// <summary>
        /// Endpoint that receives the request to return all type of documents paginated
        /// </summary>
        /// <param name="typeDocPagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all type of documents paginated")]
        [ProducesResponseType(typeof(TypeDocPagedResultDto), StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] TypeDocPagedDataDto typeDocPagedDataDto)
        {
            try
            {
                var result = _typeDocServices.FindAllPaged(typeDocPagedDataDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Argument Exception ocurred in the {nameof(TypeDocController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("The number of pages must be greater than 0");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(TypeDocController)} in the {nameof(FindAllPaged)} method");
                return BadRequest("Error returning paginated type of documents: " + ex);
            }
        }
    }
}
