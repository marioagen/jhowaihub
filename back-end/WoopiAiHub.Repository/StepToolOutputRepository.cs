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

        public async Task<bool> CreateAsync(StepToolOutput stepToolOutput)
        {
            _context.StepToolOutputs.Add(stepToolOutput);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
