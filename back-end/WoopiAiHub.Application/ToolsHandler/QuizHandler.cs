using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiIntegrationServices.Domain.Dtos.Request;

namespace WoopiAiHub.Application.ToolsHandler
{
    public class QuizHandler : IToolHandler
    {
        public string Type => HandlersTypes.Quiz;
        private readonly MessageQueues _messageQueues;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IQuestionnaireServices _quizServices;
        private readonly IConfiguration _config;

        public QuizHandler(ITenantCacheServices tenantCacheServices, IOptions<MessageQueues> messageQueues, IQuestionnaireServices quizServices, IConfiguration config)
        {
            _tenantCacheServices = tenantCacheServices;
            _messageQueues = messageQueues.Value;
            _quizServices = quizServices;
            _config = config;
        }

        public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                            StepToolParameter? input,
                                                            ICollection<StepToolOutput> outputs,
                                                            StepToolExecution? execution = null)
        {
            var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant);

            var quizId = int.Parse(input!.Value);
            var quizDto = _quizServices.FindById(quizId);
            var apikey = _config["IndexerApiKey"]!;
            var apiVersion = _config["ChatCompletionSettings:ApiVersion"]!;

            return new ExecutionMessageDto
            {
                Queue = _messageQueues.AnswerQueue,
                Message = new DocumentEmbeddingsQueryDto
                {
                    Questions = quizDto
                        .Questions
                        .Select(
                            q => new QuestionAIGatewayDto
                            {
                                Id = q.Id,
                                Question = q.Description,
                            }).ToList(),
                    ApplicationKey = tenantInfo!.AiGatewayKey,
                    ApplicationId = tenantInfo.AiGatewayApplicationId!.Value.ToString(),
                    RagProvider = tenantInfo.RagProvider,
                    EmbeddingModelName = tenantInfo.EmbeddingModelName,
                    ApiVersion = apiVersion,
                    ReferenceFile = automationServicesDto.ReferenceFile!,
                    Model = tenantInfo!.Model,
                    kValue = tenantInfo.KValue,
                    Temperature = 0,
                    Template = tenantInfo.Template.Replace("{language}", "pt"),
                    Refine_template = tenantInfo.RefineTemplate,
                    Max_tokens = tenantInfo.MaxTokens,
                    SearchMode = tenantInfo.SearchMode,
                    Tenant = tenantInfo.Name,
                    KeyMongoAccess = apikey,
                    Email = automationServicesDto.Email,
                    Data = JObject.FromObject(new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId))
                }
            };
        }
    }
}
