using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;

public class PromptHandler : IToolHandler
{
    private readonly MessageQueues _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly IPromptServices _promptServices;
    private readonly ChatCompletionSettings _chatCompletionSettings;

    public PromptHandler(ITenantCacheServices tenantCacheServices,
                         IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         IOptions<ChatCompletionSettings> chatCompletionSettings)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _chatCompletionSettings = chatCompletionSettings.Value;
    }

    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        string input,
                                                        string output)
    // converter input para inteiro e buscar prompt
    // montar o objeto para enviar para integrationServices (chatCompletionDto)
    {
        var promptId = int.Parse(input);
        var promptDto =  _promptServices.FindById(promptId);
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant, ColTypeModule.WoopiAiHub);
        
        return new ExecutionMessageDto
        {
            Queue = _messageQueues.OcrQueue,
            Message = new ChatCompletionQueryDto
            {
                ReferenceFile = automationServicesDto.ReferenceFile,
                Tenant = automationServicesDto.Tenant,
                Model = _chatCompletionSettings.Model,
                ApiVersion = _chatCompletionSettings.ApiVersion,
                ChatCompletion = new ChatCompletionDto
                {
                    Temperature = _chatCompletionSettings.Temperature,
                    MaxTokens = _chatCompletionSettings.MaxTokens,//colocar no appsettings
                    Messages = new List<ChatMessageDto>
                        {
                            new ChatMessageDto
                            {
                                Role = "system",                             //texto do ocr retornado
                                Content = string.Concat("Baseado no: \"", output, "\" e seguindo as orientações a seguir: ", promptDto.Text)
                            }
                        }
                },
                Email = automationServicesDto.Email
            }
        };
    }
}