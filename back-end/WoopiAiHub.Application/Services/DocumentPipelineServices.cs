using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.Services
{
    public class DocumentPipelineServices : IDocumentPipelineServices
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IUsageDailyServices _usageDailyServices;
        private readonly IExecutionServices _executionServices;
        private readonly MessageQueues _messageQueues;

        private const string FindingDocumentErrorMessage = "Error while finding document in database";

        public DocumentPipelineServices(
            IDocumentRepository documentRepository,
            IStepToolExecutionRepository stepToolExecutionRepository,
            IStepToolRepository stepToolRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            IUnitOfWork unitOfWork,
            ITenantCacheServices tenantCacheServices,
            IUsageDailyServices usageDailyServices,
            IExecutionServices executionServices,
            IOptions<MessageQueues> messageQueues)
        {
            _documentRepository = documentRepository;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolRepository = stepToolRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _unitOfWork = unitOfWork;
            _tenantCacheServices = tenantCacheServices;
            _usageDailyServices = usageDailyServices;
            _executionServices = executionServices;
            _messageQueues = messageQueues.Value;
        }

        
        /// <summary>
        /// Processes the OCR result and extracts document embeddings.
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<MetaDataAutomationDto> ProcessOcrResult(ProcessOcrResultDto dto)
        {
            if (dto.Data.Equals(default(MetaDataAutomationDto)))
                return new MetaDataAutomationDto();

            var documentEmbeddings = await ExtractDocumentEmbeddingsAddDto(dto);

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(dto.Data.StepToolId, dto.Data.CardId);

            if (execution is null)
                return dto.Data;

            await _executionServices.HandleExecutionProgress(execution, dto.Email);
            var dependentStepTool = await _stepToolRepository.FindDependentAsync(dto.Data.StepToolId);
            string embeddingsJson = JsonConvert.SerializeObject(new DocumentEmbeddingsDataDto
            {
                ResponseQueue = _messageQueues.EmbeddingQueueAiHubResponse,
                ReferenceFile = dto.ReferenceFile,
                DocumentEmbeddings = documentEmbeddings,
                Data = new MetaDataAutomationDto { CardId = dto.Data.CardId, StepToolId = dependentStepTool?.Id ?? 0 },
            });

            await SaveStepToolOutputAsync(execution, embeddingsJson);
            await UpdateDocumentStatusAsync(dto.ReferenceFile);

            dto.Data = dto.Data with { WorkflowId = execution.StepTool?.Step?.WorkflowId };

            return dto.Data;
        }

        /// <summary>
        /// Processes the questionnaire result from the tool and saves the result, token usage and history in the database.
        /// </summary>
        /// <param name="documentQuestionnaireDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<Document?> InputToolQuestionnaire(DocumentEmbeddingsQueryResponseDto documentQuestionnaireDto)
        {
            var documentDb = _documentRepository.FindByReferenceFile(documentQuestionnaireDto.ReferenceFile);
            if (documentDb == null)
            {
                return null;
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var dataDto = System.Text.Json.JsonSerializer.Deserialize<MetaDataAutomationDto>(documentQuestionnaireDto.Data.ToString());

                var execution = await _stepToolExecutionRepository
                    .FindByStepToolIdAndCardIdAsync(dataDto.StepToolId, dataDto.CardId);

                await _executionServices.HandleExecutionProgress(execution!, documentQuestionnaireDto.Email);
                await SaveStepToolOutputAsync(
                    execution!, 
                    System.Text.Json.JsonSerializer.Serialize(
                        documentQuestionnaireDto
                            .QuestionsAnswers
                                .Select(x => new QuestionAnswerDto {
                                    Id = x.Id,
                                    Question = x.Question,
                                    Answer = x.Answer
                                })
                                .ToList()));

                var usages = documentQuestionnaireDto.QuestionsAnswers.SelectMany(x => x.Usage)
                    .ToList();

                await _usageDailyServices.AddByRangeValuesAsync(
                    MetricNames.Token,
                    documentQuestionnaireDto.Email,
                    usages,
                    execution?.StepTool?.Step?.WorkflowId
                );

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }

            return documentDb;
        }
        
        /// <summary>
        /// Process the result of the embeddings request and updates the document status.
        /// </summary>
        /// <param name="documentEmbeddingsResultDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<MetaDataAutomationDto> ProcessEmbeddingsResult(
            DocumentEmbeddingsResultDto documentEmbeddingsResultDto)
        {
            var documentId =
                _documentRepository.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile);
            if (documentId == 0)
            {
                throw new ArgumentException(FindingDocumentErrorMessage);
            }

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(documentEmbeddingsResultDto.Data.StepToolId,
                    documentEmbeddingsResultDto.Data.CardId);
            await _executionServices.HandleExecutionProgress(execution!, documentEmbeddingsResultDto.Email);
            await SaveStepToolOutputAsync(execution!, documentEmbeddingsResultDto.ReferenceFile);
            _documentRepository.ChangeStatus(documentId, DocumentStatus.Embeddings);

            documentEmbeddingsResultDto.Data = documentEmbeddingsResultDto.Data with { WorkflowId = execution?.StepTool?.Step?.WorkflowId };

            return documentEmbeddingsResultDto.Data;
        }

        /// <summary>
        /// Updates StepToolExecution output
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="outputStepTool"></param>
        /// <returns></returns>
        private async Task SaveStepToolOutputAsync(StepToolExecution execution, string outputStepTool)
        {
            var output = new StepToolOutput(
                0,
                DateTime.Now,
                execution.StepToolId,
                execution.CardId,
                outputStepTool
            );

            await _stepToolOutputRepository.CreateAsync(output);
        }

        /// <summary>
        /// Updates document status
        /// </summary>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        private Task UpdateDocumentStatusAsync(string referenceFile)
        {
            var documentId = _documentRepository.FindDocumentIdByReferenceFile(referenceFile);
            _documentRepository.ChangeStatus(documentId, DocumentStatus.OCR);
            return Task.CompletedTask;
        }
          
        /// <summary>
        /// Extract normalized context from AnalyzeResult 
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        private async Task<List<DocumentEmbeddingsAddDto>> ExtractDocumentEmbeddingsAddDto(
            ProcessOcrResultDto processOcrResultDto)
        {
            List<DocumentEmbeddingsAddDto> listDocument = new List<DocumentEmbeddingsAddDto>();

            var tablesByPage = processOcrResultDto.AnalyzeResult.Tables
                .GroupBy(table => table.BoundingRegions.Count > 0 ? table.BoundingRegions[0].PageNumber : 0)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var page in processOcrResultDto.AnalyzeResult.Pages)
            {
                var pageText = new StringBuilder($"----------- Página {page.PageNumber} do PDF -----------\n\n");

                var paragraphTexts = page.Lines.Select(line => line.Content).ToList();

                var pageTables = tablesByPage.TryGetValue(page.PageNumber, out List<CustomDocumentTable>? value)
                    ? value
                    : [];

                var tableTexts = pageTables.Select(table =>
                {
                    var tableContent = new StringBuilder($"\n--- Tabela ---\n");
                    foreach (var row in table.Cells.GroupBy(c => c.RowIndex))
                    {
                        var line = string.Join(" | ", row.OrderBy(c => c.ColumnIndex).Select(c => c.Content));
                        tableContent.AppendLine(line);
                    }

                    return tableContent.ToString();
                }).ToList();

                var remainingParagraphs = paragraphTexts
                    .Where(paragraph => !tableTexts.Any(table => table.Contains(paragraph)))
                    .ToList();

                pageText.AppendLine(string.Join(Environment.NewLine, remainingParagraphs));
                pageText.AppendLine(string.Join(Environment.NewLine, tableTexts));

                var documentEmbeddingsAddDto = await CreateAddDocumentsEmbeddingsDtoAsync(processOcrResultDto,
                    pageText.ToString(),
                    page);
                listDocument.Add(documentEmbeddingsAddDto);
            }

            return listDocument;
        }

        /// <summary>
        /// Creates an object of type AddDocumentsRequestDto
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<DocumentEmbeddingsAddDto> CreateAddDocumentsEmbeddingsDtoAsync(
            ProcessOcrResultDto processOcrResultDto,
            string text,
            CustomDocumentPage page)
        {
            var tenant = await _tenantCacheServices.FindTenantAsync(processOcrResultDto.Tenant);
            return new DocumentEmbeddingsAddDto
            {
                ReferenceFile = processOcrResultDto.ReferenceFile,
                KeyMongoAccess = string.Empty,
                Text = text,
                Metadata = new { PageNumber = page.PageNumber },
                Tenant = processOcrResultDto.Tenant,
                EmbeddingModelName = tenant!.EmbeddingModelName,
                ChunkSize = tenant.ChunkSize,
                Email = processOcrResultDto.Email
            };
        }
    }
}
