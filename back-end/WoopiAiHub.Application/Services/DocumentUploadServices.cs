using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Refit;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class DocumentUploadServices : IDocumentUploadServices
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RequestCreateDocumentDto> _documentDtoValidator;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IAutomationServices _automationServices;
        private readonly IFileRepositoryApi _fileRepositoryApi;

        public DocumentUploadServices(
            IDocumentRepository documentRepository,
            IMemoryCache cache,
            IUnitOfWork unitOfWork,
            IValidator<RequestCreateDocumentDto> documentDtoValidator,
            IWorkflowRepository workflowRepository,
            IAutomationServices automationServices,
            IFileRepositoryApi fileRepositoryApi)
        {
            _documentRepository = documentRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
            _documentDtoValidator = documentDtoValidator;
            _workflowRepository = workflowRepository;
            _automationServices = automationServices;
            _fileRepositoryApi = fileRepositoryApi;
        }

        /// <summary>
        /// Processes the chunks of the document that will be saved.
        /// If it is the last part of the document, save it in the Database.
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <returns></returns>
        public async Task ProcessChunks(RequestCreateDocumentDto requestCreateDocumentDto,
            string tenant)
        {
            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var bytes = this.AddNewBytesToArrayChunks(requestCreateDocumentDto,
                cacheOptions);

            if (requestCreateDocumentDto.IsLast)
            {
                var referenceFile = await this.FinalizeUploadAsync(requestCreateDocumentDto, bytes, tenant);
                _cache.Remove(requestCreateDocumentDto.Name);
            }
        }

        /// <summary>
        /// It uploads the document PDF file to the fileRepository and after uploading 
        /// it saves the document data in the database.
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <returns></returns>
        /// 
        private async Task<string> FinalizeUploadAsync(RequestCreateDocumentDto requestCreateDocumentDto,
            Byte[] chunks,
            string tenant)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                await _documentDtoValidator.ValidateAndThrowAsync(requestCreateDocumentDto);
                var formFile = new FormFile(new MemoryStream(chunks),
                    0,
                    chunks.Length,
                    requestCreateDocumentDto.Filename,
                    requestCreateDocumentDto.Filename);

                var referenceFile = await this.UploadFileToRepositoryApi(formFile,
                    tenant);
                var workflows = await _workflowRepository.FindByIdsAsync(requestCreateDocumentDto.Workflows);
                var documentForDataBase = CreateDocumentForDb(requestCreateDocumentDto, workflows, referenceFile);

                ICollection<Card> cards = CreateDocumentCard(requestCreateDocumentDto, workflows);

                documentForDataBase.Cards = cards;
                _documentRepository.Create(documentForDataBase);

                var hasExecutions = await _automationServices.PrepareExecutionAsync(workflows!);
                var automationServicesDto = new AutomationServicesDto
                (
                    0,
                    0,
                    tenant,
                    requestCreateDocumentDto.EmailCreator,
                    referenceFile,
                    0
                );

                if (hasExecutions)
                {
                    await _automationServices.StartExecutionByWorkflowsAsync(automationServicesDto, workflows!);
                }

                _unitOfWork.Commit();
                return referenceFile;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Creates an Document type object to save in the database
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="referenceFile"></param>
        /// <param name="List<Workflow>"></param>
        /// <returns></returns>
        private static Document CreateDocumentForDb(RequestCreateDocumentDto requestCreateDocumentDto,
            List<Workflow> workflow, string referenceFile)
        {
            return new Document
            (
                requestCreateDocumentDto.Name,
                requestCreateDocumentDto.Description,
                referenceFile,
                (int)Domain.Enum.DocumentStatus.NotAnalyzed,
                requestCreateDocumentDto.EmailCreator,
                0,
                workflow,
                DateTime.Now
            );
        }

        /// <summary>
        /// Request the Refit interface to upload a file to the FileRepositoryApi
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> UploadFileToRepositoryApi(IFormFile formFile,
            string tenant)
        {
            FileUploadSummaryDto resultUpload;

            using (MemoryStream ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                resultUpload = await _fileRepositoryApi.Upload(new ByteArrayPart(ms.ToArray(),
                        formFile.FileName),
                    tenant);
            }

            if (resultUpload is not null)
                return resultUpload.GuidId;
            else
                throw new AppException(ErrorCode.UploadFailed,
                    "GuidId file reference returned null on upload FileRepository", null);
        }
        
        /// <summary>
        /// Adds the file chunk to the cache.
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        private byte[] AddNewBytesToArrayChunks(RequestCreateDocumentDto requestCreateDocumentDto,
            MemoryCacheEntryOptions cacheOptions)
        {
            byte[] newBytes;
            if (_cache.TryGetValue(requestCreateDocumentDto.Name,
                    out byte[]? existingBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    requestCreateDocumentDto.Chunk.CopyTo(memoryStream);
                    byte[] bytesChunk = memoryStream.ToArray();
                    newBytes = existingBytes!.Concat(bytesChunk).ToArray();

                    _cache.Set(requestCreateDocumentDto.Name,
                        newBytes,
                        cacheOptions);
                }
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    requestCreateDocumentDto.Chunk.CopyTo(memoryStream);
                    byte[] bytesChunk = memoryStream.ToArray();
                    newBytes = bytesChunk;

                    _cache.Set(requestCreateDocumentDto.Name,
                        newBytes,
                        cacheOptions);
                }
            }

            return newBytes;
        }

        /// <summary>
        /// Create card by a collections of teams
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="teams"></param>
        /// <returns></returns>
        private static List<Card> CreateDocumentCard(RequestCreateDocumentDto requestCreateDocumentDto,
            ICollection<Workflow> workflow)
        {
            return workflow
                .Select(w => w.Steps.OrderBy(s => s.Order).FirstOrDefault())
                .Where(step => step != null)
                .Select(step => new Card
                (
                    0,
                    DateTime.UtcNow,
                    step!.Id,
                    0,
                    requestCreateDocumentDto.Filename,
                    step.StatusId,
                    null
                ))
                .ToList();
        }
    }
}