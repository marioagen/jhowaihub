using FileRepository.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UploadingLargeFiles.Utilities;

namespace UploadingLargeFiles.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService,
                              ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Receives a request and saves the file sent to a storage account
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Upload(IFormFile file,
                                                [FromHeader(Name = "X-Tenant")] string tenant)
        {
            try
            {
                var fileUploadSummary = await _fileService.UploadFileAsync(file, tenant);

                return CreatedAtAction(nameof(Upload), fileUploadSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(FileController)} in the {nameof(Upload)} method");
                return BadRequest("Error while uploaded a file: " + ex);
            }
        }

        /// <summary>
        /// Receives a request and removes the file with the name passed as a storage account parameter
        /// </summary>
        /// <param name="GuidfileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpDelete("delete")]
        public ActionResult Delete(string GuidfileName)
        {
            try
            {
                if (GuidfileName is not null)
                {
                    var response = _fileService.DeleteFile(GuidfileName);

                    if (response is false)
                    {
                        return BadRequest(response);
                    }

                    return Ok();
                }
                else
                {
                    throw new ArgumentNullException($"The {nameof(GuidfileName)} is null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(FileController)} in the {nameof(Delete)} method");
                return BadRequest("Error while delete a file: " + ex);
            }
        }
    }
}