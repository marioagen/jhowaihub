using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Application.Utils.WorkflowTemplate;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.WorkflowTemplate;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class WorkflowTemplateServices : IWorkflowTemplateServices
    {
        private const string NotFoundMessage = "Workflow template not found";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowServices _workflowServices;
        private readonly ITeamRepository _teamRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IToolRepository _toolRepository;
        private readonly IPromptRepository _promptRepository;
        private readonly IApiTemplateRepository _apiTemplateRepository;
        private readonly IUserServices _userServices;
        private readonly IEncryptionService _encryptionService;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private readonly IConfiguration _configuration;
        private readonly WorkflowTemplateSettings _settings;
        private readonly ILogger<WorkflowTemplateServices> _logger;

        public WorkflowTemplateServices(
            IWorkflowRepository workflowRepository,
            IWorkflowServices workflowServices,
            ITeamRepository teamRepository,
            IProfileRepository profileRepository,
            IStatusRepository statusRepository,
            IToolRepository toolRepository,
            IPromptRepository promptRepository,
            IApiTemplateRepository apiTemplateRepository,
            IUserServices userServices,
            IEncryptionService encryptionService,
            IFunctionFileRetriever functionFileRetriever,
            IConfiguration configuration,
            IOptions<WorkflowTemplateSettings> settings,
            ILogger<WorkflowTemplateServices> logger)
        {
            _workflowRepository = workflowRepository;
            _workflowServices = workflowServices;
            _teamRepository = teamRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _toolRepository = toolRepository;
            _promptRepository = promptRepository;
            _apiTemplateRepository = apiTemplateRepository;
            _userServices = userServices;
            _encryptionService = encryptionService;
            _functionFileRetriever = functionFileRetriever;
            _configuration = configuration;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<List<WorkflowTemplateListItemDto>> FindTemplatesAsync(string? query, string? orderBy)
        {
            var templates = await LoadAllPackagesAsync();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLowerInvariant();
                templates = templates.Where(t =>
                    t.Name.ToLowerInvariant().Contains(lowerQuery) ||
                    t.Description.ToLowerInvariant().Contains(lowerQuery) ||
                    t.Category.ToLowerInvariant().Contains(lowerQuery)).ToList();
            }

            templates = orderBy?.ToLowerInvariant() switch
            {
                "name_asc" => templates.OrderBy(t => t.Name).ToList(),
                "name_desc" => templates.OrderByDescending(t => t.Name).ToList(),
                "created_asc" => templates.OrderBy(t => t.Created).ToList(),
                _ => templates.OrderByDescending(t => t.Created).ToList()
            };

            return templates.Select(t => new WorkflowTemplateListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                Version = t.Version,
                Created = t.Created,
                StepCount = t.Steps.Count,
                TeamNames = t.TeamNames,
                RequiredSecrets = t.RequiredSecrets
            }).ToList();
        }

        public async Task<WorkflowTemplatePackageDto?> FindTemplateByIdAsync(Guid id)
        {
            var templates = await LoadAllPackagesAsync();
            return templates.FirstOrDefault(t => t.Id == id);
        }

        public async Task<WorkflowTemplatePackageDto> ExportAsync(int workflowId)
        {
            var phase1 = await _workflowServices.FindPhase1ById(workflowId);
            var phase2 = await _workflowServices.FindPhase2ById(workflowId);
            var phase3 = await _workflowServices.FindPhase3ById(workflowId);

            var prompts = new Dictionary<string, WorkflowTemplatePromptDto>();
            var apiTemplates = new Dictionary<string, WorkflowTemplateApiTemplateDto>();

            var steps = new List<WorkflowTemplateStepDto>();
            foreach (var step in phase2.OrderBy(s => s.Order))
            {
                var phase3Step = phase3.FirstOrDefault(s => s.Order == step.Order);
                var stepTools = new List<WorkflowTemplateStepToolDto>();

                if (phase3Step?.StepTools != null)
                {
                    foreach (var st in phase3Step.StepTools.OrderBy(t => t.Order))
                    {
                        var toolType = st.Tool?.ToolType ?? string.Empty;
                        var parameters = new List<WorkflowTemplateParameterDto>();

                        foreach (var param in st.Parameters)
                        {
                            var exported = await ExportParameterAsync(toolType, param, prompts, apiTemplates);
                            if (exported != null)
                                parameters.Add(exported);
                        }

                        stepTools.Add(new WorkflowTemplateStepToolDto
                        {
                            Order = st.Order,
                            ToolType = toolType,
                            PositionX = st.PositionX,
                            PositionY = st.PositionY,
                            Dependencies = st.Dependencies?.Select(d => new WorkflowTemplateDependencyDto
                            {
                                StepOrder = d.StepOrder,
                                StepToolOrder = d.StepToolOrder
                            }).ToList() ?? [],
                            Parameters = parameters
                        });
                    }
                }

                steps.Add(new WorkflowTemplateStepDto
                {
                    Order = step.Order,
                    Name = step.Name,
                    ProfileName = step.Profile.Name,
                    ProfileCode = WorkflowTemplateCanonicalMapper.ToProfileCode(step.Profile.Name),
                    StatusName = step.Status.Name,
                    StatusCode = WorkflowTemplateCanonicalMapper.ToStatusCode(step.Status.Name),
                    StepTools = stepTools
                });
            }

            var package = new WorkflowTemplatePackageDto
            {
                SchemaVersion = "1.1",
                Id = Guid.NewGuid(),
                Name = phase1.Name,
                Description = phase1.Description,
                Category = "Exportado",
                Version = "1.0.0",
                Created = DateTime.UtcNow,
                TeamNames = phase1.Teams.Select(t => t.Name).ToList(),
                TeamCodes = phase1.Teams.Select(t => WorkflowTemplateCanonicalMapper.ToTeamCode(t.Name)).ToList(),
                Steps = steps,
                Prompts = prompts.Values.ToList(),
                ApiTemplates = apiTemplates.Values.ToList()
            };

            SanitizeExportPackage(package);
            return package;
        }

        public async Task<List<WorkflowTemplateImportResultDto>> ImportByIdsAsync(
            WorkflowTemplateImportRequestDto request,
            string email)
        {
            if (request.TemplateIds == null || request.TemplateIds.Count == 0)
                return [];

            var results = new List<WorkflowTemplateImportResultDto>();
            foreach (var templateId in request.TemplateIds)
            {
                var package = await FindTemplateByIdAsync(templateId)
                    ?? throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);

                ValidateSecretsResolved(package, request.SecretValues);

                var workflowId = await ImportPackageAsync(package, email, request.SecretValues);
                results.Add(new WorkflowTemplateImportResultDto
                {
                    TemplateId = templateId,
                    WorkflowId = workflowId,
                    Name = package.Name
                });
            }

            return results;
        }

        private async Task<int> ImportPackageAsync(
            WorkflowTemplatePackageDto package,
            string email,
            IReadOnlyDictionary<string, string> secretValues)
        {
            var userId = _userServices.FindIdByEmail(email);
            if (userId == Guid.Empty)
                throw new AppException(ErrorCode.NotFound, "User not found", null);

            var promptRefMap = await ImportPromptsAsync(package.Prompts, userId);
            var apiRefMap = await ImportApiTemplatesAsync(package.ApiTemplates, secretValues);
            var toolTypeMap = await BuildToolTypeMapAsync();

            var teamIds = ResolveTeamIdsFromPackage(package);
            var profiles = (await _profileRepository.FindAll()).Select(p => (p.Id, p.Name)).ToList();
            var statuses = (await _statusRepository.FindAll()).Select(s => (s.Id, s.Name)).ToList();

            var workflowName = await ResolveUniqueWorkflowNameAsync(package.Name);

            var workflowId = await _workflowServices.CreatePhase1(new WorkflowPhase1Dto
            {
                Name = workflowName,
                Description = package.Description,
                Teams = teamIds
            });

            var phase2Steps = package.Steps.OrderBy(s => s.Order).Select(s => new StepPhase2Dto
            {
                Id = 0,
                Name = s.Name,
                Order = s.Order,
                ProfileId = WorkflowTemplateCanonicalMapper.ResolveProfileId(
                    profiles, s.ProfileCode, s.ProfileName),
                StatusId = WorkflowTemplateCanonicalMapper.ResolveStatusId(
                    statuses, s.StatusCode, s.StatusName)
            }).ToList();

            await _workflowServices.UpdatePhase2(new WorkflowPhase2Dto
            {
                WorkflowId = workflowId,
                Steps = phase2Steps,
                HasStepTool = package.Steps.Any(s => s.StepTools.Count > 0)
            }, new HeadersDto());

            var createdSteps = await _workflowServices.FindPhase2ById(workflowId);
            var phase3Steps = BuildPhase3Steps(package, createdSteps, toolTypeMap, promptRefMap, apiRefMap, secretValues);

            await _workflowServices.UpdatePhase3(new WorkflowPhase3Dto
            {
                WorkflowId = workflowId,
                Steps = phase3Steps,
                ResetDocuments = false
            }, new HeadersDto());

            return workflowId;
        }

        private List<StepPhase3Dto> BuildPhase3Steps(
            WorkflowTemplatePackageDto package,
            List<StepDto> createdSteps,
            Dictionary<string, int> toolTypeMap,
            Dictionary<string, int> promptRefMap,
            Dictionary<string, int> apiRefMap,
            IReadOnlyDictionary<string, string> secretValues)
        {
            var steps = new List<StepPhase3Dto>();

            foreach (var templateStep in package.Steps.OrderBy(s => s.Order))
            {
                var dbStep = createdSteps.FirstOrDefault(s => s.Order == templateStep.Order)
                    ?? throw new AppException(ErrorCode.NotFound, $"Step order {templateStep.Order} not found", StepLabel.NotFound);

                var stepTools = templateStep.StepTools.OrderBy(st => st.Order).Select(st =>
                {
                    if (!toolTypeMap.TryGetValue(st.ToolType, out var toolId))
                        throw new AppException(ErrorCode.NotFound, $"Tool type '{st.ToolType}' not found", ToolLabel.NotFound);

                    return new StepToolUpdateDto
                    {
                        Id = 0,
                        StepId = dbStep.Id,
                        ToolId = toolId,
                        Order = st.Order,
                        PositionX = st.PositionX,
                        PositionY = st.PositionY,
                        Dependencies = st.Dependencies.Select(d => new StepToolOutputDependencyDto
                        {
                            StepOrder = d.StepOrder,
                            StepToolOrder = d.StepToolOrder
                        }).ToList(),
                        Parameters = st.Parameters.Select(p => new StepToolParameterUpdateDto
                        {
                            Value = RemapParameterValue(st.ToolType, p.Value, promptRefMap, apiRefMap, secretValues),
                            RequiredFile = p.RequiredFile,
                            WebhookId = p.WebhookId
                        }).ToList()
                    };
                }).ToList();

                var stepOrder = dbStep.Order;
                steps.Add(new StepPhase3Dto
                {
                    Id = dbStep.Id,
                    Order = stepOrder,
                    StepTools = stepTools
                });
            }

            return steps;
        }

        private string RemapParameterValue(
            string toolType,
            string value,
            Dictionary<string, int> promptRefMap,
            Dictionary<string, int> apiRefMap,
            IReadOnlyDictionary<string, string> secretValues)
        {
            value = WorkflowTemplateSecretSanitizer.ApplySecrets(value, secretValues);

            if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                if (promptRefMap.TryGetValue(value, out var promptId))
                    return promptId.ToString();
                if (int.TryParse(value, out _))
                    return value;
                throw new AppException(ErrorCode.NotFound, $"Prompt ref '{value}' not found", null);
            }

            if (string.Equals(toolType, HandlersTypes.API, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var node = JsonNode.Parse(value)?.AsObject()
                        ?? throw new AppException(ErrorCode.InvalidValue, "Invalid API parameter JSON", null);

                    if (node.TryGetPropertyValue("apiTemplateRef", out var refNode))
                    {
                        var apiRef = refNode?.GetValue<string>() ?? string.Empty;
                        if (!apiRefMap.TryGetValue(apiRef, out var templateId))
                            throw new AppException(ErrorCode.NotFound, $"API template ref '{apiRef}' not found", null);

                        node["templateId"] = templateId;
                        node.Remove("apiTemplateRef");
                        return node.ToJsonString(JsonOptions);
                    }

                    if (node.TryGetPropertyValue("templateId", out var templateIdNode)
                        && int.TryParse(templateIdNode?.GetValue<string>(), out var existingId))
                    {
                        return value;
                    }
                }
                catch (AppException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AppException(ErrorCode.InvalidValue, ex.Message, null);
                }
            }

            return value;
        }

        private async Task<Dictionary<string, int>> ImportPromptsAsync(
            List<WorkflowTemplatePromptDto> prompts,
            Guid userId)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var templatePrompt in prompts)
            {
                var name = templatePrompt.Name.Length > 50
                    ? templatePrompt.Name[..50]
                    : templatePrompt.Name;
                var description = templatePrompt.Description.Length > 500
                    ? templatePrompt.Description[..500]
                    : templatePrompt.Description;

                var existing = _promptRepository.FindByNameAndUser(name, userId);
                if (existing != null)
                {
                    map[templatePrompt.Ref] = existing.Id;
                    continue;
                }

                var prompt = new Prompt(
                    0,
                    DateTime.UtcNow,
                    name,
                    description,
                    templatePrompt.Text,
                    userId,
                    isEdited: false,
                    isImported: true,
                    enableAccessToMcp: templatePrompt.EnableAccessToMcp);

                var created = _promptRepository.CreateAndReturn(prompt)
                    ?? throw new AppException(ErrorCode.DefaultError, "Failed to create prompt", null);

                map[templatePrompt.Ref] = created.Id;
            }

            return map;
        }

        private async Task<Dictionary<string, int>> ImportApiTemplatesAsync(
            List<WorkflowTemplateApiTemplateDto> apiTemplates,
            IReadOnlyDictionary<string, string> secretValues)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var template in apiTemplates)
            {
                var sanitizedName = template.Name;
                var url = WorkflowTemplateSecretSanitizer.ApplySecrets(template.Url, secretValues);
                var query = WorkflowTemplateApiTemplateNormalizer.NormalizeKeyValueTemplate(
                    WorkflowTemplateSecretSanitizer.ApplySecrets(template.QueryTemplate, secretValues));
                var header = WorkflowTemplateApiTemplateNormalizer.NormalizeKeyValueTemplate(
                    WorkflowTemplateSecretSanitizer.ApplySecrets(template.HeaderTemplate, secretValues));
                var body = WorkflowTemplateSecretSanitizer.ApplySecrets(template.BodyTemplate, secretValues);

                var all = await _apiTemplateRepository.FindAll(new ApiTemplateFilterDto());
                var existing = all.FirstOrDefault(a =>
                    a.Name.Equals(sanitizedName, StringComparison.OrdinalIgnoreCase));

                if (existing?.Id is int existingId)
                {
                    await UpdateImportedApiTemplateAsync(existingId, url, query, header, body);
                    map[template.Ref] = existingId;
                    continue;
                }

                var entity = new ApiTemplate(
                    sanitizedName,
                    template.Method,
                    url,
                    query,
                    header,
                    body,
                    template.Description,
                    template.EnableAccessFromMcp);

                await _apiTemplateRepository.CreateAsync(entity);
                map[template.Ref] = entity.Id;
            }

            return map;
        }

        private async Task UpdateImportedApiTemplateAsync(
            int templateId,
            string url,
            string? query,
            string? header,
            string? body)
        {
            var entity = await _apiTemplateRepository.FindByIdReturnModel(templateId);
            if (entity == null)
                return;

            entity.UpdateUrl(url);
            entity.UpdateQueryTemplate(query);
            entity.UpdateHeaderTemplate(header);
            entity.UpdateBodyTemplate(body);
            await _apiTemplateRepository.UpdateAsync(entity);
        }

        private async Task<Dictionary<string, int>> BuildToolTypeMapAsync()
        {
            var tools = await _toolRepository.FindAllAsync();
            return tools
                .Where(t => !string.IsNullOrWhiteSpace(t.ToolType))
                .GroupBy(t => t.ToolType, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<string> ResolveUniqueWorkflowNameAsync(string baseName)
        {
            var existing = _workflowServices.FindAll();
            if (existing.All(w => !w.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
                return baseName;

            var suffix = 2;
            while (existing.Any(w => w.Name.Equals($"{baseName} ({suffix})", StringComparison.OrdinalIgnoreCase)))
                suffix++;

            return $"{baseName} ({suffix})";
        }

        private async Task<WorkflowTemplateParameterDto?> ExportParameterAsync(
            string toolType,
            StepToolParameterDto param,
            Dictionary<string, WorkflowTemplatePromptDto> prompts,
            Dictionary<string, WorkflowTemplateApiTemplateDto> apiTemplates)
        {
            if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(param.Value, out var promptId))
                    return null;

                var prompt = _promptRepository.FindById(promptId);
                if (prompt == null)
                    return null;

                var promptRef = $"prompt-{promptId}";
                if (!prompts.ContainsKey(promptRef))
                {
                    prompts[promptRef] = new WorkflowTemplatePromptDto
                    {
                        Ref = promptRef,
                        Name = prompt.Name,
                        Description = prompt.Description,
                        Text = prompt.Text,
                        EnableAccessToMcp = prompt.EnableAccessToMcp
                    };
                }

                return new WorkflowTemplateParameterDto
                {
                    Value = promptRef,
                    RequiredFile = param.RequiredFile,
                    WebhookId = param.WebhookId
                };
            }

            if (string.Equals(toolType, HandlersTypes.API, StringComparison.OrdinalIgnoreCase))
            {
                var decrypted = _encryptionService.IsEncrypted(param.Value)
                    ? _encryptionService.Decrypt(param.Value)
                    : param.Value;

                var request = JsonSerializer.Deserialize<ApiRequestDto>(decrypted, JsonOptions);
                if (request == null)
                    return null;

                var apiRef = $"api-{request.TemplateId}";
                if (!apiTemplates.ContainsKey(apiRef))
                {
                    var template = await _apiTemplateRepository.FindByIdReturnModel(request.TemplateId);
                    if (template != null)
                    {
                        apiTemplates[apiRef] = new WorkflowTemplateApiTemplateDto
                        {
                            Ref = apiRef,
                            Name = template.Name,
                            Method = template.Method,
                            Url = template.Url,
                            QueryTemplate = template.QueryTemplate,
                            HeaderTemplate = template.HeaderTemplate,
                            BodyTemplate = template.BodyTemplate,
                            Description = template.Description,
                            EnableAccessFromMcp = template.EnableAccessFromMcp
                        };
                    }
                }

                var node = JsonNode.Parse(decrypted)?.AsObject();
                if (node != null)
                {
                    node["apiTemplateRef"] = apiRef;
                    node.Remove("templateId");
                    decrypted = node.ToJsonString(JsonOptions);
                }

                return new WorkflowTemplateParameterDto
                {
                    Value = decrypted,
                    RequiredFile = param.RequiredFile,
                    WebhookId = param.WebhookId
                };
            }

            return new WorkflowTemplateParameterDto
            {
                Value = param.Value,
                RequiredFile = param.RequiredFile,
                WebhookId = string.Equals(toolType, HandlersTypes.N8N, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : param.WebhookId
            };
        }

        private static void SanitizeExportPackage(WorkflowTemplatePackageDto package)
        {
            var allSecrets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var api in package.ApiTemplates)
            {
                var baseKey = $"api_{SlugRef(api.Ref)}";
                SanitizeApiTemplateField(api, t => t.Url, (t, v) => t.Url = v, $"{baseKey}_url", allSecrets);
                SanitizeApiTemplateField(api, t => t.QueryTemplate, (t, v) => t.QueryTemplate = v, $"{baseKey}_query", allSecrets);
                SanitizeApiTemplateField(api, t => t.HeaderTemplate, (t, v) => t.HeaderTemplate = v, $"{baseKey}_header", allSecrets);
                SanitizeApiTemplateField(api, t => t.BodyTemplate, (t, v) => t.BodyTemplate = v, $"{baseKey}_body", allSecrets);
            }

            foreach (var step in package.Steps)
            {
                foreach (var tool in step.StepTools)
                {
                    if (!string.Equals(tool.ToolType, HandlersTypes.API, StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (var parameter in tool.Parameters)
                    {
                        var (sanitized, secrets) = WorkflowTemplateSecretSanitizer.SanitizeText(
                            parameter.Value,
                            $"step_{step.Order}_tool_{tool.Order}");
                        parameter.Value = sanitized;
                        allSecrets.UnionWith(secrets);
                    }
                }
            }

            package.RequiredSecrets = allSecrets.OrderBy(x => x).ToList();
        }

        private static void SanitizeApiTemplateField(
            WorkflowTemplateApiTemplateDto api,
            Func<WorkflowTemplateApiTemplateDto, string?> getter,
            Action<WorkflowTemplateApiTemplateDto, string?> setter,
            string secretKeyBase,
            HashSet<string> allSecrets)
        {
            var (sanitized, secrets) = WorkflowTemplateSecretSanitizer.SanitizeText(getter(api), secretKeyBase);
            setter(api, sanitized);
            allSecrets.UnionWith(secrets);
        }

        private static void ValidateSecretsResolved(
            WorkflowTemplatePackageDto package,
            IReadOnlyDictionary<string, string> secretValues)
        {
            var texts = CollectSecretTexts(package);
            var required = package.RequiredSecrets.Count > 0
                ? package.RequiredSecrets
                : WorkflowTemplateSecretSanitizer.FindUnresolvedSecretsInPackage(texts);

            var missing = required
                .Where(key => !secretValues.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                .ToList();

            if (missing.Count == 0)
                return;

            throw new AppException(
                ErrorCode.RequiredField,
                $"Missing secret values for import: {string.Join(", ", missing)}",
                null);
        }

        private static List<string?> CollectSecretTexts(WorkflowTemplatePackageDto package)
        {
            var texts = new List<string?>();
            foreach (var api in package.ApiTemplates)
            {
                texts.Add(api.Url);
                texts.Add(api.QueryTemplate);
                texts.Add(api.HeaderTemplate);
                texts.Add(api.BodyTemplate);
            }

            foreach (var step in package.Steps)
            {
                foreach (var tool in step.StepTools)
                {
                    foreach (var parameter in tool.Parameters)
                        texts.Add(parameter.Value);
                }
            }

            return texts;
        }

        private List<int> ResolveTeamIdsFromPackage(WorkflowTemplatePackageDto package)
        {
            var allTeams = _teamRepository.FindAll()
                .ToList()
                .Select(t => (t.Id, t.Name))
                .ToList();
            if (allTeams.Count == 0)
                throw new AppException(ErrorCode.NotFound, "No teams found in tenant", TeamLabel.NotFound);

            var ids = new HashSet<int>();

            foreach (var code in package.TeamCodes)
            {
                var id = WorkflowTemplateCanonicalMapper.ResolveTeamId(allTeams, [code], []);
                if (id != 0)
                    ids.Add(id);
            }

            foreach (var name in package.TeamNames)
            {
                var id = WorkflowTemplateCanonicalMapper.ResolveTeamId(allTeams, [], [name]);
                if (id != 0)
                    ids.Add(id);
            }

            if (ids.Count == 0)
            {
                var fallback = WorkflowTemplateCanonicalMapper.ResolveTeamId(allTeams, ["ADMIN"], ["Admin"]);
                if (fallback != 0)
                    ids.Add(fallback);
                else
                    ids.Add(allTeams.First().Id);
            }

            return ids.ToList();
        }

        private static string SlugRef(string refValue) =>
            refValue.Replace("api-", "", StringComparison.OrdinalIgnoreCase)
                .Replace("prompt-", "", StringComparison.OrdinalIgnoreCase);

        private async Task<List<WorkflowTemplatePackageDto>> LoadAllPackagesAsync()
        {
            var local = await TryLoadFromLocalFileAsync();
            if (local.Count > 0)
                return local;

            return await TryLoadFromFunctionAsync();
        }

        private async Task<List<WorkflowTemplatePackageDto>> TryLoadFromLocalFileAsync()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, _settings.LocalCatalogPath),
                Path.Combine(Directory.GetCurrentDirectory(), _settings.LocalCatalogPath)
            };

            foreach (var path in candidates.Distinct())
            {
                if (!File.Exists(path))
                    continue;

                try
                {
                    var json = await File.ReadAllTextAsync(path);
                    var response = JsonSerializer.Deserialize<WorkflowTemplatesResponse>(json, JsonOptions);
                    return response?.Workflows ?? [];
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load workflow templates from {Path}", path);
                }
            }

            return [];
        }

        private async Task<List<WorkflowTemplatePackageDto>> TryLoadFromFunctionAsync()
        {
            try
            {
                var functionApiKeyAuth = _configuration["RefitExternalSettings:FunctionApiKey"];
                var response = await _functionFileRetriever.Get(
                    _settings.TemplateFileName,
                    functionApiKeyAuth!,
                    _settings.Folder);

                if (!response.IsSuccessStatusCode)
                    return [];

                var jsonContent = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<WorkflowTemplatesResponse>(jsonContent, JsonOptions);
                return items?.Workflows ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load workflow templates from external catalog");
                return [];
            }
        }
    }
}
