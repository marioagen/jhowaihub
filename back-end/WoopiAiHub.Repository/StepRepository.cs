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
        /// Retrieves a step by its ID, including related profile, status, and cards.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Step?> FindById(int id)
        {
            return _context.Steps
                           .Include(s => s.Profile)
                           .Include(s => s.Status)
                           .Include(s => s.Cards)
                           .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
