using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository.Audit
{
    public class AuditorRepository : IAuditorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes the auditor repository with the application database context and user repository.
        /// </summary>
        public AuditorRepository(
            ApplicationDbContext context,
            IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Returns documents for the auditor driven by the top N documentIds (by most recent audit activity). Load-more pattern: take 10, then 20, 30… One row per document with DocumentId, DocumentName, Workflows (with step), ActionsCount (audit rows for that document), IsFinalized (all cards of document finalized).
        /// Optional filters: search (DocumentId/CardId when numeric, or DocumentName/CardName/WorkflowName contains), isFinalized (true = finalized only, false = non-finalized only).
        /// </summary>
        public async Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, bool? isFinalized = null)
        {
            const int defaultTake = 10;
            if (take <= 0) take = defaultTake;

            int? searchAsId = null;
            if (!string.IsNullOrWhiteSpace(search) && int.TryParse(search.Trim(), out var parsedId))
                searchAsId = parsedId;

            var auditQuery = _context.AuditCards.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                auditQuery = auditQuery.Where(a =>
                    (searchAsId != null && (a.DocumentId == searchAsId.Value || a.CardId == searchAsId.Value))
                    || (a.Document != null && a.Document.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Step != null && a.Card.Step.Workflow != null && a.Card.Step.Workflow.Name.Contains(searchTerm)));
            }

            if (isFinalized.HasValue)
            {
                if (isFinalized.Value)
                    auditQuery = auditQuery.Where(a => a.Card != null && a.Card.Status != null && a.Card.Status.Name == StatusNames.Finalize);
                else
                    auditQuery = auditQuery.Where(a => a.Card == null || a.Card.Status == null || a.Card.Status.Name != StatusNames.Finalize);
            }

            var documentIdList = await auditQuery
                .OrderByDescending(a => a.Created)
                .Select(a => a.DocumentId)
                .Take(take)
                .ToListAsync();
            var documentIds = documentIdList.Distinct().ToList();

            if (documentIds.Count == 0)
                return new List<CardAuditorSummaryDto>();

            var auditRowsQuery = _context.AuditCards.AsNoTracking()
                .Where(a => documentIds.Contains(a.DocumentId));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                auditRowsQuery = auditRowsQuery.Where(a =>
                    (searchAsId != null && (a.DocumentId == searchAsId.Value || a.CardId == searchAsId.Value))
                    || (a.Document != null && a.Document.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Step != null && a.Card.Step.Workflow != null && a.Card.Step.Workflow.Name.Contains(searchTerm)));
            }

            if (isFinalized.HasValue)
            {
                if (isFinalized.Value)
                    auditRowsQuery = auditRowsQuery.Where(a => a.Card != null && a.Card.Status != null && a.Card.Status.Name == StatusNames.Finalize);
                else
                    auditRowsQuery = auditRowsQuery.Where(a => a.Card == null || a.Card.Status == null || a.Card.Status.Name != StatusNames.Finalize);
            }

            var auditRows = await auditRowsQuery
                .Select(a => new
                {
                    a.DocumentId,
                    DocumentName = a.Document != null ? a.Document.Name : string.Empty,
                    a.WorkflowId,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    StepId = a.Card != null ? a.Card.StepId : 0,
                    StepName = a.Card != null && a.Card.Step != null ? a.Card.Step.Name : string.Empty,
                    a.CardId,
                    CardStatusName = a.Card != null && a.Card.Status != null ? a.Card.Status.Name : string.Empty
                })
                .ToListAsync();

            var isFinalizedByDocument = auditRows
                .GroupBy(a => a.DocumentId)
                .ToDictionary(g => g.Key, g =>
                {
                    var cardsInDoc = g.GroupBy(x => x.CardId).Select(x => x.First().CardStatusName).ToList();
                    return cardsInDoc.Count > 0 && cardsInDoc.All(s => s == StatusNames.Finalize);
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
                    .Select(x => new { x.WorkflowId, x.WorkflowName, x.StepId, x.StepName, x.CardId })
                    .GroupBy(x => new { x.WorkflowId, x.WorkflowName, x.StepId, x.StepName })
                    .Select(x => x.First())
                    .Select(x => new CardAuditorWorkflowsDto
                    {
                        Id = x.WorkflowId,
                        Name = x.WorkflowName,
                        StepId = x.StepId,
                        StepName = x.StepName,
                        CardId = x.CardId
                    })
                    .ToList();
                return new CardAuditorSummaryDto
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
        /// Returns document audit detail for a document and workflow: DocumentId, DocumentName, WorkflowId, WorkflowName, and DocumentHistory (up to N audit entries). Optional filters: search (UserName, DocumentName, CardName, ActionType, StepName), userId, actionType, stepId. Order by Created desc or asc. Returns null when no audit rows exist.
        /// </summary>
        public async Task<CardAuditorDetailDto?> FindCardAuditDetailsAsync(int documentId, int workflowId, int take, string? search = null, Guid? userId = null, int? actionType = null, int? stepId = null, bool orderDescending = true)
        {
            const int defaultTake = 10;
            if (take <= 0) take = defaultTake;

            var query = _context.AuditCards.AsNoTracking()
                .Where(a => a.DocumentId == documentId && a.WorkflowId == workflowId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(a =>
                    (a.User != null && a.User.Name != null && a.User.Name.Contains(searchTerm))
                    || (a.Document != null && a.Document.Name != null && a.Document.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Name != null && a.Card.Name.Contains(searchTerm))
                    || a.ActionType.ToString().Contains(searchTerm)
                    || (a.Card != null && a.Card.Step != null && a.Card.Step.Name != null && a.Card.Step.Name.Contains(searchTerm)));
            }

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);
            if (actionType.HasValue)
                query = query.Where(a => (int)a.ActionType == actionType.Value);
            if (stepId.HasValue)
                query = query.Where(a => a.Card != null && a.Card.StepId == stepId.Value);

            var ordered = orderDescending
                ? query.OrderByDescending(a => a.Created)
                : query.OrderBy(a => a.Created);

            var rows = await ordered
                .Take(take)
                .Select(a => new
                {
                    DocumentName = a.Document != null ? a.Document.Name : string.Empty,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    ActionName = a.ActionType.ToString(),
                    StepId = a.Card != null ? a.Card.StepId : 0,
                    StepName = a.Card != null && a.Card.Step != null ? a.Card.Step.Name : string.Empty,
                    a.Created
                })
                .ToListAsync();

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

            return new CardAuditorDetailDto
            {
                DocumentId = documentId,
                DocumentName = first.DocumentName,
                WorkflowId = workflowId,
                WorkflowName = first.WorkflowName,
                DocumentHistory = documentHistory
            };
        }

        /// <summary>
        /// Returns workflow-based audit entries (one row per workflow) with CardCount, LogsCount, Team, and Profile.
        /// Load-more pattern: take 10, then 20, 30, … (first N most recently audited workflows).
        /// Optional search: filters by WorkflowName or TeamName (contains).
        /// </summary>
        public async Task<ICollection<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync(int take = 10, string? search = null)
        {
            const int defaultTake = 10;
            if (take <= 0) take = defaultTake;

            var query = _context.AuditCards.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(a =>
                    (a.Workflow != null && a.Workflow.Name != null && a.Workflow.Name.Contains(searchTerm))
                    || (a.Workflow != null && a.Workflow.Teams.Any() && a.Workflow.Teams.Any(t => t.Name != null && t.Name.Contains(searchTerm))));
            }

            var workflowList = await query
                .OrderByDescending(a => a.Created)
                .Select(a => a.WorkflowId)
                .Take(take)
                .ToListAsync();

            if (workflowList.Count == 0)
                return new List<WorkflowAuditorSummaryDto>();

            var auditRows = await _context.AuditCards
                .AsNoTracking()
                .Where(a => workflowList.Contains(a.WorkflowId))
                .Select(a => new
                {
                    a.WorkflowId,
                    a.CardId,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    TeamId = a.Workflow != null && a.Workflow.Teams.Any()
                        ? (int?)a.Workflow.Teams.OrderBy(t => t.Id).Select(t => t.Id).FirstOrDefault()
                        : null,
                    TeamName = a.Workflow != null && a.Workflow.Teams.Any()
                        ? a.Workflow.Teams.OrderBy(t => t.Id).Select(t => t.Name).FirstOrDefault() ?? string.Empty
                        : string.Empty,
                    ProfileId = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? (int?)a.Card.Step.Profile.Id
                        : null,
                    ProfileName = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? a.Card.Step.Profile.Name ?? string.Empty
                        : string.Empty
                })
                .ToListAsync();

            var AuditorByWorkflow = auditRows
                .GroupBy(a => a.WorkflowId)
                .OrderBy(g => g.Key)
                .ToList();

            return AuditorByWorkflow.Select(g =>
            {
                var first = g.First();
                return new WorkflowAuditorSummaryDto
                {
                    WorkflowId = g.Key,
                    WorkflowName = first.WorkflowName,
                    CardCount = g.Select(a => a.CardId).Distinct().Count(),
                    LogsCount = g.Count(),
                    TeamId = first.TeamId,
                    TeamName = first.TeamName,
                    ProfileId = first.ProfileId,
                    ProfileName = first.ProfileName
                };
            }).ToList();
        }

        /// <summary>
        /// Returns audit data for a workflow: WorkflowId, WorkflowName, LogCount, StepsCount, CardStatusCount, Cards. Returns null when no audit entries exist for the workflow.
        /// Optional filters: search (UserName, CardName, StepName, ActionType contains), stepId, actionType (AuditCardActionType enum value).
        /// </summary>
        public async Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId, string? search = null, int? stepId = null, int? actionType = null)
        {
            var query = _context.AuditCards
                .AsNoTracking()
                .Where(a => a.WorkflowId == workflowId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(a =>
                    (a.User != null && a.User.Name != null && a.User.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Name != null && a.Card.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Step != null && a.Card.Step.Name != null && a.Card.Step.Name.Contains(searchTerm))
                    || a.ActionType.ToString().Contains(searchTerm));
            }

            if (stepId.HasValue)
                query = query.Where(a => a.Card != null && a.Card.StepId == stepId.Value);

            if (actionType.HasValue)
                query = query.Where(a => (int)a.ActionType == actionType.Value);

            var auditRows = await query
                .Select(a => new
                {
                    a.Id,
                    a.CardId,
                    a.WorkflowId,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    a.Created,
                    a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    a.ActionType,
                    CardName = a.Card != null ? a.Card.Name : string.Empty,
                    CardStatus = a.Card != null && a.Card.Status != null ? a.Card.Status.Name : string.Empty,
                    StepId = a.Card != null ? a.Card.StepId : 0,
                    StepName = a.Card != null && a.Card.Step != null ? a.Card.Step.Name : string.Empty
                })
                .ToListAsync();

            if (auditRows.Count == 0)
                return null;

            var first = auditRows.First();
            var distinctCardIds = auditRows.Select(a => a.CardId).Distinct().ToList();

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
                    CardCount = g.Select(a => a.CardId).Distinct().Count()
                })
                .OrderBy(s => s.StepId)
                .ToList();

            int finalized = 0;
            int rejected = 0;
            if (distinctCardIds.Count > 0)
            {
                var cardStatuses = await _context.Cards
                    .AsNoTracking()
                    .Where(c => distinctCardIds.Contains(c.Id))
                    .Select(c => new { c.Id, StatusName = c.Status != null ? c.Status.Name : string.Empty })
                    .ToListAsync();
                finalized = cardStatuses.Count(s => s.StatusName == StatusNames.Finalize);
                rejected = cardStatuses.Count(s => s.StatusName == StatusNames.Rejected);
            }

            var cardStatusCount = new WorkflowAuditorCardStatusCountDto
            {
                TotalCards = distinctCardIds.Count,
                Finalized = finalized,
                Rejected = rejected
            };

            return new WorkflowAuditorDetailsDto
            {
                WorkflowId = first.WorkflowId,
                WorkflowName = first.WorkflowName,
                LogCount = auditRows.Count,
                StepsCount = stepsCount,
                CardStatusCount = cardStatusCount,
                Cards = cards
            };
        }

        /// <summary>
        /// Returns user-based audit entries (one row per user) with UserId, UserName, Teams, Profiles, WorkflowCount, LogCount.
        /// Source: AuditCards grouped by UserId. Load-more pattern: take 10, then 20, 30, … (first N users).
        /// Optional filters: userName (contains on User.Name), teamId (user has at least one audit entry in a workflow with that team).
        /// </summary>
        public async Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int take = 10, string? userName = null, int? teamId = null)
        {
            const int defaultTake = 10;
            if (take <= 0) take = defaultTake;

            var query = _context.AuditCards.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(userName))
            {
                var nameTerm = userName.Trim();
                query = query.Where(a => a.User != null && a.User.Name.Contains(nameTerm));
            }

            if (teamId.HasValue)
                query = query.Where(a => a.Workflow != null && a.Workflow.Teams.Any(t => t.Id == teamId.Value));

            var userIds = await query
                .Select(a => a.UserId)
                .Distinct()
                .OrderBy(id => id)
                .Take(take)
                .ToListAsync();

            if (userIds.Count == 0)
                return new List<UserAuditorSummaryDto>();

            var auditRows = await _context.AuditCards
                .AsNoTracking()
                .Where(a => userIds.Contains(a.UserId))
                .Select(a => new
                {
                    a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    a.WorkflowId,
                    Teams = a.Workflow != null && a.Workflow.Teams != null
                        ? a.Workflow.Teams.Select(t => new UsersAuditorTeamsDto { TeamId = t.Id, TeamName = t.Name ?? string.Empty })
                        : null,
                    ProfileId = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? (int?)a.Card.Step.Profile.Id
                        : (int?)null,
                    ProfileName = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? a.Card.Step.Profile.Name ?? string.Empty
                        : string.Empty
                })
                .ToListAsync();

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

        /// <summary>
        /// Returns full audit details for a single user by userId: UserId, UserName, Teams, Profiles, LogCountTotal, LogCountByActionType, and Actions list.
        /// Returns null when the user has no audit entries. Optional: search (CardName, WorkflowName, ActionType contains), action type code; order by Created desc or asc.
        /// </summary>
        public async Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, string? search = null, int? actionTypeCode = null, bool orderDescending = true)
        {
            var query = _context.AuditCards
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(a =>
                    (a.Card != null && a.Card.Name != null && a.Card.Name.Contains(searchTerm))
                    || (a.Workflow != null && a.Workflow.Name != null && a.Workflow.Name.Contains(searchTerm))
                    || a.ActionType.ToString().Contains(searchTerm));
            }

            if (actionTypeCode.HasValue)
                query = query.Where(a => (int)a.ActionType == actionTypeCode.Value);

            var projected = query
                .Select(a => new
                {
                    a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    a.WorkflowId,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    Teams = a.Workflow != null && a.Workflow.Teams != null
                        ? a.Workflow.Teams.Select(t => new UsersAuditorTeamsDto { TeamId = t.Id, TeamName = t.Name ?? string.Empty })
                        : null,
                    ProfileId = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? (int?)a.Card.Step.Profile.Id
                        : (int?)null,
                    ProfileName = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? a.Card.Step.Profile.Name ?? string.Empty
                        : string.Empty,
                    a.ActionType,
                    a.CardId,
                    CardName = a.Card != null ? a.Card.Name : string.Empty,
                    a.Created
                });

            var auditRows = await (orderDescending
                ? projected.OrderByDescending(a => a.Created)
                : projected.OrderBy(a => a.Created))
                .ToListAsync();

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
    }
}
