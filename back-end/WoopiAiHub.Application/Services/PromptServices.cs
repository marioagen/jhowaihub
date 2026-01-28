using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
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
        private readonly IHubNotifier _hubNotifier;
        private readonly IDocumentHistoryRepository _documentHistoryRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private readonly IConfiguration _config;
        private readonly PromptSettings _promptSettings;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IChatCompletionApi _chatCompletionApi;
        private readonly ChatCompletionSettings _chatCompletionSettings;

        public PromptServices(IUnitOfWork unitOfWork,
            IPromptRepository promptRepository,
            IValidatePrompt validatePrompt,
            IUserServices userServices,
            IStepToolExecutionRepository stepToolExecutionRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            IHubNotifier hubNotifier,
            IDocumentHistoryRepository documentHistoryRepository,
            IWorkflowRepository workflowRepository,
            IFunctionFileRetriever functionFileRetriever,
            IOptions<PromptSettings> promptSettingsOptions,
            IConfiguration config,
            ITenantCacheServices tenantCacheServices,
            IChatCompletionApi chatCompletionApi,
            IOptions<ChatCompletionSettings> chatCompletionSettings)
        {
            _unitOfWork = unitOfWork;
            _promptRepository = promptRepository;
            _validatePrompt = validatePrompt;
            _userServices = userServices;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _hubNotifier = hubNotifier;
            _documentHistoryRepository = documentHistoryRepository;
            _workflowRepository = workflowRepository;
            _functionFileRetriever = functionFileRetriever;
            _config = config;
            _promptSettings = promptSettingsOptions.Value;
            _tenantCacheServices = tenantCacheServices;
            _chatCompletionApi = chatCompletionApi;
            _chatCompletionSettings = chatCompletionSettings.Value;
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

            var templates = items?.Prompts;
            if (templates == null)
            {
                return new List<PromptTemplateDto>();
            }

            return templates;
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
            if (importedPrompts == null || importedPrompts.Count == 0)
            {
                return false;
            }

            var idUser = _userServices.FindIdByEmail(email);
            if (idUser == Guid.Empty)
            {
                throw new ArgumentException("Invalid user id");
            }

            var prompts = importedPrompts.Select(dto => new Prompt(
                0,
                dto.Created,
                dto.Name.Substring(0, Math.Min(dto.Name.Length, 50)),
                dto.Description.Substring(0, Math.Min(dto.Description.Length, 95)),
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

            var result = _validatePrompt.ValidatePromptFields(prompt);

            if (!result)
            {
                return false;
            }

            var createPromptResult = _promptRepository.CreateUniquePrompt(prompt);
            if (!createPromptResult)
            {
                throw new AppException(ErrorCode.Duplicated, "Duplicated Prompt", null);
            }

            return createPromptResult;
        }

        /// <summary>
        /// Update a prompt
        /// </summary>
        /// <param name="promptUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Update(PromptUpdateDto promptUpdateDto, string emailCreator)
        {
            _validatePrompt.ValidateOwnership(promptUpdateDto.Id,
                emailCreator);

            var promptDto = _promptRepository.FindById(promptUpdateDto.Id);
            if (promptDto == null)
            {
                throw new ArgumentException("Prompt not found");
            }

            var prompt = GeneratePromptToUpdate(promptDto, promptUpdateDto);

            _validatePrompt.ValidatePromptFields(prompt);

            var promptUpdateResult = _promptRepository.Update(prompt);

            if (!promptUpdateResult)
            {
                throw new ArgumentException("Update prompt Failed");
            }

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
                Items = query, CurrentPage = currentPage, TotalPages = pageCount, Count = count
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
            var promptDto = _promptRepository.FindById(id);
            if (promptDto == null)
            {
                throw new ArgumentException("Prompt not found");
            }

            return promptDto;
        }

        /// <summary>
        /// Find all prompts with email creator
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public IQueryable<PromptDto> FindAll(string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            var query = _promptRepository.FindAllWithOwnerStatus(idUser);

            if (query == null)
                throw new ArgumentException("Prompt not found");
            if (idUser == Guid.Empty)
                throw new ArgumentException("Invalid user id");

            return query;
        }

        /// <summary>
        /// Asynchronously retrieves all prompts in the basic format.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<PromptInternalDto>> FindAllInternal()
        {
            return await _promptRepository.FindAllInternal();
        }

        /// <summary>
        /// Generate a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        private static Domain.Models.Prompt GeneratePromptToCreate(PromptCreateDto promptCreateDto, Guid idUser)
        {
            var prompt = new Domain.Models.Prompt(
                0,
                DateTime.Now,
                promptCreateDto.Name,
                promptCreateDto.Description,
                promptCreateDto.Text,
                idUser);

            return prompt;
        }

        /// <summary>
        /// Update a prompt
        /// </summary>
        /// <param name="promptDto"></param>
        /// <param name="promptUpdateDto"></param>
        private static Prompt GeneratePromptToUpdate(PromptDto promptDto, PromptUpdateDto promptUpdateDto)
        {
            var prompt = new Prompt(
                promptDto.Id,
                promptDto.Created,
                promptUpdateDto.Name,
                promptUpdateDto.Description,
                promptUpdateDto.Text,
                promptDto.IdUser,
                true);

            return prompt;
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
                dataDto.CardId);
            if (execution == null)
            {
                throw new ArgumentException("StepToolExecution not found");
            }

            var documentHistory = new DocumentHistory(execution.Card!.DocumentId,
                "Prompt",
                chatCompletionResponseDto.Choices[0].Message.Content,
                0,
                DateTime.Now);

            _unitOfWork.BeginTransaction();
            try
            {
                _documentHistoryRepository.Create(documentHistory);
                await UpdateExecutionAsync(execution!, chatCompletionResponseDto.Email);
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
        /// Updates StepToolExecution status and send notification 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task UpdateExecutionAsync(StepToolExecution execution, string email)
        {
            var count = await _stepToolExecutionRepository.ExecutionsByStepIdCountAsync(execution.StepTool!.StepId,
                execution.CardId);
            var percent = ((double)execution.StepTool.Order / count) * 100;

            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var tool = await _workflowRepository.FindToolByStepToolId(execution.StepTool.Id);

            await _hubNotifier.CardProgessAsync(email, execution.CardId, percent, execution.StepTool.StepId,
                tool != null ? tool.Name : string.Empty);
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

        public async Task<string> AiPromptRefinement(string prompt, string tenantId)
        {
            var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenantId);
            if (tenantInfo!.AiGatewayApplicationId.HasValue is false || string.IsNullOrEmpty(tenantInfo.AiGatewayKey))
            {
                throw new ArgumentException("AiGateway ApplicationId not found");
            }

            var systemPrompt = @"
Você é um Conversor Neutro de Linguagem Natural para Prompt Técnico.
Sua função NÃO é executar lógica,
NÃO é classificar,
NÃO é extrair dados,
NÃO é decidir nada.
Sua ÚNICA função é converter o texto fornecido pelo usuário em um PROMPT TÉCNICO, refletindo somente e exatamente o que está escrito no texto.

REGRA ABSOLUTA — FIDELIDADE AO TEXTO (CRÍTICO)
Você SÓ pode gerar instruções, campos e regras que estejam EXPLICITAMENTE ou IMPLICITAMENTE descritas no texto recebido.

É PROIBIDO:
reaproveitar campos de outros exemplos
assumir área de negócio
assumir tipo de documento
assumir etapa anterior
assumir nomes de campos “padrão”
completar lacunas com “bom senso”
Se o texto não pedir, não gere.

OBJETIVO
Converter um texto em linguagem natural em um PROMPT TÉCNICO, que:
reflita fielmente os objetivos descritos no texto
contenha somente os objetivos daquela solicitação
gere somente os campos necessários para esses objetivos
não carregue contexto externo
não tenha memória de outras conversões

SOBRE ETAPAS (IMPORTANTE)
O texto pode descrever uma etapa, parte de uma etapa ou múltiplas ações.
Você deve:
identificar o que o texto pede e gerar o prompt somente para isso

NÃO assuma que:
existe Fase 1
existe Fase 2
existe pipeline
existe histórico

Se o texto não disser, não existe.

SOBRE CAMPOS (REGRA CRÍTICA)
Você deve gerar uma LISTA FECHADA DE CAMPOS, criada exclusivamente a partir do texto.

Regras obrigatórias:
NÃO inventar campos
NÃO repetir campos de outros exemplos
NÃO gerar campos vazios
NÃO gerar listas ([])
NÃO gerar objetos ({})
NÃO gerar booleanos (true/false, sim/não)

Se não houver evidência → campo não existe

Cada campo deve conter UM ÚNICO VALOR FACTUAL.

PROIBIÇÕES ABSOLUTAS

É EXPRESSAMENTE PROIBIDO:
criar campos de status (ex: alto_risco, aprovado)
criar campos de decisão
criar campos genéricos (“outros”, “detalhes”)
criar campos não citados no texto
reutilizar exemplos anteriores como regra

FORMATO DO PROMPT GERADO (OBRIGATÓRIO)
O prompt técnico gerado deve seguir exatamente este formato:
Você é um motor responsável por [descrever exatamente o que o texto pede].

Sua função é executar somente os objetivos abaixo, com base no conteúdo analisado:
[objetivo 1 exatamente como inferido do texto]
[objetivo 2 exatamente como inferido do texto]
[objetivo 3 se existir]

REGRAS DE FORMATAÇÃO (CRÍTICO):
Responda APENAS com o objeto JSON cru.
NÃO utilize blocos de código markdown.
NÃO inclua textos introdutórios ou conclusivos.
NÃO gere listas.
NÃO gere objetos JSON dentro de campos.
NÃO gere campos booleanos.
NÃO gere campos fora da lista permitida.
NÃO gere campos sem evidência explícita no conteúdo.
CAMPOS PERMITIDOS (GERADOS A PARTIR DO TEXTO)

O JSON pode conter APENAS os campos abaixo:
""[campo_1]"" — criado porque o texto pede explicitamente
""[campo_2]"" — criado porque o texto pede explicitamente
""[campo_3]"" — criado porque o texto pede explicitamente

Se não houver evidência para um campo, NÃO CRIE O CAMPO.

Saída Obrigatória (EXEMPLO):
{
""[campo_1]"": ""Valor"",
""[campo_2]"": ""Valor""
}

EXEMPLOS (APENAS COMO REFERÊNCIA, NÃO COMO REGRA)

Os exemplos abaixo NÃO devem ser reaproveitados automaticamente.
Eles servem apenas para ilustrar estilo de texto humano.

(No nosso dia a dia chega muito documento por e-mail…
[texto completo fornecido pelo usuário])

FORMATAÇÃO DA SUA RESPOSTA (CRÍTICO)
Retorne APENAS o texto do prompt técnico
NÃO use markdown
NÃO inclua exemplos de execução
Texto pronto para copiar e colar
Texto a ser convertido: {{Regra de negócio}}";

            var fullPrompt = systemPrompt.Replace("{{Regra de negócio}}", prompt);

            var chatCompletionDto = new ChatCompletionDto
            {
                Temperature = _chatCompletionSettings.Temperature,
                MaxTokens = _chatCompletionSettings.MaxTokens,
                Messages = new List<ChatMessageDto> { new ChatMessageDto { Role = "system", Content = fullPrompt } }
            };

            var response = await _chatCompletionApi.GetChatCompletion(
                tenantInfo.AiGatewayApplicationId.Value.ToString(),
                _chatCompletionSettings.Model,
                _chatCompletionSettings.ApiVersion,
                tenantInfo.AiGatewayKey,
                chatCompletionDto);

            return response.Choices[0].Message.Content;
        }
    }
}