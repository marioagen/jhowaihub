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
                           .Include(s => s.Workflow)
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
        public bool DeleteByIds(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
                return false;

            var deletedCount = _context.Steps
                .Where(a => ids.Contains(a.Id))
                .ExecuteDelete();

            return deletedCount > 0;
        }

        /// <summary>
        /// Finds a step by its order and workflow ID.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public Task<Step?> FindByOrderAndWorkflowId(int order,
                                                    int workflowId)
        {
            return _context.Steps
                           .FirstOrDefaultAsync(s => s.Order == order &&
                                                     s.WorkflowId == workflowId);
        }
    }
}
