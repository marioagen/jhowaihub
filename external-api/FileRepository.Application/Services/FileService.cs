using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FileRepository.Domain.DTOs;
using FileRepository.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace FileRepository.Application.Services
{
    public class FileService : IFileService
    {
        private readonly List<string> allowedExtensions = new() { ".pdf", ".PDF" };
        private readonly ILogger<FileService> _logger;
        private readonly BlobContainerClient _storageAccountAzure;

        public FileService(ILogger<FileService> logger,
                           BlobContainerClient storageAccountAzure)
        {
            _logger = logger;
            _storageAccountAzure = storageAccountAzure;
        }

        /// <summary>
        /// Save a file to the storage account
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<FileUploadSummaryDto> UploadFileAsync(IFormFile file,
                                                                string tenant)
        {
            if (file is not null)
            {
                var GuidIdFile = "a" + Guid.NewGuid().ToString("N");

                await SaveFileAsync(file,
                                    GuidIdFile,
                                    tenant);

                return this.FormatReturnUpload(file,
                                               GuidIdFile);
            }
            else
            {
                _logger.LogError($"An exception occurred in the {nameof(FileService)} in the {nameof(UploadFileAsync)} method");
                throw new ArgumentNullException("An error occurred while reading the file for upload");
            }
        }

        /// <summary>
        /// Get a file from a storage account
        /// </summary>
        /// <param name="GuidfileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<BlobDownloadStreamingResult> GetFileAsync(string GuidfileName)
        {
            var fileAzureStorage = _storageAccountAzure.GetBlobClient(GuidfileName);
            var existsFile = await fileAzureStorage.ExistsAsync();

            if (existsFile.Value is false)
            {
                throw new Exception("The requested file does not exist");
            }

            var download = await fileAzureStorage.DownloadStreamingAsync();

            return download;
        }

        /// <summary>
        /// Delete a file from a storage account
        /// </summary>
        /// <param name="GuidfileName"></param>
        /// <returns></returns>
        public bool DeleteFile(string GuidfileName)
        {
            var blobClient = _storageAccountAzure.GetBlobClient(GuidfileName);

            if (blobClient.Exists().Value is true)
            {
                blobClient.DeleteIfExists();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Performs all necessary steps to save the file to the storage account
        /// </summary>
        /// <param name="file"></param>
        /// <param name="GuidfileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task SaveFileAsync(IFormFile file,
                                         string GuidfileName,
                                         string tenant)
        {
            var extension = Path.GetExtension(file.FileName);

            if (allowedExtensions.Contains(extension) is false)
            {
                _logger.LogError($"An exception occurred in the {nameof(FileService)} in the {nameof(SaveFileAsync)} method, invalid extension");
                throw new ArgumentException("Only .pdf files are accepted for upload");
            }

            string pathToUpload = $"{tenant}/{GuidfileName}";
            var clientFile = _storageAccountAzure.GetBlobClient(pathToUpload);
            
            await using (Stream? data = file.OpenReadStream())
            {
                await clientFile.UploadAsync(data);
            }
        }

        /// <summary>
        /// Format an object to return in the upload request
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="GuidfileName"></param>
        /// <returns></returns>
        private FileUploadSummaryDto FormatReturnUpload(IFormFile formFile,
                                                        string GuidfileName)
        {
            return new FileUploadSummaryDto
            {
                TotalSizeUploaded = ConvertSizeToString(formFile.Length),
                FileName = formFile.FileName,
                GuidId = GuidfileName
            };
        }

        /// <summary>
        /// Runs the size to string conversion process
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string ConvertSizeToString(long bytes)
        {
            var fileSize = new decimal(bytes);
            var kilobyte = new decimal(1024);
            var megabyte = new decimal(1024 * 1024);
            var gigabyte = new decimal(1024 * 1024 * 1024);

            return fileSize switch
            {
                _ when fileSize < kilobyte => "Less then 1KB",
                _ when fileSize < megabyte =>
                    $"{Math.Round(fileSize / kilobyte, fileSize < 10 * kilobyte ? 2 : 1, MidpointRounding.AwayFromZero):##,###.##}KB",
                _ when fileSize < gigabyte =>
                    $"{Math.Round(fileSize / megabyte, fileSize < 10 * megabyte ? 2 : 1, MidpointRounding.AwayFromZero):##,###.##}MB",
                _ when fileSize >= gigabyte =>
                    $"{Math.Round(fileSize / gigabyte, fileSize < 10 * gigabyte ? 2 : 1, MidpointRounding.AwayFromZero):##,###.##}GB",
                _ => "n/a"
            };
        }

        /// <summary>
        /// Get request limit from content-type
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        private string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                _logger.LogError($"An exception occurred in the {nameof(FileService)} in the {nameof(GetBoundary)} method");
                throw new InvalidDataException("Missing content-type boundary.");
            }

            return boundary;
        }
    }
}
