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
        private readonly IDocumentUploadServices _documentUploadServices;
        private readonly IDocumentDeletionServices _documentDeletionServices;
        private const string PdfContentType = "application/pdf";

        public DocumentController(IDocumentServices documentServices,
                                  IDocumentUploadServices documentUploadServices,
                                  IDocumentDeletionServices documentDeletionServices)
        {
            _documentServices = documentServices;
            _documentUploadServices = documentUploadServices;
            _documentDeletionServices = documentDeletionServices;
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
            var documentList = _documentServices.FindAllPaged(documentPagedDataDto, headersDto.EmailCreator);
            return Ok(documentList);
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
            await _documentUploadServices.ProcessChunks(requestCreateDocumentDto, headersDto.Tenant);
            return requestCreateDocumentDto.IsLast ? Ok() : Accepted();
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
            var result = await _documentDeletionServices.Delete(ids, headersDto);
            if (result)
                return Ok();
            return BadRequest("Error while deleting from database");
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
            var result = await _documentServices.CheckerExceededPages(headersDto.EmailCreator);
            return Ok(result);
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
            FindDocumentDto result = await _documentServices.FindDocumentById(id, headersDto.Tenant);
            return File(result.BytesDocument, PdfContentType, $"{result.ReferenceFile}.pdf");
        }
    }
}
