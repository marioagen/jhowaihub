using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StepToolDependencyRepository : IStepToolDependencyRepository
    {
        private readonly ApplicationDbContext _context;

        public StepToolDependencyRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Deletes all StepToolDependency records associated with the provided step tool IDs.
        /// </summary>
        /// <param name="stepToolIds"></param>
        /// <returns></returns>
        public async Task DeleteByStepToolIdAsync(IEnumerable<int> stepToolIds)
        {
            var dependencies = await _context.Set<StepToolDependency>()
                .Where(d => stepToolIds!.Contains(d.StepToolId) || stepToolIds!.Contains(d.DependsOnStepToolId))
                .ToListAsync();

            if (dependencies.Count > 0)
            {
                _context.Set<StepToolDependency>().RemoveRange(dependencies);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds a new StepToolDependency record to the database asynchronously.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public async Task CreateAsync(StepToolDependency dependency)
        {
            ArgumentNullException.ThrowIfNull(dependency);
            await _context.Set<StepToolDependency>().AddAsync(dependency);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Finds all StepToolDependency records associated with the specified step tool ID.
        /// </summary>
        /// <param name="stepToolId"></param>
        /// <returns></returns>
        public async Task<ICollection<StepToolDependency>> FindByStepToolIdAsync(int stepToolId)
        {
            return await _context.Set<StepToolDependency>()
                .Where(d => d.StepToolId == stepToolId)
                .ToListAsync();
        }
    }
}
