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
            return output;
        }
    }
}
