using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.ToolsHandler;

public class ParserHandler : IToolHandler
{
    public string Type => HandlersTypes.Parser;

    private readonly MessageQueues _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly IDocumentRepository _documentRepository;

    public ParserHandler(
        ITenantCacheServices tenantCacheServices,
        IOptions<MessageQueues> messageQueues,
        IDocumentRepository documentRepository)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
        _documentRepository = documentRepository;
    }

    public async Task<ExecutionMessageDto> BuildPayload(
        AutomationServicesDto automationServicesDto,
        StepToolParameter? input,
        ICollection<StepToolOutput> outputs,
        StepToolExecution? execution = null)
    {
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant);
        if (string.IsNullOrEmpty(tenantInfo!.OcrModel))
        {
            throw new ArgumentException("Ocr not found");
        }

        var extractionMode = ResolveExtractionMode(input, automationServicesDto.ReferenceFile!);
        var processOcrDto = new ProcessOcrDto
        {
            Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
            Tenant = automationServicesDto.Tenant,
            ReferenceFile = automationServicesDto.ReferenceFile!,
            Model = tenantInfo.OcrModel,
            Email = automationServicesDto.Email,
            ResponseQueue = _messageQueues.OcrQueueAiHubResponse,
            ExtractionMode = extractionMode
        };

        if (DocumentExtractionModes.RequiresNativeProcessing(extractionMode))
        {
            return new ExecutionMessageDto
            {
                Queue = _messageQueues.ParserNativeQueue,
                Message = processOcrDto
            };
        }

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.OcrQueue,
            Message = processOcrDto
        };
    }

    private string ResolveExtractionMode(StepToolParameter? input, string referenceFile)
    {
        var toolMode = input?.Value;
        if (!string.IsNullOrWhiteSpace(toolMode)
            && !toolMode.Equals(DocumentExtractionModes.Auto, StringComparison.OrdinalIgnoreCase)
            && DocumentExtractionModes.IsValid(toolMode))
        {
            return toolMode;
        }

        var documentId = _documentRepository.FindDocumentIdByReferenceFile(referenceFile);
        if (documentId > 0)
        {
            var document = _documentRepository.FindById(documentId);
            if (!string.IsNullOrWhiteSpace(document?.ExtractionMode)
                && DocumentExtractionModes.IsValid(document.ExtractionMode))
            {
                return document.ExtractionMode!;
            }
        }

        return DocumentExtractionModes.Auto;
    }
}
