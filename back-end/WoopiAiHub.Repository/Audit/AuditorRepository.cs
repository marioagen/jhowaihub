using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Documents;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Users;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Workflows;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Models.Audit;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository.Audit
{
    /// <summary>
    /// Repository for audit data. Reads from AuditCards and related entities to support document, workflow, and user audit summaries and details.
    /// </summary>
    public class AuditorRepository : IAuditorRepository
    {
        private const int DefaultTake = 10;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes the auditor repository with the application database context.
        /// </summary>
        public AuditorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> distinct document IDs ordered by most recent audit activity, skipping <paramref name="skip"/>. Optional search (document name, workflow name, or numeric document id) and isFinalized filter.
        /// </summary>
        public async Task<List<int>> FindDocumentIdsForDocumentsSummaryAsync(int take, int skip, string? search, bool? isFinalized = null)
        {
            if (take <= 0) take = DefaultTake;
            if (skip < 0) skip = 0;

            int? searchAsId = null;
            if (!string.IsNullOrWhiteSpace(search) && int.TryParse(search.Trim(), out var parsedId))
                searchAsId = parsedId;

            var auditQuery = ApplyDocumentsSummaryFilters(_context.AuditCards.AsNoTracking(), search, searchAsId, isFinalized);

            return await auditQuery
                .GroupBy(a => a.DocumentId)
                .Select(g => new { DocumentId = g.Key, MaxCreated = g.Max(a => a.Created) })
                .OrderByDescending(x => x.MaxCreated)
                .Skip(skip)
                .Take(take)
                .Select(x => x.DocumentId)
                .ToListAsync();
        }

        /// <summary>
        /// Returns audit rows for the given document IDs, applying the same search and isFinalized filter as the documents summary. Projects to DocumentAuditorSummaryRowDto (document, workflow, step, card, status).
        /// </summary>
        public async Task<List<DocumentAuditorSummaryRowDto>> FindAuditRowsForDocumentsSummaryAsync(IReadOnlyList<int> documentIds, string? search, bool? isFinalized = null)
        {
            if (documentIds.Count == 0)
                return new List<DocumentAuditorSummaryRowDto>();

            int? searchAsId = null;
            if (!string.IsNullOrWhiteSpace(search) && int.TryParse(search.Trim(), out var parsedId))
                searchAsId = parsedId;

            var auditRowsQuery = _context.AuditCards.AsNoTracking()
                .Where(a => documentIds.Contains(a.DocumentId));
            auditRowsQuery = ApplyDocumentsSummaryFilters(auditRowsQuery, search, searchAsId, isFinalized);

            return await auditRowsQuery
                .Select(a => new DocumentAuditorSummaryRowDto
                {
                    DocumentId = a.DocumentId,
                    DocumentName = a.Document != null ? a.Document.Name : string.Empty,
                    WorkflowId = a.WorkflowId,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    StepId = a.Card != null ? a.Card.StepId : 0,
                    StepName = a.Card != null && a.Card.Step != null ? a.Card.Step.Name : string.Empty,
                    CardId = a.CardId,
                    CardStatusName = a.Card != null && a.Card.Status != null ? a.Card.Status.Name : string.Empty
                })
                .ToListAsync();
        }

        /// <summary>
        /// Applies search (document/workflow name or numeric id) and isFinalized filters to an AuditCards query.
        /// </summary>
        private static IQueryable<AuditCard> ApplyDocumentsSummaryFilters(
            IQueryable<AuditCard> query,
            string? search,
            int? searchAsId,
            bool? isFinalized)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(a =>
                    (searchAsId != null && (a.DocumentId == searchAsId.Value || a.CardId == searchAsId.Value))
                    || (a.Document != null && a.Document.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Name.Contains(searchTerm))
                    || (a.Card != null && a.Card.Step != null && a.Card.Step.Workflow != null && a.Card.Step.Workflow.Name.Contains(searchTerm)));
            }

            if (isFinalized.HasValue)
            {
                if (isFinalized.Value)
                    query = query.Where(a => a.Card != null && a.Card.Status != null && a.Card.Status.Name == StatusNames.Finalize);
                else
                    query = query.Where(a => a.Card == null || a.Card.Status == null || a.Card.Status.Name != StatusNames.Finalize);
            }

            return query;
        }

        /// <summary>
        /// Applies search (user/document/card/action/step name), userId, actionType, and stepId filters to a document-detail AuditCards query.
        /// </summary>
        private static IQueryable<AuditCard> ApplyDocumentDetailFilters(
            IQueryable<AuditCard> query,
            string? search,
            Guid? userId,
            int? actionType,
            int? stepId)
        {
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

            return query;
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> audit rows for the given document and workflow. Optional filters: search (user/document/action/step name), userId, actionType, stepId. Ordered by Created (asc or desc).
        /// </summary>
        public async Task<List<DocumentAuditorDetailRowDto>> FindAuditRowsForDocumentDetailAsync(int documentId, int workflowId, int take, string? search, Guid? userId, int? actionType, int? stepId, bool orderDescending)
        {
            if (take <= 0) take = DefaultTake;

            var query = _context.AuditCards.AsNoTracking()
                .Where(a => a.DocumentId == documentId && a.WorkflowId == workflowId);
            query = ApplyDocumentDetailFilters(query, search, userId, actionType, stepId);

            var ordered = orderDescending
                ? query.OrderByDescending(a => a.Created)
                : query.OrderBy(a => a.Created);

            return await ordered
                .Take(take)
                .Select(a => new DocumentAuditorDetailRowDto
                {
                    DocumentName = a.Document != null ? a.Document.Name : string.Empty,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    ActionName = a.ActionType.ToString(),
                    StepId = a.Card != null ? a.Card.StepId : 0,
                    StepName = a.Card != null && a.Card.Step != null ? a.Card.Step.Name : string.Empty,
                    Created = a.Created
                })
                .ToListAsync();
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> workflow IDs ordered by most recent audit activity, skipping <paramref name="skip"/>. Optional search by workflow name or team name.
        /// </summary>
        public async Task<List<int>> FindWorkflowIdsForWorkflowSummaryAsync(int take, int skip, string? search)
        {
            if (take <= 0) take = DefaultTake;
            if (skip < 0) skip = 0;

            var query = _context.AuditCards.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(a =>
                    (a.Workflow != null && a.Workflow.Name != null && a.Workflow.Name.Contains(searchTerm))
                    || (a.Workflow != null && a.Workflow.Teams.Any() && a.Workflow.Teams.Any(t => t.Name != null && t.Name.Contains(searchTerm))));
            }

            return await query
                .GroupBy(a => a.WorkflowId)
                .Select(g => new { WorkflowId = g.Key, MaxCreated = g.Max(a => a.Created) })
                .OrderByDescending(x => x.MaxCreated)
                .Skip(skip)
                .Take(take)
                .Select(x => x.WorkflowId)
                .ToListAsync();
        }

        /// <summary>
        /// Returns all audit rows for the given workflow IDs, projected to WorkflowAuditorSummaryRowDto (workflow, document, team, profile).
        /// </summary>
        public async Task<List<WorkflowAuditorSummaryRowDto>> FindAuditRowsForWorkflowSummaryAsync(IReadOnlyList<int> workflowIds)
        {
            if (workflowIds.Count == 0)
                return new List<WorkflowAuditorSummaryRowDto>();

            return await _context.AuditCards
                .AsNoTracking()
                .Where(a => workflowIds.Contains(a.WorkflowId))
                .Select(a => new WorkflowAuditorSummaryRowDto
                {
                    WorkflowId = a.WorkflowId,
                    DocumentId = a.DocumentId,
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
        }

        /// <summary>
        /// Applies search (user/card/step/action name), stepId, and actionType filters to a workflow-details AuditCards query.
        /// </summary>
        private static IQueryable<AuditCard> ApplyWorkflowDetailsFilters(
            IQueryable<AuditCard> query,
            string? search,
            int? stepId,
            int? actionType)
        {
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

            return query;
        }

        /// <summary>
        /// Returns all audit rows for the given workflow. Optional filters: search (user/card/step/action name), stepId, actionType. Ordered by Created (asc or desc). Projects to WorkflowAuditorDetailsRowDto.
        /// </summary>
        public async Task<List<WorkflowAuditorDetailsRowDto>> FindAuditRowsForWorkflowDetailsAsync(int workflowId, string? search, int? stepId, int? actionType, bool orderDescending)
        {
            var query = _context.AuditCards
                .AsNoTracking()
                .Where(a => a.WorkflowId == workflowId);
            query = ApplyWorkflowDetailsFilters(query, search, stepId, actionType);

            var ordered = orderDescending
                ? query.OrderByDescending(a => a.Created)
                : query.OrderBy(a => a.Created);

            return await ordered
                .Select(a => new WorkflowAuditorDetailsRowDto
                {
                    Id = a.Id,
                    CardId = a.CardId,
                    DocumentId = a.DocumentId,
                    WorkflowId = a.WorkflowId,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    Created = a.Created,
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    ActionType = a.ActionType,
                    CardName = a.Card != null ? a.Card.Name : string.Empty,
                    CardStatus = a.Card != null && a.Card.Status != null ? a.Card.Status.Name : string.Empty,
                    StepId = a.Card != null ? a.Card.StepId : 0,
                    StepName = a.Card != null && a.Card.Step != null ? a.Card.Step.Name : string.Empty
                })
                .ToListAsync();
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> distinct user IDs (ordered by id) that have audit entries, skipping <paramref name="skip"/>. Optional filters: userName (contains), teamId.
        /// </summary>
        public async Task<List<Guid>> FindUserIdsForUserSummaryAsync(int take, int skip, string? userName, int? teamId)
        {
            if (take <= 0) take = DefaultTake;
            if (skip < 0) skip = 0;

            var query = _context.AuditCards.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(userName))
            {
                var nameTerm = userName.Trim();
                query = query.Where(a => a.User != null && a.User.Name != null && a.User.Name.Contains(nameTerm));
            }

            if (teamId.HasValue)
                query = query.Where(a => a.Workflow != null && a.Workflow.Teams.Any(t => t.Id == teamId.Value));

            return await query
                .Select(a => a.UserId)
                .Distinct()
                .OrderBy(id => id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Returns all audit rows for the given user IDs, projected to UserAuditorSummaryRowDto (user, workflow, teams, profile).
        /// </summary>
        public async Task<List<UserAuditorSummaryRowDto>> FindAuditRowsForUserSummaryAsync(IReadOnlyList<Guid> userIds)
        {
            if (userIds.Count == 0)
                return new List<UserAuditorSummaryRowDto>();

            return await _context.AuditCards
                .AsNoTracking()
                .Where(a => userIds.Contains(a.UserId))
                .Select(a => new UserAuditorSummaryRowDto
                {
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    WorkflowId = a.WorkflowId,
                    Teams = a.Workflow != null && a.Workflow.Teams != null
                        ? a.Workflow.Teams.Select(t => new UsersAuditorTeamsDto { TeamId = t.Id, TeamName = t.Name ?? string.Empty })
                        : null,
                    ProfileId = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? (int?)a.Card.Step.Profile.Id
                        : null,
                    ProfileName = a.Card != null && a.Card.Step != null && a.Card.Step.Profile != null
                        ? a.Card.Step.Profile.Name ?? string.Empty
                        : string.Empty
                })
                .ToListAsync();
        }

        /// <summary>
        /// Applies search (card/workflow/action name) and actionTypeCode filters to a user-details AuditCards query.
        /// </summary>
        private static IQueryable<AuditCard> ApplyUserDetailsFilters(
            IQueryable<AuditCard> query,
            string? search,
            int? actionTypeCode)
        {
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

            return query;
        }

        /// <summary>
        /// Returns up to <paramref name="take"/> audit rows for the given user. Optional filters: search (card/workflow/action name), actionTypeCode. Ordered by Created (asc or desc). Projects to UserAuditorDetailsRowDto.
        /// </summary>
        public async Task<List<UserAuditorDetailsRowDto>> FindAuditRowsForUserDetailsAsync(Guid userId, int take, string? search, int? actionTypeCode, bool orderDescending)
        {
            if (take <= 0) take = DefaultTake;

            var query = _context.AuditCards
                .AsNoTracking()
                .Where(a => a.UserId == userId);
            query = ApplyUserDetailsFilters(query, search, actionTypeCode);

            var projected = query
                .Select(a => new UserAuditorDetailsRowDto
                {
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Name : string.Empty,
                    WorkflowId = a.WorkflowId ?? null,
                    WorkflowName = a.Workflow != null ? a.Workflow.Name : string.Empty,
                    Teams = a.Workflow.Teams.Select(t => new UsersAuditorTeamsDto { TeamId = t.Id, TeamName = t.Name ?? string.Empty }) ?? [],
                    ProfileId = a.Card.Step.Profile.Id ?? null,
                    ProfileName = a.Card.Step.Profile.Name ?? string.Empty,
                    ActionType = a.ActionType,
                    CardId = a.CardId,
                    CardName = a.Card != null ? a.Card.Name : string.Empty,
                    Created = a.Created
                });

            return await (orderDescending
                ? projected.OrderByDescending(a => a.Created)
                : projected.OrderBy(a => a.Created))
                .Take(take)
                .ToListAsync();
        }
    }
}
