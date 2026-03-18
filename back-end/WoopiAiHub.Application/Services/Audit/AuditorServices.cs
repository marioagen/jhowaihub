using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Documents;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Users;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Workflows;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
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

        /// <summary>
        /// Initializes the auditor service with the auditor repository.
        /// </summary>
        public AuditorServices(IAuditorRepository auditorRepository)
        {
            _auditorRepository = auditorRepository;
        }

        /// <summary>
        /// Returns all audit card action types as code and name for use in filters and dropdowns.
        /// </summary>
        public Task<ICollection<AuditorActionTypeDto>> FindActionTypesAsync()
        {
            var result = Enum.GetValues<AuditCardActionType>()
                .Select(e => new AuditorActionTypeDto { Code = (int)e, Name = e.ToString() })
                .OrderBy(x => x.Code)
                .ToList();
            return Task.FromResult<ICollection<AuditorActionTypeDto>>(result);
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> documents with document audit summary: DocumentId, DocumentName, Workflows (id, name, step), ActionsCount, IsFinalized. LoadMore logic: starting at <paramref name="skip"/>; optional search and isFinalized filter. HasMore is true when more documents exist.
        /// </summary>
        public async Task<AuditorLoadMoreResultDto<DocumentAuditorSummaryDto>> FindDocumentsAuditSummaryAsync(int take, int skip, string? search, bool? isFinalized = null)
        {
            var documentIds = await _auditorRepository.FindDocumentIdsForDocumentsSummaryAsync(take + 1, skip, search, isFinalized);
            if (documentIds.Count == 0)
                return new AuditorLoadMoreResultDto<DocumentAuditorSummaryDto> { Items = new List<DocumentAuditorSummaryDto>(), HasMore = false };

            var hasMore = documentIds.Count > take;
            var idsToUse = hasMore ? documentIds.Take(take).ToList() : documentIds;

            var auditRows = await _auditorRepository.FindAuditRowsForDocumentsSummaryAsync(idsToUse, search, isFinalized);

            var isFinalizedByDocument = BuildIsFinalizedByDocument(auditRows);

            var groupedByDocument = auditRows
                .GroupBy(a => a.DocumentId)
                .OrderBy(g => g.Key)
                .ToList();

            var list = BuildDocumentAuditorSummaryList(groupedByDocument, isFinalizedByDocument);

            return new AuditorLoadMoreResultDto<DocumentAuditorSummaryDto> { Items = list, HasMore = hasMore };
        }

        /// <summary>
        /// Returns document audit detail for a document and workflow: DocumentId, DocumentName, WorkflowId, WorkflowName, DocumentHistory (user, action, step, created). Optional filters: search (user/document/action/step name), userId, actionType, stepId; limited by <paramref name="take"/>. Returns null when no audit rows exist.
        /// </summary>
        public async Task<DocumentAuditorDetailDto?> FindDocumentAuditDetailsAsync(int documentId, int workflowId, int take, string? search = null, Guid? userId = null, int? actionType = null, int? stepId = null, bool orderDescending = true)
        {
            var rows = await _auditorRepository.FindAuditRowsForDocumentDetailAsync(documentId, workflowId, take, search, userId, actionType, stepId, orderDescending);
            if (rows.Count == 0)
                return null;

            var first = rows[0];
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

        /// <summary>
        /// Returns workflow audit summaries (one per workflow): WorkflowId, WorkflowName, DocumentCount, LogsCount, TeamId, TeamName, ProfileId, ProfileName. LoadMore logic: up to <paramref name="take"/> workflows starting at <paramref name="skip"/>; optional search by workflow or team name. HasMore is true when more workflows exist.
        /// </summary>
        public async Task<AuditorLoadMoreResultDto<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync(int take = 10, int skip = 0, string? search = null)
        {
            var workflowIds = await _auditorRepository.FindWorkflowIdsForWorkflowSummaryAsync(take + 1, skip, search);
            if (workflowIds.Count == 0)
                return new AuditorLoadMoreResultDto<WorkflowAuditorSummaryDto> { Items = new List<WorkflowAuditorSummaryDto>(), HasMore = false };

            var hasMore = workflowIds.Count > take;
            var idsToUse = hasMore ? workflowIds.Take(take).ToList() : workflowIds;

            var auditRows = await _auditorRepository.FindAuditRowsForWorkflowSummaryAsync(idsToUse);

            var groupedByWorkflow = auditRows
                .GroupBy(a => a.WorkflowId)
                .OrderBy(g => g.Key)
                .ToList();

            var list = groupedByWorkflow.Select(g =>
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

            return new AuditorLoadMoreResultDto<WorkflowAuditorSummaryDto> { Items = list, HasMore = hasMore };
        }

        /// <summary>
        /// Returns full workflow audit details: WorkflowId, WorkflowName, LogCount, StepsCount (step id/name and document count per step), DocumentStatusCount (total/finalized/rejected), and Cards (card audit history, limited by take). Optional filters: search (user/card/step/action), stepId, actionType. Returns null when the workflow has no audit entries.
        /// </summary>
        public async Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId, int take, string? search = null, int? stepId = null, int? actionType = null, bool orderDescending = true)
        {
            var auditRows = await _auditorRepository.FindAuditRowsForWorkflowDetailsAsync(workflowId, search, stepId, actionType, orderDescending);
            if (auditRows.Count == 0)
                return null;

            var first = auditRows[0];
            var cards = BuildWorkflowAuditorCards(auditRows, take);
            var stepsCount = BuildWorkflowStepsCount(auditRows);
            var documentStatusCount = BuildWorkflowDocumentStatusCount(auditRows);

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

        /// <summary>
        /// Returns user audit summaries (one per user): UserId, UserName, Teams, Profiles, WorkflowCount, LogCount. Up to <paramref name="take"/> users starting at <paramref name="skip"/>; optional filters by userName and teamId. HasMore is true when more users exist.
        /// </summary>
        public async Task<AuditorLoadMoreResultDto<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int take = 10, int skip = 0, string? userName = null, int? teamId = null)
        {
            var userIds = await _auditorRepository.FindUserIdsForUserSummaryAsync(take + 1, skip, userName, teamId);
            if (userIds.Count == 0)
                return new AuditorLoadMoreResultDto<UserAuditorSummaryDto> { Items = new List<UserAuditorSummaryDto>(), HasMore = false };

            var hasMore = userIds.Count > take;
            var idsToUse = hasMore ? userIds.Take(take).ToList() : userIds;

            var auditRows = await _auditorRepository.FindAuditRowsForUserSummaryAsync(idsToUse);

            var groupedByUser = auditRows
                .GroupBy(a => a.UserId)
                .OrderBy(g => g.Key)
                .ToList();

            var list = groupedByUser.Select(g =>
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

            return new AuditorLoadMoreResultDto<UserAuditorSummaryDto> { Items = list, HasMore = hasMore };
        }

        /// <summary>
        /// Returns full user audit details: UserId, UserName, Teams, Profiles, LogCountTotal, LogCountByActionType, Actions (card, action type, workflow, created, limited by take). Optional filters: search (card/workflow/action name), actionTypeCode. Returns null when the user has no audit entries.
        /// </summary>
        public async Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, int take, string? search = null, int? actionTypeCode = null, bool orderDescending = true)
        {
            var auditRows = await _auditorRepository.FindAuditRowsForUserDetailsAsync(userId, take, search, actionTypeCode, orderDescending);
            if (auditRows.Count == 0)
                return null;

            var first = auditRows[0];
            return new UserAuditorDetailsDto
            {
                UserId = first.UserId,
                UserName = first.UserName,
                Teams = BuildDistinctTeamsFromUserDetailsRows(auditRows),
                Profiles = BuildDistinctProfilesFromUserDetailsRows(auditRows),
                LogCountTotal = auditRows.Count,
                LogCountByActionType = BuildCountByActionTypeFromUserDetailsRows(auditRows),
                Actions = BuildUserAuditorActionsList(auditRows)
            };
        }

        /// <summary>
        /// Builds distinct teams (by TeamId and TeamName) from user audit detail rows.
        /// </summary>
        private static List<UsersAuditorTeamsDto> BuildDistinctTeamsFromUserDetailsRows(IEnumerable<UserAuditorDetailsRowDto> auditRows)
        {
            return auditRows
                .SelectMany(a => a.Teams ?? Enumerable.Empty<UsersAuditorTeamsDto>())
                .GroupBy(t => new { t.TeamId, t.TeamName })
                .Select(x => x.First())
                .ToList();
        }

        /// <summary>
        /// Builds distinct profiles (by ProfileId and ProfileName) from user audit detail rows.
        /// </summary>
        private static List<UsersAuditorProfilesDto> BuildDistinctProfilesFromUserDetailsRows(IEnumerable<UserAuditorDetailsRowDto> auditRows)
        {
            return auditRows
                .Where(a => a.ProfileId.HasValue)
                .Select(a => new UsersAuditorProfilesDto { ProfileId = a.ProfileId!.Value, ProfileName = a.ProfileName })
                .GroupBy(p => new { p.ProfileId, p.ProfileName })
                .Select(x => x.First())
                .ToList();
        }

        /// <summary>
        /// Builds action type counts (code and count) from user audit detail rows, ordered by action type code.
        /// </summary>
        private static List<UsersAuditorActionTypeCountsDto> BuildCountByActionTypeFromUserDetailsRows(IEnumerable<UserAuditorDetailsRowDto> auditRows)
        {
            return auditRows
                .GroupBy(a => (int)a.ActionType)
                .Select(g => new UsersAuditorActionTypeCountsDto { ActionTypeCode = g.Key, Count = g.Count() })
                .OrderBy(x => x.ActionTypeCode)
                .ToList();
        }

        /// <summary>
        /// Builds the list of user auditor action DTOs (card, action type, workflow, created) from user audit detail rows.
        /// </summary>
        private static List<UsersAuditorActionsDto> BuildUserAuditorActionsList(IEnumerable<UserAuditorDetailsRowDto> auditRows)
        {
            return auditRows
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
        }

        /// <summary>
        /// Builds a dictionary mapping each document ID to whether all its cards are finalized (all card statuses equal Finalize).
        /// </summary>
        private static Dictionary<int, bool> BuildIsFinalizedByDocument(IEnumerable<DocumentAuditorSummaryRowDto> auditRows)
        {
            return auditRows
                .GroupBy(a => a.DocumentId)
                .ToDictionary(g => g.Key, g =>
                {
                    var statusesInDoc = g.GroupBy(x => x.CardId).Select(x => x.First().CardStatusName).ToList();
                    return statusesInDoc.Count > 0 && statusesInDoc.All(s => s == StatusNames.Finalize);
                });
        }

        /// <summary>
        /// Builds the list of document audit summary DTOs from grouped audit rows and the finalized-per-document map.
        /// </summary>
        private static List<DocumentAuditorSummaryDto> BuildDocumentAuditorSummaryList(
            List<IGrouping<int, DocumentAuditorSummaryRowDto>> groupedByDocument,
            Dictionary<int, bool> isFinalizedByDocument)
        {
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

        /// <summary>
        /// Builds the list of workflow card audit DTOs from audit rows, limited by <paramref name="take"/> (or all if take &lt;= 0).
        /// </summary>
        private static List<WorkflowAuditorCardsDto> BuildWorkflowAuditorCards(IEnumerable<WorkflowAuditorDetailsRowDto> auditRows, int take)
        {
            var limit = take <= 0 ? int.MaxValue : take;
            return auditRows
                .Take(limit)
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
        }

        /// <summary>
        /// Builds the list of step counts (step id, name, and distinct document count per step) from workflow audit rows.
        /// </summary>
        private static List<WorkflowAuditorStepCountsDto> BuildWorkflowStepsCount(IEnumerable<WorkflowAuditorDetailsRowDto> auditRows)
        {
            return auditRows
                .GroupBy(a => new { a.StepId, a.StepName })
                .Select(g => new WorkflowAuditorStepCountsDto
                {
                    StepId = g.Key.StepId,
                    StepName = g.Key.StepName,
                    DocumentCount = g.Select(a => a.DocumentId).Distinct().Count()
                })
                .OrderBy(s => s.StepId)
                .ToList();
        }

        /// <summary>
        /// Builds the document status count DTO (total, finalized, rejected) from workflow audit rows using the latest card status per document.
        /// </summary>
        private static WorkflowAuditorDocumentStatusCountDto BuildWorkflowDocumentStatusCount(IEnumerable<WorkflowAuditorDetailsRowDto> auditRows)
        {
            var distinctDocumentIds = auditRows.Select(a => a.DocumentId).Distinct().ToList();
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

            return new WorkflowAuditorDocumentStatusCountDto
            {
                TotalDocuments = distinctDocumentIds.Count,
                Finalized = finalized,
                Rejected = rejected
            };
        }
    }
}
