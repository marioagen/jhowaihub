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
    }
}
