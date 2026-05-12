using System.Linq.Dynamic.Core;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Services
{
    public class PromptServices : IPromptServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPromptRepository _promptRepository;
        private readonly IValidatePrompt _validatePrompt;
        private readonly IUserServices _userServices;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IDocumentHistoryRepository _documentHistoryRepository;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private readonly IConfiguration _config;
        private readonly PromptSettings _promptSettings;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IRagInvocationRouter _ragInvocationRouter;
        private readonly ChatCompletionSettings _chatCompletionSettings;
        private readonly IUsageDailyServices _usageDailyServices;
        private readonly IExecutionServices _executionServices;

        public PromptServices(IUnitOfWork unitOfWork,
            IPromptRepository promptRepository,
            IValidatePrompt validatePrompt,
            IUserServices userServices,
            IStepToolExecutionRepository stepToolExecutionRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            IDocumentHistoryRepository documentHistoryRepository,
            IFunctionFileRetriever functionFileRetriever,
            IOptions<PromptSettings> promptSettingsOptions,
            IConfiguration config,
            ITenantCacheServices tenantCacheServices,
            IRagInvocationRouter ragInvocationRouter,
            IOptions<ChatCompletionSettings> chatCompletionSettings,
            IUsageDailyServices usageDailyServices,
            IExecutionServices executionServices)
        {
            _unitOfWork = unitOfWork;
            _promptRepository = promptRepository;
            _validatePrompt = validatePrompt;
            _userServices = userServices;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _documentHistoryRepository = documentHistoryRepository;
            _functionFileRetriever = functionFileRetriever;
            _config = config;
            _promptSettings = promptSettingsOptions.Value;
            _tenantCacheServices = tenantCacheServices;
            _ragInvocationRouter = ragInvocationRouter;
            _chatCompletionSettings = chatCompletionSettings.Value;
            _usageDailyServices = usageDailyServices;
            _executionServices = executionServices;
        }

        /// <summary>
        /// Find prompt templates from external source
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public async Task<List<PromptTemplateDto>> FindPromptTemplates(string? query, string? orderBy)
        {
            var templates = await FindAllTemplates();

            if (!string.IsNullOrEmpty(query))
            {
                var lowerQuery = query.ToLower();
                templates = templates.Where(t =>
                    t.Name.ToLower().Contains(lowerQuery) ||
                    t.Description.ToLower().Contains(lowerQuery) ||
                    t.Text.ToLower().Contains(lowerQuery)
                ).ToList();
            }

            templates = orderBy?.ToLower() switch
            {
                "name_asc" => templates.OrderBy(t => t.Name).ToList(),
                "name_desc" => templates.OrderByDescending(t => t.Name).ToList(),
                "created_asc" => templates.OrderBy(t => t.Created).ToList(),
                _ => templates.OrderByDescending(t => t.Created).ToList()
            };

            return templates;
        }

        /// <summary>
        /// Find all prompt templates
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<List<PromptTemplateDto>> FindAllTemplates()
        {
            var functionApiKeyAuth = _config["RefitExternalSettings:FunctionApiKey"];

            var response = await _functionFileRetriever.Get(_promptSettings.TemplateFileName,
                functionApiKeyAuth!,
                _promptSettings.Folder);

            if (!response.IsSuccessStatusCode)
            {
                throw new AppException(ErrorCode.DefaultError, "Failed to retrieve prompt templates", null);
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<PromptTemplatesResponse>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return items?.Prompts ?? new List<PromptTemplateDto>();
        }

        /// <summary>
        /// Import prompts from template IDs
        /// </summary>
        /// <param name="templateIds"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> ImportPromptsByIds(List<Guid> templateIds, string email)
        {
            if (templateIds == null || templateIds.Count == 0)
            {
                return false;
            }

            var allTemplates = await FindAllTemplates();

            var selectedTemplates = allTemplates.Where(t => templateIds.Contains(t.Id)).ToList();

            if (selectedTemplates.Count == 0)
            {
                throw new ArgumentException("Selected templates not found");
            }

            var importedPrompts = selectedTemplates.Select(t => new ImportedPromptDto
            {
                TemplateId = t.Id,
                Name = t.Name,
                Description = t.Description,
                Text = t.Text,
                Created = t.Created
            }).ToList();

            return ImportPrompts(importedPrompts, email);
        }

        /// <summary>
        /// Import prompts from templates
        /// </summary>
        /// <param name="importedPrompts"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool ImportPrompts(List<ImportedPromptDto> importedPrompts, string email)
        {
            var idUser = _userServices.FindIdByEmail(email);
            if (idUser == Guid.Empty)
            {
                throw new ArgumentException("Invalid user id");
            }

            var prompts = importedPrompts.Select(dto => new Prompt(
                0,
                dto.Created,
                dto.Name.Substring(0, Math.Min(dto.Name.Length, 50)),
                dto.Description.Substring(0, Math.Min(dto.Description.Length, 500)),
                dto.Text,
                idUser,
                isEdited: false,
                isImported: true
            )).ToList();

            return _promptRepository.CreateByRange(prompts);
        }

        /// <summary>
        /// Create a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool CreateUniquePrompt(PromptCreateDto promptCreateDto, string email)
        {
            var idUser = _userServices.FindIdByEmail(email);
            var prompt = GeneratePromptToCreate(promptCreateDto, idUser);

            var result = _validatePrompt.ValidateRequiredPromptFields(prompt);

            if (!result)
            {
                return false;
            }

            var promptWithSameName = _promptRepository.FindByNameAndUser(prompt.Name, idUser);
            if (promptWithSameName != null)
            {
                throw new AppException(ErrorCode.Duplicated, "prompts.duplicated", null);
            }

            return _promptRepository.Create(prompt);
        }

        /// <summary>
        /// Create a new prompt from integration, this method is used for create a prompt from external source like prompt templates, for this reason the validation is only for the fields and not for the ownership, because the prompt is created with the user that is making the request and not with the user that is in the template, also this method return the prompt created with the id to be used in the front end after the integration
        /// </summary>
        /// <param name="promptIntegrationCreateDto"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AppException"></exception>
        public PromptIntegrationDto CreateUniquePromptFromIntegration(
            PromptIntegrationCreateDto promptIntegrationCreateDto,
            string email)
        {
            var idUser = _userServices.FindIdByEmail(email);
            var prompt = CreateAndValidatePromptFields(promptIntegrationCreateDto, idUser);

            var promptWithSameName = _promptRepository.FindByNameAndUser(prompt.Name, idUser);
            if (promptWithSameName != null)
            {
                throw new AppException(ErrorCode.Duplicated, "The Prompt name is already in use.", null);
            }

            var createPromptResult = _promptRepository.CreateAndReturn(prompt);

            if (createPromptResult == null)
            {
                throw new InvalidOperationException("Create prompt Failed");
            }

            return new PromptIntegrationDto
            {
                Id = prompt.Id,
                Name = prompt.Name,
                Description = prompt.Description,
                Text = prompt.Text
            };
        }

        /// <summary>
        /// Create a new prompt and validate the required fields, this method is used for create a prompt from external source like prompt templates, for this reason the validation is only for the fields and not for the ownership, because the prompt is created with the user that is making the request and not with the user that is in the template
        /// </summary>
        /// <param name="promptIntegrationCreateDto"></param>
        /// <param name="idUser"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private Prompt CreateAndValidatePromptFields(PromptIntegrationCreateDto promptIntegrationCreateDto, Guid idUser)
        {
            var prompt = new Prompt(
                0,
                DateTime.Now,
                promptIntegrationCreateDto.Name,
                promptIntegrationCreateDto.Description,
                promptIntegrationCreateDto.Text,
                idUser,
                isEdited: false,
                isImported: true
            );

            var result = _validatePrompt.ValidateRequiredPromptFields(prompt);
            if (!result)
            {
                throw new AppException(ErrorCode.RequiredField, "Fill in all the fields", null);
            }

            return prompt;
        }

        /// <summary>
        /// Updates a prompt and reconciles its ApiTemplate associations,
        /// removing de-selected ones and persisting newly added ones.
        /// </summary>
        /// <param name="promptUpdateDto">The update payload.</param>
        /// <param name="emailCreator">Email of the requesting user, used for ownership validation.</param>
        /// <returns><c>true</c> when the update is persisted successfully.</returns>
        public async Task<bool> Update(PromptUpdateDto promptUpdateDto, string emailCreator)
        {
            _validatePrompt.ValidateOwnership(promptUpdateDto.Id, emailCreator);

            var promptDto = _promptRepository.FindById(promptUpdateDto.Id);
            if (promptDto == null)
            {
                throw new ArgumentException("Prompt not found");
            }

            (var prompt, var promptApiTemplateIds) = GeneratePromptToUpdate(promptDto, promptUpdateDto);

            _validatePrompt.ValidateRequiredPromptFields(prompt);

            var templatesToRemove = await _promptRepository.FindPromptApiTemplatesByIds(promptApiTemplateIds);
            if (promptApiTemplateIds.Count > 0 && templatesToRemove.Count == 0)
            {
                throw new ArgumentException("Update prompt Failed");
            }

            await _promptRepository.UpdateAndRemovePromptApisFromPrompt(prompt, templatesToRemove);

            return true;
        }

        /// <summary>
        /// Find all prompts by email creator paginated
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public PagedResultDto<PromptDto> FindByIdUserPaged(PagedDataDto pagedDataDto,
            string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            var query = _promptRepository.FindByIdUser(idUser);

            if (query == null)
                throw new ArgumentException("Prompt not found");

            if (idUser == Guid.Empty)
                throw new ArgumentException("Invalid user id");

            query = pagedDataDto.IsAscending
                ? query.OrderBy(nameof(Prompt.Name))
                : query.OrderBy(nameof(Prompt.Name) + " descending");

            var result = PromptPagination(query, new PagedDataDto());

            return result;
        }

        /// <summary>
        /// Find all prompts paginated
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public PagedResultDto<PromptDto> FindAllPaged(PagedDataDto pagedDataDto,
            string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            if (pagedDataDto.Page > 0)
            {
                var query = _promptRepository.FindAllWithOwnerStatus(idUser);

                query = pagedDataDto.IsAscending
                    ? query.OrderBy(nameof(Prompt.Name))
                    : query.OrderBy(nameof(Prompt.Name) + " descending");

                var result = PromptPagination(query, pagedDataDto);
                return result;
            }
            else
            {
                throw new ArgumentException("The number of pages must be greater than 0");
            }
        }

        /// <summary>
        /// Delete a prompt by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var result = _promptRepository.Delete(ids);
            if (!result)
            {
                throw new ArgumentException("Delete prompt failed");
            }

            return result;
        }

        /// <summary>
        /// Pagination of the prompt
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        private static PagedResultDto<PromptDto> PromptPagination(IQueryable<PromptDto> query,
            PagedDataDto pagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(pagedDataDto.Search))
            {
                string search = pagedDataDto.Search.ToLower();
                query = query.Where(i => i.Id.ToString().Contains(search) ||
                                         i.Name.Contains(search) ||
                                         i.Description.Contains(search) ||
                                         i.Text.Contains(search));
            }

            var count = query.Count();

            if (pagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)count / pagedDataDto.PageSize);
                currentPage = pagedDataDto.Page <= pageCount ? pagedDataDto.Page : 1;
                query = query.Skip((currentPage - 1) * pagedDataDto.PageSize)
                    .Take(pagedDataDto.PageSize);
            }

            return new PagedResultDto<PromptDto>()
            {
                Items = query,
                CurrentPage = currentPage,
                TotalPages = pageCount,
                Count = count
            };
        }

        /// <summary>
        /// Find a prompt by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public PromptDto FindById(int id)
        {
            return _promptRepository.FindById(id) ??
                throw new AppException(ErrorCode.NotFound, "Prompt not found", null);
        }

        /// <summary>
        /// Find all prompts with email creator
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public IQueryable<PromptDto> FindAll(string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            var query = _promptRepository.FindAllWithOwnerStatus(idUser) ?? throw new ArgumentException("Prompt not found");

            if (idUser == Guid.Empty)
                throw new ArgumentException("Invalid user id");

            return query;
        }

        /// <summary>
        /// Asynchronously retrieves all prompts in the basic format.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<PromptIntegrationDto>> FindAllInternal()
        {
            return await _promptRepository.FindAllInternal();
        }

        /// <summary>
        /// Generate a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        private static Prompt GeneratePromptToCreate(PromptCreateDto promptCreateDto, Guid idUser)
        {
            var prompt = new Prompt(
                0,
                DateTime.Now,
                promptCreateDto.Name,
                promptCreateDto.Description,
                promptCreateDto.Text,
                idUser,
                enableAccessToMcp: promptCreateDto.EnableAccessToMcp);

            prompt.PromptApiTemplates = promptCreateDto.ApiTemplatesSelected
                .Select(x => new PromptApiTemplate(0, x, 0, DateTime.Now))
                .ToList();

            return prompt;
        }

        /// <summary>
        /// Update a prompt
        /// </summary>
        /// <param name="promptDto"></param>
        /// <param name="promptUpdateDto"></param>
        private static (Prompt, List<int> promptApiTemplateIds) GeneratePromptToUpdate(PromptDto promptDto, PromptUpdateDto promptUpdateDto)
        {
            var prompt = new Prompt(
                promptDto.Id,
                promptDto.Created,
                promptUpdateDto.Name,
                promptUpdateDto.Description,
                promptUpdateDto.Text,
                promptDto.IdUser,
                enableAccessToMcp: promptDto.EnableAccessToMcp);

            var apiToDelete = promptDto.PromptApiTemplates.Where(x => !promptUpdateDto.ApiTemplatesSelected.Contains(x.ApiTemplateId)).Select(x => x.Id).ToList();
            var apiToCreate = promptUpdateDto.ApiTemplatesSelected.Where(x => !promptDto.PromptApiTemplates.Any(p => p.ApiTemplateId == x));
            prompt.PromptApiTemplates = apiToCreate
                .Select(x => new PromptApiTemplate(0, x, 0, DateTime.Now))
                .ToList();

            return (prompt, apiToDelete);
        }

        /// <summary>
        /// Process the chat completion result and create a DocumentAnswer.
        /// </summary>
        /// <param name="chatCompletionResponseDto"></param>
        /// <returns></returns>
        public async Task ProcessChatCompletionResult(ChatCompletionResponseDto chatCompletionResponseDto)
        {
            var dataDto = JsonSerializer.Deserialize<MetaDataAutomationDto>(chatCompletionResponseDto.Data.ToString());
            var execution = await _stepToolExecutionRepository.FindByStepToolIdAndCardIdAsync(dataDto.StepToolId,
                dataDto.CardId) ?? throw new ArgumentException("StepToolExecution not found");

            var documentHistory = new DocumentHistory(execution.Card!.DocumentId,
                "Prompt",
                chatCompletionResponseDto.Choices[0].Message.Content,
                0,
                DateTime.Now);

            _unitOfWork.BeginTransaction();
            try
            {
                _documentHistoryRepository.Create(documentHistory);
                await _executionServices.HandleExecutionProgress(execution, chatCompletionResponseDto.Email);
                await SaveStepToolOutputAsync(execution!, chatCompletionResponseDto.Choices[0].Message.Content);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Will process the response from open ai
        /// </summary>
        /// <param name="responseDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AppException"></exception>
        public async Task<StepToolExecution> ProcessOpenAiResponseResult(OpenAiResponseConsumerResponseDto responseDto)
        {
            var dataDto = JsonSerializer.Deserialize<MetaDataAutomationDto>(responseDto.Data.ToString());
            var execution = await _stepToolExecutionRepository.FindByStepToolIdAndCardIdAsync(dataDto.StepToolId,
                dataDto.CardId);

            if (execution == null)
            {
                throw new ArgumentException("StepToolExecution not found");
            }

            string message = FindTheOutputFromOpenAiResponseToPromptUsed(responseDto);

            _unitOfWork.BeginTransaction();
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    var documentHistory = new DocumentHistory(execution.Card!.DocumentId,
                    "Prompt",
                    message,
                    0,
                    DateTime.Now);
                    _documentHistoryRepository.Create(documentHistory);
                    await _executionServices.HandleExecutionProgress(execution!, responseDto.Email);
                    await SaveStepToolOutputAsync(execution!, message);
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }

            return execution;
        }

        /// <summary>
        /// get the output from open ai response tools
        /// </summary>
        /// <param name="responseDto"></param>
        /// <returns></returns>
        private static string FindTheOutputFromOpenAiResponseToPromptUsed(OpenAiResponseConsumerResponseDto responseDto)
        {
            return responseDto
                    .Response
                    .Output
                    .FirstOrDefault(x => x.Type == OpenAiResponsesTypes.Message)?
                    .Content
                    .FirstOrDefault(x => x.Type == OpenAiResponseInputContentType.OutputText)?
                    .Text ?? string.Empty;
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
        /// Refine prompt using AI Gateway
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> AiPromptRefinement(string prompt, string tenantId, string email)
        {
            var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenantId);
            if (tenantInfo!.AiGatewayApplicationId.HasValue is false || string.IsNullOrEmpty(tenantInfo.AiGatewayKey))
            {
                throw new ArgumentException("AiGateway ApplicationId not found");
            }

            var refinementPrompt = _config["PromptSettings:RefinementPrompt"];

            if (string.IsNullOrEmpty(refinementPrompt))
            {
                throw new ArgumentException("Refinement prompt template not found");
            }

            var fullPrompt = refinementPrompt.Replace("{{Regra de negócio}}", prompt);

            var chatCompletionDto = new ChatCompletionDto
            {
                Temperature = _chatCompletionSettings.Temperature,
                MaxTokens = _chatCompletionSettings.MaxTokens,
                Messages = new List<ChatMessageDto> { new ChatMessageDto { Role = "system", Content = fullPrompt } }
            };
            var response = await _ragInvocationRouter.ExecuteChatCompletionAsync(
                tenantInfo,
                email,
                chatCompletionDto,
                _chatCompletionSettings.Model,
                _chatCompletionSettings.ApiVersion,
                CancellationToken.None);

            var tokens = response.Usage?.TotalTokens ?? 0;
            await _usageDailyServices.AddByValuesAsync(MetricNames.Token, email, tokens, _chatCompletionSettings.Model);

            return response.Choices[0].Message.Content;
        }
    }
}
