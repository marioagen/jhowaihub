using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StepRepository : IStepRepository
    {
        private readonly ApplicationDbContext _context;
        public StepRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> Create(Step step)
        {
            _context.Steps.Add(step);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates an existing step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> Update(Step step)
        {
            _context.Steps.Update(step);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Retrieves a step by its ID, including related profile, status, and cards.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Step?> FindById(int id)
        {
            return await _context.Steps
                           .Include(s => s.Profile)
                           .Include(s => s.Status)
                           .Include(s => s.Cards)
                           .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves all steps associated with a specific workflow ID.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ICollection<Step> FindByIds(IEnumerable<int> ids)
        {
            return _context.Steps.AsNoTracking().Where(t => ids.Contains(t.Id)).ToList();
        }

        /// <summary>
        /// Delete steps by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(ICollection<int> ids)
        {
            var steps = _context.Steps.Where(a => ids.Contains(a.Id));

            if (steps.Any())
            {
                _context.Steps.RemoveRange(steps);
                _context.SaveChanges();
                return true;
            }

            return false;
        }
    }
}
