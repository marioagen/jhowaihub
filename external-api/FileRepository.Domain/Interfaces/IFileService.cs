using Azure.Storage.Blobs.Models;
using FileRepository.Domain.DTOs;
using Microsoft.AspNetCore.Http;

namespace FileRepository.Domain.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadSummaryDto> UploadFileAsync(IFormFile file, string tenant);
        bool DeleteFile(string GuidfileName);
        Task<BlobDownloadStreamingResult> GetFileAsync(string GuidfileName);
    }
}
