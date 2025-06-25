using FileRepository.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileRepository.Functions
{
    public class FileRetrieverAsync
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileRetrieverAsync> _logger;

        public FileRetrieverAsync(IFileService fileService,
                                  ILogger<FileRetrieverAsync> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [Function("FileRetrieverAsync")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Starting file retrieval process.");

                var fileGuidId = req.Query["fileGuidId"];
                if (string.IsNullOrEmpty(fileGuidId))
                {
                    _logger.LogWarning("Missing or empty 'fileGuidId' parameter.");
                    return new BadRequestObjectResult("The 'fileGuidId' parameter is required and cannot be empty.");
                }

                if (!req.Headers.TryGetValue("X-Tenant",
                    out var tenant))
                {
                    _logger.LogWarning("Missing 'X-Tenant' header.");
                    return new BadRequestObjectResult("The 'X-Tenant' header is required in the request.");
                }

                string path = $"{tenant}/{fileGuidId}";
                var result = await _fileService.GetFileAsync(path);

                _logger.LogInformation("File retrieval successful for FileGuidId: {FileGuidId} at {Timestamp}",
                                       fileGuidId, DateTime.UtcNow);

                return new FileStreamResult(result.Content,
                                            "application/octet-stream")
                {
                    FileDownloadName = fileGuidId,
                };
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "A null argument was passed.");
                return new BadRequestObjectResult("A required parameter was not provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during file retrieval.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
