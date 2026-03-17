using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Services.Audit;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Fetches raw audit data from the repository and builds document, workflow, and user audit DTOs.
    /// </summary>
    public class AuditorServices : IAuditorServices
    {
        private readonly IAuditorRepository _auditorRepository;
        private readonly ILogger<AuditorServices> _logger;

        /// <summary>
        /// Initializes the auditor service with the auditor repository and logger.
        /// </summary>
        public AuditorServices(IAuditorRepository auditorRepository, ILogger<AuditorServices> logger)
        {
            _auditorRepository = auditorRepository;
            _logger = logger;
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> documents with document audit summary: DocumentId, DocumentName, Workflows (id, name, step), ActionsCount, IsFinalized. Optional search (document name, workflow name, or numeric id) and isFinalized filter. Ordered by most recent audit activity.
        /// </summary>
        public async Task<ICollection<DocumentAuditorSummaryDto>> FindDocumentsAuditSummaryAsync(int take, string? search, bool? isFinalized = null)
        {
            try
            {
                var documentIds = await _auditorRepository.FindDocumentIdsForDocumentsSummaryAsync(take, search, isFinalized);
                if (documentIds.Count == 0)
                    return new List<DocumentAuditorSummaryDto>();

                var auditRows = await _auditorRepository.FindAuditRowsForDocumentsSummaryAsync(documentIds, search, isFinalized);

                var isFinalizedByDocument = auditRows
                    .GroupBy(a => a.DocumentId)
                    .ToDictionary(g => g.Key, g =>
                    {
                        var statusesInDoc = g.GroupBy(x => x.CardId).Select(x => x.First().CardStatusName).ToList();
                        return statusesInDoc.Count > 0 && statusesInDoc.All(s => s == StatusNames.Finalize);
                    });

                var groupedByDocument = auditRows
                    .GroupBy(a => a.DocumentId)
                    .OrderBy(g => g.Key)
                    .ToList();

                return groupedByDocument.Select(g =>
                {
                    var first = g.First();
                    var workflows = g
                        .Where(x => x.WorkflowId != 0)
                        .Select(x => new { x.WorkflowId, x.WorkflowName, x.StepId, x.StepName, x.DocumentId })
                        .GroupBy(x => new { x.WorkflowId, x.WorkflowName, x.StepId, x.StepName })
                        .Select(x => x.First())
                        .Select(x => new DocumentAuditorWorkflowsDto
                        {
                            Id = x.WorkflowId,
                            Name = x.WorkflowName,
                            StepId = x.StepId,
                            StepName = x.StepName,
                            DocumentId = x.DocumentId
                        })
                        .ToList();
                    return new DocumentAuditorSummaryDto
                    {
                        DocumentId = g.Key,
                        DocumentName = first.DocumentName,
                        Workflows = workflows,
                        ActionsCount = g.Count(),
                        IsFinalized = isFinalizedByDocument.TryGetValue(g.Key, out var finalized) && finalized
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AuditorServices)} in the {nameof(FindDocumentsAuditSummaryAsync)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Returns document audit detail for a document and workflow: DocumentId, DocumentName, WorkflowId, WorkflowName, DocumentHistory (user, action, step, created). Optional filters: search (user/document/action/step name), userId, actionType, stepId; limited by <paramref name="take"/>. Returns null when no audit rows exist.
        /// </summary>
        public async Task<DocumentAuditorDetailDto?> FindDocumentAuditDetailsAsync(int documentId, int workflowId, int take, string? search = null, Guid? userId = null, int? actionType = null, int? stepId = null, bool orderDescending = true)
        {
            try
            {
                var rows = await _auditorRepository.FindAuditRowsForDocumentDetailAsync(documentId, workflowId, take, search, userId, actionType, stepId, orderDescending);
                if (rows.Count == 0)
                    return null;

                var first = rows.First();
                var documentHistory = rows.Select(a => new DocumentAuditorHistoryEntryDto
                {
                    UserId = a.UserId,
                    UserName = a.UserName,
                    ActionName = a.ActionName,
                    StepId = a.StepId,
                    StepName = a.StepName,
                    Created = a.Created
                }).ToList();

                return new DocumentAuditorDetailDto
                {
                    DocumentId = documentId,
                    DocumentName = first.DocumentName,
                    WorkflowId = workflowId,
                    WorkflowName = first.WorkflowName,
                    DocumentHistory = documentHistory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AuditorServices)} in the {nameof(FindDocumentAuditDetailsAsync)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Returns workflow audit summaries (one per workflow): WorkflowId, WorkflowName, DocumentCount, LogsCount, TeamId, TeamName, ProfileId, ProfileName. Up to <paramref name="take"/> workflows by most recent activity; optional search by workflow or team name.
        /// </summary>
        public async Task<ICollection<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync(int take = 10, string? search = null)
        {
            try
            {
                var workflowList = await _auditorRepository.FindWorkflowIdsForWorkflowSummaryAsync(take, search);
                if (workflowList.Count == 0)
                    return new List<WorkflowAuditorSummaryDto>();

                var auditRows = await _auditorRepository.FindAuditRowsForWorkflowSummaryAsync(workflowList);

                var groupedByWorkflow = auditRows
                    .GroupBy(a => a.WorkflowId)
                    .OrderBy(g => g.Key)
                    .ToList();

                return groupedByWorkflow.Select(g =>
                {
                    var first = g.First();
                    return new WorkflowAuditorSummaryDto
                    {
                        WorkflowId = g.Key,
                        WorkflowName = first.WorkflowName,
                        DocumentCount = g.Select(a => a.DocumentId).Distinct().Count(),
                        LogsCount = g.Count(),
                        TeamId = first.TeamId,
                        TeamName = first.TeamName,
                        ProfileId = first.ProfileId,
                        ProfileName = first.ProfileName
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AuditorServices)} in the {nameof(FindWorkflowAuditSummaryAsync)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Returns full workflow audit details: WorkflowId, WorkflowName, LogCount, StepsCount (step id/name and document count per step), DocumentStatusCount (total/finalized/rejected), and Cards (card audit history). Optional filters: search (user/card/step/action), stepId, actionType. Returns null when the workflow has no audit entries.
        /// </summary>
        public async Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId, string? search = null, int? stepId = null, int? actionType = null, bool orderDescending = true)
        {
            try
            {
                var auditRows = await _auditorRepository.FindAuditRowsForWorkflowDetailsAsync(workflowId, search, stepId, actionType, orderDescending);
                if (auditRows.Count == 0)
                    return null;

                var first = auditRows.First();
                var distinctDocumentIds = auditRows.Select(a => a.DocumentId).Distinct().ToList();

                var cards = auditRows
                    .Select(a => new WorkflowAuditorCardsDto
                    {
                        CardId = a.CardId,
                        CardName = a.CardName,
                        CardStatus = a.CardStatus,
                        StepId = a.StepId,
                        StepName = a.StepName,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        ActionType = a.ActionType.ToString(),
                        Created = a.Created
                    })
                    .ToList();

                var stepsCount = auditRows
                    .GroupBy(a => new { a.StepId, a.StepName })
                    .Select(g => new WorkflowAuditorStepCountsDto
                    {
                        StepId = g.Key.StepId,
                        StepName = g.Key.StepName,
                        DocumentCount = g.Select(a => a.DocumentId).Distinct().Count()
                    })
                    .OrderBy(s => s.StepId)
                    .ToList();

                var latestStatusPerCard = auditRows
                    .GroupBy(a => new { a.DocumentId, a.CardId })
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Created).First().CardStatus);

                var statusesByDocument = auditRows
                    .Select(a => new { a.DocumentId, a.CardId })
                    .Distinct()
                    .GroupBy(x => x.DocumentId)
                    .ToDictionary(g => g.Key, g => g.Select(x => latestStatusPerCard[new { x.DocumentId, x.CardId }]).ToList());
                var finalized = statusesByDocument.Count(kv => kv.Value.Count > 0 && kv.Value.All(s => s == StatusNames.Finalize));
                var rejected = statusesByDocument.Count(kv => kv.Value.Any(s => s == StatusNames.Rejected));

                var documentStatusCount = new WorkflowAuditorDocumentStatusCountDto
                {
                    TotalDocuments = distinctDocumentIds.Count,
                    Finalized = finalized,
                    Rejected = rejected
                };

                return new WorkflowAuditorDetailsDto
                {
                    WorkflowId = first.WorkflowId,
                    WorkflowName = first.WorkflowName,
                    LogCount = auditRows.Count,
                    StepsCount = stepsCount,
                    DocumentStatusCount = documentStatusCount,
                    Cards = cards
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AuditorServices)} in the {nameof(FindWorkflowAuditDetailsAsync)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Returns user audit summaries (one per user): UserId, UserName, Teams, Profiles, WorkflowCount, LogCount. Up to <paramref name="take"/> users; optional filters by userName and teamId.
        /// </summary>
        public async Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int take = 10, string? userName = null, int? teamId = null)
        {
            try
            {
                var userIds = await _auditorRepository.FindUserIdsForUserSummaryAsync(take, userName, teamId);
                if (userIds.Count == 0)
                    return new List<UserAuditorSummaryDto>();

                var auditRows = await _auditorRepository.FindAuditRowsForUserSummaryAsync(userIds);

                var groupedByUser = auditRows
                    .GroupBy(a => a.UserId)
                    .OrderBy(g => g.Key)
                    .ToList();

                return groupedByUser.Select(g =>
                {
                    var first = g.First();
                    var allTeams = g.SelectMany(a => a.Teams ?? Enumerable.Empty<UsersAuditorTeamsDto>());
                    var distinctTeams = allTeams
                        .GroupBy(t => new { t.TeamId, t.TeamName })
                        .Select(x => x.First())
                        .ToList();
                    var distinctProfiles = g
                        .Where(a => a.ProfileId.HasValue)
                        .Select(a => new UsersAuditorProfilesDto { ProfileId = a.ProfileId!.Value, ProfileName = a.ProfileName })
                        .GroupBy(p => new { p.ProfileId, p.ProfileName })
                        .Select(x => x.First())
                        .ToList();
                    return new UserAuditorSummaryDto
                    {
                        UserId = g.Key,
                        UserName = first.UserName,
                        Teams = distinctTeams,
                        Profiles = distinctProfiles,
                        WorkflowCount = g.Select(a => a.WorkflowId).Distinct().Count(),
                        LogCount = g.Count()
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AuditorServices)} in the {nameof(FindUserAuditSummaryAsync)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Returns full user audit details: UserId, UserName, Teams, Profiles, LogCountTotal, LogCountByActionType, Actions (card, action type, workflow, created). Optional filters: search (card/workflow/action name), actionTypeCode. Returns null when the user has no audit entries.
        /// </summary>
        public async Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, string? search = null, int? actionTypeCode = null, bool orderDescending = true)
        {
            try
            {
                var auditRows = await _auditorRepository.FindAuditRowsForUserDetailsAsync(userId, search, actionTypeCode, orderDescending);
                if (auditRows.Count == 0)
                    return null;

                var first = auditRows.First();
                var distinctTeams = auditRows
                    .SelectMany(a => a.Teams ?? Enumerable.Empty<UsersAuditorTeamsDto>())
                    .GroupBy(t => new { t.TeamId, t.TeamName })
                    .Select(x => x.First())
                    .ToList();
                var distinctProfiles = auditRows
                    .Where(a => a.ProfileId.HasValue)
                    .Select(a => new UsersAuditorProfilesDto { ProfileId = a.ProfileId!.Value, ProfileName = a.ProfileName })
                    .GroupBy(p => new { p.ProfileId, p.ProfileName })
                    .Select(x => x.First())
                    .ToList();
                var countByActionType = auditRows
                    .GroupBy(a => (int)a.ActionType)
                    .Select(g => new UsersAuditorActionTypeCountsDto { ActionTypeCode = g.Key, Count = g.Count() })
                    .OrderBy(x => x.ActionTypeCode)
                    .ToList();
                var actions = auditRows
                    .Select(a => new UsersAuditorActionsDto
                    {
                        CardId = a.CardId,
                        CardName = a.CardName,
                        ActionType = a.ActionType.ToString(),
                        WorkflowId = a.WorkflowId,
                        WorkflowName = a.WorkflowName,
                        Created = a.Created
                    })
                    .ToList();

                return new UserAuditorDetailsDto
                {
                    UserId = first.UserId,
                    UserName = first.UserName,
                    Teams = distinctTeams,
                    Profiles = distinctProfiles,
                    LogCountTotal = auditRows.Count,
                    LogCountByActionType = countByActionType,
                    Actions = actions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(AuditorServices)} in the {nameof(FindUserAuditDetailsAsync)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }
    }
}
