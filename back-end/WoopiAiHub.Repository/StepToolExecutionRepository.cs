using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class StepToolExecutionRepository : IStepToolExecutionRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public StepToolExecutionRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a collection of <see cref="StepToolExecution"/> entities to the database asynchronously.
        /// </summary>
        /// <remarks>This method saves all provided entities to the database in a single operation. 
        /// Ensure that the provided list contains valid entities to avoid validation errors.</remarks>
        /// <param name="stepToolExecutions">A list of <see cref="StepToolExecution"/> entities to be added to the database.  The list cannot be null or
        /// empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/>  if the
        /// operation completes successfully.</returns>
        public async Task<bool> CreateRangeAsync(List<StepToolExecution> stepToolExecutions)
        {
            await _context.StepToolExecutions.AddRangeAsync(stepToolExecutions);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Asynchronously retrieves the first running OCR step tool execution associated with the specified card ID.
        /// </summary>
        /// <remarks>This method searches for a step tool execution where the status is <see
        /// cref="Domain.Enum.StatusExecution.Running"/> and the tool type is "Ocr". If no matching execution is found,
        /// the method returns <see langword="null"/>.</remarks>
        /// <param name="cardId">The unique identifier of the card to search for.</param>
        /// <returns>A <see cref="StepToolExecution"/> object representing the running OCR step tool execution if found;
        /// otherwise, <see langword="null"/>.</returns>
        public async Task<StepToolExecution?> FindRunningOcrByCardIdAsync(int cardId)
        {
            return await _context.StepToolExecutions
                                 .FirstOrDefaultAsync(s => s.CardId == cardId &&
                                                           s.Status == Domain.Enum.StatusExecution.Running &&
                                                           s.StepTool!.Tool!.ToolType!.Equals("Ocr"));
        }

       /// <summary>
       /// Asynchronously retrieves a <see cref="StepToolExecution"/> entity based on the specified step tool ID and
       /// card ID.
       /// </summary>
       /// <remarks>This method queries the database for a single <see cref="StepToolExecution"/> entity
       /// where the  <c>StepToolId</c> matches <paramref name="stepToolId"/> and the <c>CardId</c> matches <paramref
       /// name="cardId"/>.</remarks>
       /// <param name="stepToolId">The unique identifier of the step tool.</param>
       /// <param name="cardId">The unique identifier of the card.</param>
       /// <returns>A task that represents the asynchronous operation. The task result contains the  <see
       /// cref="StepToolExecution"/> entity that matches the specified step tool ID and card ID,  or <see
       /// langword="null"/> if no matching entity is found.</returns>
        public async Task<StepToolExecution?> FindByStepToolIdAndCardIdAsync(int stepToolId, int cardId)
        {
            return await _context.StepToolExecutions
                                 .Include(s => s.StepTool)
                                 .Include(c => c.Card)
                                 .FirstOrDefaultAsync(s => s.StepToolId.Equals(stepToolId) &&
                                                           s.CardId.Equals(cardId));
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="StepToolExecution"/> entity based on the specified card ID.
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<ICollection<StepToolExecution>> FindByStepToolByCardIdAsync(int cardId)
        {
            return await _context.StepToolExecutions
                        .Include(s => s.StepTool)
                            .ThenInclude(s => s!.Tool)
                            .ThenInclude(t => t!.ToolType)
                        .Where(s => s.CardId == cardId)
                        .ToListAsync();
        }

        /// <summary>
        /// Updates the specified <see cref="StepToolExecution"/> entity in the database.
        /// </summary>
        /// <remarks>This method updates the state of the provided entity in the database context and
        /// persists the changes. Ensure that the entity being updated is tracked by the context before calling this
        /// method.</remarks>
        /// <param name="stepToolExecution">The <see cref="StepToolExecution"/> entity to update. This entity must already exist in the database.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(StepToolExecution stepToolExecution)
        {
            _context.StepToolExecutions.Update(stepToolExecution);
            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Cont step executions 
        /// </summary>
        /// <param name="stepId"></param>
        /// <returns></returns>
        public async Task<int> ExecutionsByStepIdCountAsync(int stepId,
                                                            int cardId)
        {
            return await _context.StepToolExecutions
                                 .Include(e => e.StepTool)
                                 .CountAsync(e => e.StepTool!.StepId == stepId &&
                                                  e.CardId == cardId);
        }

        /// <summary>
        /// Finds existing step tool executions for the given collection of card IDs.
        /// </summary>
        /// <param name="cardIds"></param>
        /// <returns></returns>
        public async Task<ICollection<(int StepToolId, int CardId)>> FindExistingExecutionsAsync(IEnumerable<int> cardIds)
        {
            return await _context.StepToolExecutions
                                 .Where(e => cardIds.Contains(e.CardId))
                                 .Select(e => new ValueTuple<int, int>(e.StepToolId, e.CardId))
                                 .ToListAsync();
        }

        /// <summary>
        /// Deletes the records with the specified IDs from the data store.
        /// </summary>
        /// <remarks>If the specified collection of IDs is empty or none of the IDs match existing
        /// records, no changes are made, and the method returns <see langword="false"/>.</remarks>
        /// <param name="ids">A collection of IDs representing the records to delete. Cannot be null.</param>
        /// <returns><see langword="true"/> if one or more records were successfully deleted; otherwise, <see langword="false"/>.</returns>
        public bool DeleteByIds(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
                return false;

            var executions = _context.StepToolExecutions
                .Where(a => ids!.Contains(a.Id))
                .ToList();

            if (executions.Count > 0)
            {
                _context.StepToolExecutions.RemoveRange(executions);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes all step tool executions associated with the specified card IDs.
        /// </summary>
        /// <remarks>If the specified collection of card IDs is empty or none of the card IDs match existing
        /// records, no changes are made, and the method returns <see langword="false"/>.</remarks>
        /// <param name="cardIds">A collection of card IDs representing the cards whose executions are to be deleted. Cannot be null.</param>
        /// <returns><see langword="true"/> if one or more records were successfully deleted; otherwise, <see langword="false"/>.</returns>
        public bool DeleteByCardIds(IEnumerable<int> cardIds)
        {
            if (!cardIds?.Any() ?? true)
                return false;

            var executions = _context.StepToolExecutions
                .Where(e => cardIds!.Contains(e.CardId))
                .ToList();

            if (executions.Count > 0)
            {
                _context.StepToolExecutions.RemoveRange(executions);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Return StepToolExecution by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StepToolExecution?> FindByIdAsync(int id)
        {
            return await _context.StepToolExecutions
                .Include(e => e.StepTool)
                .Include(e => e.Card)
                    .ThenInclude(e => e!.Document)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Deletes all step tool executions associated with the specified step tool IDs.
        /// </summary>
        /// <param name="stepToolIds">A collection of step tool IDs whose executions are to be deleted.</param>
        /// <returns><see langword="true"/> if one or more records were deleted; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DeleteByStepToolIdsAsync(IEnumerable<int> stepToolIds)
        {
            if (!stepToolIds?.Any() ?? true)
                return false;

            var executions = await _context.StepToolExecutions
                .Where(e => stepToolIds!.Contains(e.StepToolId))
                .ToListAsync();

            if (executions.Count == 0)
                return false;

            _context.StepToolExecutions.RemoveRange(executions);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Returns true if any execution record exists for the specified step tool IDs.
        /// Used to validate whether a tool flow can be removed from a step.
        /// </summary>
        /// <param name="stepToolIds">A collection of StepTool IDs to check.</param>
        /// <returns>True if at least one execution exists; otherwise, false.</returns>
        public async Task<bool> HasExecutionsByStepToolIdsAsync(IEnumerable<int> stepToolIds)
        {
            if (!stepToolIds?.Any() ?? true)
                return false;

            return await _context.StepToolExecutions
                .AnyAsync(e => stepToolIds!.Contains(e.StepToolId));
        }

        /// <summary>
        /// Retrieves all step tool executions for the specified step tool IDs and card IDs in a single query.
        /// </summary>
        /// <remarks>This method is optimized for bulk retrieval of executions and should be used when you need
        /// to evaluate the status of multiple step tools across multiple cards to avoid N+M query patterns.</remarks>
        /// <param name="stepToolIds">A collection of step tool IDs to retrieve executions for.</param>
        /// <param name="cardIds">A collection of card IDs to retrieve executions for.</param>
        /// <returns>A collection of StepToolExecution entities matching the specified step tool and card IDs.</returns>
        public async Task<ICollection<StepToolExecution>> FindByStepToolIdsAndCardIdsAsync(IEnumerable<int> stepToolIds, IEnumerable<int> cardIds)
        {
            var stepToolIdsList = stepToolIds.ToList();
            var cardIdsList = cardIds.ToList();

            return await _context.StepToolExecutions
                .Where(e => stepToolIdsList.Contains(e.StepToolId) && cardIdsList.Contains(e.CardId))
                .ToListAsync();
        }
    }
}
