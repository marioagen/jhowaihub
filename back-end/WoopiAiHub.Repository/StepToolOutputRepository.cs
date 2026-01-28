using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class StepToolOutputRepository : IStepToolOutputRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public StepToolOutputRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously creates a new <see cref="StepToolOutput"/> record in the database.
        /// </summary>
        /// <param name="stepToolOutput">The <see cref="StepToolOutput"/> instance to be added to the database. Cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// operation completes successfully.</returns>
        public async Task<bool> CreateAsync(StepToolOutput stepToolOutput)
        {
            _context.StepToolOutputs.Add(stepToolOutput);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves the value associated with the specified step tool identifier.
        /// </summary>
        /// <remarks>This method queries the database asynchronously to find the first value associated
        /// with the given  <paramref name="stepToolId"/>. If no matching record exists, the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="stepToolId">The identifier of the step tool whose value is to be retrieved.</param>
        /// <returns>A string representing the value associated with the specified step tool identifier,  or <see
        /// langword="null"/> if no matching record is found.</returns>
        public async Task<string> FindByStepToolId(int stepToolId,
            int cardId)
        {
            var output = await _context.StepToolOutputs.Where(u => u.StepToolId.Equals(stepToolId) &&
                                                                   u.CardId.Equals(cardId))
                .Select(v => v.Value)
                .FirstOrDefaultAsync();
            return output ?? string.Empty;
        }

        /// <summary>
        /// Retrieves all outputs associated with the specified step tool identifiers and card ID.
        /// </summary>
        /// <param name="stepToolId">The identifiers of the step tools whose outputs are to be retrieved.</param>
        /// <param name="cardId">The identifier of the card associated with the outputs.</param>
        /// <returns>A list of StepToolOutput objects associated with the specified step tool and card.</returns>
        public async Task<List<StepToolOutput>> FindAllByStepToolListIdsAsync(IEnumerable<int> stepToolIds, int cardId)
        {
            return await _context.StepToolOutputs
                .AsNoTracking()
                .Include(sto => sto.StepTool)
                .ThenInclude(st => st.Tool)
                .ThenInclude(t => t!.ToolType)
                .Where(u => stepToolIds!.Contains(u.StepToolId) &&
                            u.CardId.Equals(cardId))
                .ToListAsync();
        }

        /// <summary>
        /// Deletes the entities with the specified IDs from the data source.
        /// </summary>
        /// <remarks>If the specified collection of IDs is empty or none of the IDs match existing
        /// entities, no changes are made, and the method returns <see langword="false"/>.</remarks>
        /// <param name="ids">A collection of IDs representing the entities to delete. Cannot be null.</param>
        /// <returns><see langword="true"/> if one or more entities were successfully deleted; otherwise, <see
        /// langword="false"/>.</returns>
        public bool DeleteByIds(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
                return false;

            var outputs = _context.StepToolOutputs
                .Where(a => ids!.Contains(a.Id))
                .ToList();

            if (outputs.Count > 0)
            {
                _context.StepToolOutputs.RemoveRange(outputs);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds all step tool outputs for a specific card.
        /// </summary>
        /// <param name="cardId">The ID of the card.</param>
        /// <returns>A list of step tool outputs with related StepTool and Tool information.</returns>
        public async Task<List<StepToolOutput>> FindByCardIdAsync(int cardId)
        {
            return await _context.StepToolOutputs
                .Where(o => o.CardId == cardId)
                .Include(o => o.StepTool)
                .ThenInclude(st => st.Tool)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if any of the specified StepTools have associated output data.
        /// </summary>
        /// <param name="stepToolIds">The collection of StepTool IDs to check.</param>
        /// <returns>True if any StepTool has outputs; otherwise, false.</returns>
        public async Task<bool> HasOutputsByStepToolIds(IEnumerable<int> stepToolIds)
        {
            if (!stepToolIds?.Any() ?? true)
                return false;

            return await _context.StepToolOutputs
                .AnyAsync(o => stepToolIds!.Contains(o.StepToolId));
        }

        /// <summary>
        /// Deletes all step tool outputs associated with the specified card IDs.
        /// </summary>
        /// <remarks>If the specified collection of card IDs is empty or none of the card IDs match existing
        /// entities, no changes are made, and the method returns <see langword="false"/>.</remarks>
        /// <param name="cardIds">A collection of card IDs representing the cards whose outputs are to be deleted. Cannot be null.</param>
        /// <returns><see langword="true"/> if one or more entities were successfully deleted; otherwise, <see langword="false"/>.</returns>
        public bool DeleteByCardIds(IEnumerable<int> cardIds)
        {
            if (!cardIds?.Any() ?? true)
                return false;

            var outputs = _context.StepToolOutputs
                .Where(o => cardIds!.Contains(o.CardId))
                .ToList();

            if (outputs.Count > 0)
            {
                _context.StepToolOutputs.RemoveRange(outputs);
                _context.SaveChanges();
                return true;
            }

            return false;
        }
    }
}
