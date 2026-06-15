using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class CardRepository : ICardRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardRepository"/> class with the EF Core database context.
        /// </summary>
        public CardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Counts how many cards are using the provided step ids.
        /// </summary>
        /// <param name="ids">Collection of step ids to check.</param>
        /// <returns>Number of cards that reference any of the given steps.</returns>
        public async Task<int> CountByStepsInUse(ICollection<int> ids)
        {
            return await _context.Cards.CountAsync(a => ids.Contains(a.StepId) && a.Enable);
        }

        /// <summary>
        /// Counts every card linked to the provided step ids regardless of the Enable flag.
        /// Used by the wizard's blocker logic so that historical (soft-deleted) cards still
        /// prevent destructive edits like step removal — preserving audit traceability.
        /// </summary>
        /// <param name="ids">Collection of step ids to check.</param>
        /// <returns>Number of cards (active or disabled) that reference any of the given steps.</returns>
        public async Task<int> CountAllByStepIdsAsync(ICollection<int> ids)
        {
            return await _context.Cards
                .IgnoreQueryFilters()
                .CountAsync(a => ids.Contains(a.StepId));
        }

        /// <summary>
        /// Returns a card by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindById(int id)
        {
            return await _context.Cards
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Returns a card by its ID with status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindByIdWithStatus(int id)
        {
            return await _context.Cards
                .AsNoTracking()
                .Include(s => s.Status)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Returns a card by its ID with status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindByIdWithStepWorkflow(int id)
        {
            return await _context.Cards
                .Include(s => s.Step)
                    .ThenInclude(st => st!.Workflow)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Loads multiple cards by id with step and workflow navigation properties, tracked by the current context.
        /// </summary>
        /// <param name="cardIds">Card identifiers to load.</param>
        public async Task<List<Card>> FindRangeByIdsWithStepWorkflowTracked(IReadOnlyList<int> cardIds)
        {
            return await _context.Cards
                .Where(c => cardIds.Contains(c.Id))
                .Include(s => s.Step)
                    .ThenInclude(st => st!.Workflow)
                .ToListAsync();
        }

        /// <summary>
        /// Loads cards by id with step and workflow navigation properties tracked; returns an empty list when <paramref name="cardIds"/> is null or empty.
        /// </summary>
        /// <inheritdoc />
        public async Task<List<Card>?> FindByCardIdsAsync(IReadOnlyList<int> cardIds)
        {
            if (cardIds == null || cardIds.Count == 0)
                return new List<Card>();
            return await FindRangeByIdsWithStepWorkflowTracked(cardIds);
        }

        /// <summary>
        /// Returns a card by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindByIdWithDocument(int id)
        {
            return await _context.Cards
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Include(d => d.Document)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns a card dto by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CardAnalysisDto?> FindByIdWithDocumentAndWorkflow(int id)
        {
            return await _context.Cards
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CardAnalysisDto
                {
                    Id = c.Id,
                    Created = c.Created,
                    StepId = c.StepId,
                    DocumentId = c.DocumentId,
                    AssignedUserId = c.AssignedUserId,
                    Name = c.Name,
                    StatusId = c.StatusId,
                    Document = c.Document != null ? new DocumentDto
                    {
                        Id = c.Document.Id,
                        Name = c.Document.Name,
                        Description = c.Document.Description,
                        ReferenceFile = c.Document.ReferenceFile
                    } : null,
                    Step = c.Step != null ? new StepDto
                    {
                        Id = c.Step.Id,
                        Name = c.Step.Name,
                        Order = c.Step.Order,
                        WorkflowId = c.Step.WorkflowId,
                    } : null,
                    Outputs = c.Outputs != null ? c.Outputs.Select(o => new StepToolOutputAnalysesDto
                    {
                        Id = o.Id,
                        StepToolId = o.StepToolId,
                        Value = o.Value,
                        StepTool = o.StepTool != null ? new StepToolDto
                        {
                            Id = o.StepTool.Id,
                            StepId = o.StepTool.StepId,
                            ToolId = o.StepTool.ToolId,
                            Parameters = o.StepTool.Parameters.Select(p => new StepToolParameterDto
                            {
                                Id = p.Id,
                                Value = p.Value,
                                RequiredFile = p.RequiredFile,
                                WebhookId = p.WebhookId,
                                Type = string.Empty
                            }).ToList(),
                            Tool = o.StepTool.Tool != null ? new ToolDto
                            {
                                Id = o.StepTool.Tool.Id,
                                Name = o.StepTool.Tool.Name,
                                ToolTypeId = o.StepTool.Tool.ToolTypeId,
                                ToolType = o.StepTool.Tool.ToolType != null ?
                                    o.StepTool.Tool.ToolType.Name
                                    : string.Empty,
                            } : null
                        } : null
                    }).ToList() : null
                }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns a card by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindByIdWithStepAndProfile(int id)
        {
            return await _context.Cards
                .Where(c => c.Id == id)
                .Include(s => s.Step)
                    .ThenInclude(p => p!.Profile)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates a card.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool Update(Card card)
        {
            _context.Cards.Update(card);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// Updates the specified collection of card entities; equivalent to <see cref="UpdateRange"/>.
        /// </summary>
        public Task<bool> UpdateList(List<Card> cards) => UpdateRange(cards);

        /// <summary>
        /// Updates StepId, StatusId and AssignedUserId for the given cards directly in the database,
        /// bypassing the EF ChangeTracker entirely.
        /// </summary>
        /// <remarks>
        /// Using ExecuteUpdateAsync avoids the InvalidOperationException thrown when a navigation property
        /// for a required relationship (e.g. Card.Step) is null on a tracked entity.
        /// Cards with the same scalar values are batched into a single UPDATE statement.
        /// </remarks>
        public async Task<bool> UpdateRange(List<Card> cards)
        {
            if (cards == null || cards.Count == 0)
                return false;

            var totalUpdated = 0;
            var groups = cards.GroupBy(c => new { c.StepId, c.StatusId, c.AssignedUserId });
            foreach (var group in groups)
            {
                var ids = group.Select(c => c.Id).ToList();
                var stepId = group.Key.StepId;
                var statusId = group.Key.StatusId;
                var assignedUserId = group.Key.AssignedUserId;

                totalUpdated += await _context.Cards
                    .Where(c => ids.Contains(c.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.StepId, stepId)
                        .SetProperty(c => c.StatusId, statusId)
                        .SetProperty(c => c.AssignedUserId, assignedUserId));
            }

            return totalUpdated > 0;
        }

        /// <summary>
        /// Logically disables the specified collection of card entities by their ids.
        /// </summary>
        /// <param name="cardIds">A list of card ids to disable. Cannot be null or empty.</param>
        /// <returns>True if one or more records were updated successfully; otherwise, false.</returns>
        public async Task<bool> DisableByIds(List<int> cardIds)
        {
            var disabledCardsList = await _context.Cards
                .Where(c => cardIds.Contains(c.Id))
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(card => card.Enable, card => false));

            return disabledCardsList > 0;
        }

        /// <summary>
        /// Logically deletes cards by document ids by setting Enable to false (soft delete).
        /// Cards are excluded from default queries via the global query filter.
        /// </summary>
        /// <param name="documentIds">Ids of documents whose cards should be logically deleted.</param>
        /// <returns>True if any card was updated; otherwise false.</returns>
        public async Task<bool> DeleteByDocumentIds(List<int> documentIds)
        {
            if (documentIds == null || documentIds.Count == 0)
                return false;

            var disabledCardsList = await _context.Cards
                .Where(c => documentIds.Contains(c.DocumentId))
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(card => card.Enable, card => false));

            return disabledCardsList > 0;
        }

        /// <summary>
        /// Asynchronously retrieves the IDs of cards that are active in the first step.
        /// </summary>
        /// <remarks>A card is considered active in the first step if its associated step has an order
        /// value of 1.</remarks>
        /// <param name="cardIds">A collection of card IDs to filter. Only the IDs present in this collection and associated with cards in the
        /// first step will be returned.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of integers
        /// representing the IDs of the cards that are active in the first step.</returns>
        public async Task<ICollection<int>> FindActiveCardIdsInFirstStepAsync(IEnumerable<int> cardIds)
        {
            return await _context.Cards
                .Where(c => cardIds.Contains(c.Id) && c.Step!.Order == 1)
                .Select(c => c.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a card by its document ID, including related executions, step tools, and tool type information.
        /// </summary>
        /// <param name="documentId">The document ID to search for</param>
        /// <returns>A card with its related data, or null if not found</returns>
        public async Task<Card?> FindByDocumentIdCardAsync(int documentId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Where(c => c.DocumentId == documentId)
                .Include(c => c.Executions)
                    .ThenInclude(e => e.StepTool)
                        .ThenInclude(st => st!.Tool)
                .           ThenInclude(t => t!.ToolType)
                .OrderByDescending(c => c.Created)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds all cards associated with a specific document ID, with Step and Workflow included.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of cards with Step and Workflow loaded.</returns>
        public async Task<List<Card>> FindByDocumentIdCardListWithStepWorkflowAsync(int documentId)
        {
            return await _context.Cards
                .Where(c => c.DocumentId == documentId)
                .Include(c => c.Step)
                    .ThenInclude(s => s!.Workflow)
                .OrderBy(c => c.Step!.Order)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Finds all cards associated with a specific document ID.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of cards with their related Step information.</returns>
        public async Task<List<Card>> FindByDocumentIdCardListAsync(int documentId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Where(c => c.DocumentId == documentId)
                .Include(c => c.Step)
                .Include(c => c.Outputs)
                    .ThenInclude(o => o.StepTool)
                        .ThenInclude(st => st!.Tool)
                .OrderBy(c => c.Step!.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Finds card header info (CardName and WorkflowName) by cardId.
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<CardHeaderDto?> FindHeaderInfoAsync(int cardId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Where(c => c.Id == cardId)
                .Select(c => new CardHeaderDto
                {
                    CardName = c.Name,
                    WorkflowName = c.Step != null && c.Step.Workflow != null ? c.Step.Workflow.Name : string.Empty,
                    WorkflowId = c.Step != null && c.Step.Workflow != null ? c.Step.Workflow.Id : 0,
                    StatusName = c.Status != null ? c.Status.Name : string.Empty,
                    CurrentStepOrder = c.Step != null ? c.Step.Order : 0,
                    DocumentBatchId = c.DocumentBatchId
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves the IDs of cards associated with the specified document IDs.
        /// </summary>
        /// <param name="documentIds">The collection of document IDs to search for.</param>
        /// <returns>A collection of card IDs associated with the specified documents.</returns>
        public async Task<ICollection<int>> FindCardIdsByDocumentIdsAsync(IEnumerable<int> documentIds)
        {
            return await _context.Cards
                .Where(c => documentIds.Contains(c.DocumentId))
                .Select(c => c.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a collection of cards associated with the specified document batch identifier.
        /// </summary>
        /// <remarks>The returned collection is ordered by card identifier. Ensure that the
        /// documentBatchId provided is valid to avoid unexpected results.</remarks>
        /// <param name="documentBatchId">The unique identifier of the document batch for which to retrieve cards. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of Card objects
        /// linked to the specified document batch. The collection will be empty if no matching cards are found.</returns>
        public async Task<List<Card>> FindByDocumentBatchId(int documentBatchId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Include(c => c.Document)
                .Include(c => c.Step)
                .Where(c => c.DocumentBatchId == documentBatchId)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves cards in the given batch that belong to the specified workflow,
        /// allowing the analyze page to list only documents from the same esteira.
        /// </summary>
        /// <param name="documentBatchId">The batch identifier shared by all uploaded documents.</param>
        /// <param name="workflowId">The workflow whose cards should be returned.</param>
        /// <returns>Cards in the batch whose step belongs to <paramref name="workflowId"/>, ordered by card id.</returns>
        public async Task<List<Card>> FindByDocumentBatchIdAndWorkflow(int documentBatchId, int workflowId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Include(c => c.Document)
                .Include(c => c.Step)
                .Where(c => c.DocumentBatchId == documentBatchId && c.Step != null && c.Step.WorkflowId == workflowId)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Returns the card with step and workflow, or all cards in the same document batch when the card belongs to a batch.
        /// </summary>
        /// <inheritdoc />
        public async Task<List<Card>?> FindCardOrBatchWithStepWorkflowAsync(int cardId)
        {
            var card = await FindByIdWithStepWorkflow(cardId);
            if (card == null)
                return null;
            if (card.DocumentBatchId.HasValue)
                return await FindByDocumentBatchId(card.DocumentBatchId.Value);
            return [card];
        }

        /// <summary>
        /// Returns the card with document loaded, or all cards in the same document batch when the card belongs to a batch.
        /// </summary>
        /// <inheritdoc />
        public async Task<List<Card>?> FindCardOrBatchWithDocumentAsync(int cardId)
        {
            var card = await FindByIdWithDocument(cardId);
            if (card == null)
                return null;
            if (card.DocumentBatchId.HasValue)
                return await FindByDocumentBatchId(card.DocumentBatchId.Value);
            return [card];
        }

        /// <summary>
        /// Retrieves a card by its identifier, including its associated executions.
        /// </summary>
        /// <remarks>The returned card includes its related executions loaded from the database. This
        /// method performs a single query and may return null if no card with the specified ID exists.</remarks>
        /// <param name="cardId">The unique identifier of the card to retrieve. Must be a valid card ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the card with its executions if
        /// found; otherwise, null.</returns>
        public async Task<Card?> FindByIdWithExecutions(int cardId)
        {
            return await _context.Cards
                .Include(c => c.Executions)
                .FirstOrDefaultAsync(c => c.Id == cardId);
        }

        /// <summary>
        /// Retrieves a card by its identifier, including its associated document and step entities.
        /// </summary>
        /// <remarks>The returned card will have its Document and Step navigation properties populated.
        /// This method performs a database query and may return null if no card with the specified ID exists.</remarks>
        /// <param name="cardId">The unique identifier of the card to retrieve. Must be a valid card ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the card with its document and
        /// step included if found; otherwise, null.</returns>
        public async Task<Card?> FindByIdWithDocumentAndStep(int cardId)
        {
            return await _context.Cards
                .Include(c => c.Document)
                .Include(c => c.Step)
                .FirstOrDefaultAsync(c => c.Id == cardId);
        }

        /// <summary>
        /// Returns (cardId, documentId) pairs for active cards belonging to any of the specified steps.
        /// Used to collect audit data and orphan-document candidates before cards are disabled.
        /// The global query filter (Enable = true) ensures only active cards are returned.
        /// </summary>
        public async Task<List<(int cardId, int documentId)>> FindCardDocumentPairsByStepIdsAsync(List<int> stepIds)
        {
            if (stepIds == null || stepIds.Count == 0)
                return [];

            var rows = await _context.Cards
                .Where(c => stepIds.Contains(c.StepId))
                .Select(c => new { c.Id, c.DocumentId })
                .ToListAsync();

            return rows.Select(r => (r.Id, r.DocumentId)).ToList();
        }
    }
}
