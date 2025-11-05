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

        public async Task DeleteByStepToolIdAsync(int stepToolId)
        {
            var dependencies = await _context.Set<StepToolDependency>()
                .Where(d => d.StepToolId == stepToolId)
                .ToListAsync();

            if (dependencies.Any())
            {
                _context.Set<StepToolDependency>().RemoveRange(dependencies);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(StepToolDependency dependency)
        {
            ArgumentNullException.ThrowIfNull(dependency);
            await _context.Set<StepToolDependency>().AddAsync(dependency);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<StepToolDependency>> GetByStepToolIdAsync(int stepToolId)
        {
            return await _context.Set<StepToolDependency>()
                .Where(d => d.StepToolId == stepToolId)
                .ToListAsync();
        }
    }
}
