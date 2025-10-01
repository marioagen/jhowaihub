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
                                                           s.StepTool.Tool.ToolType.Equals("Ocr"));
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
                                 .FirstOrDefaultAsync(s => s.StepToolId.Equals(stepToolId) &&
                                                           s.CardId.Equals(cardId));
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
    }
}
