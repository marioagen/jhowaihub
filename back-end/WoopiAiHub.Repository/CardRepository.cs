using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
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

        public CardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// checks if a collection of card IDs exists in the database.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> ExistsStepsInUse(ICollection<int> ids)
        {
            return await _context.Cards.Where(a => ids.Contains(a.StepId)).AnyAsync();
        }

        /// <summary>
        /// Returns a card by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindById(int id)
        {
            return await _context.Cards.Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns a card by its ID with status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindByIdWithStatus(int id)
        {
            return await _context.Cards
                .Include(s => s.Status)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
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
        /// Returns a card by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindByIdWithDocument(int id)
        {
            return await _context.Cards.Where(c => c.Id == id)
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
            return await _context.Cards.Where(c => c.Id == id)
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
            return await _context.Cards.Where(c => c.Id == id)
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
        /// Updates the specified collection of card entities in the database.
        /// </summary>
        /// <remarks>Throws an exception if any card in the list is invalid or if a database error occurs.
        /// All changes are committed in a single transaction.</remarks>
        /// <param name="cards">A list of <see cref="Card"/> objects to update. Each card must have a valid identifier corresponding to an
        /// existing record in the database. Cannot be null.</param>
        /// <returns>true if one or more records were updated successfully; otherwise, false.</returns>
        public bool UpdateList(List<Card> cards)
        {
            _context.Cards.UpdateRange(cards);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// Logically deletes cards by document ids by setting Enable to false (soft delete).
        /// Cards are excluded from default queries via the global query filter.
        /// </summary>
        /// <param name="documentIds">Ids of documents whose cards should be logically deleted.</param>
        /// <returns>True if any card was updated; otherwise false.</returns>
        public async Task<bool> DeleteByDocumentIds(List<int> documentIds)
        {
            var cards = await _context.Cards.Where(c => documentIds.Contains(c.DocumentId)).ToListAsync();
            if (cards.Count > 0)
            {
                foreach (var card in cards)
                    card.Disable();
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
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
                    CurrentStepOrder = c.Step != null ? c.Step.Order : 0
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
                .Include(c => c.Document)
                .Where(c => c.DocumentBatchId == documentBatchId)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
    }
}
