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

        public async Task<bool> CreateRangeAsync(List<StepToolExecution> stepToolExecutions)
        {
            await _context.StepToolExecutions.AddRangeAsync(stepToolExecutions);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StepToolExecution?> FindRunningOcrByCardIdAsync(int cardId)
        {
            return await _context.StepToolExecutions
                                 .FirstOrDefaultAsync(s => s.CardId == cardId &&
                                                           s.Status == Domain.Enum.StatusExecution.Running &&
                                                           s.StepTool.Tool.ToolType.Equals("Ocr"));
        }

        public async Task<StepToolExecution?> FindByStepToolIdAndCardIdAsync(int stepToolId, int cardId)
        {
            return await _context.StepToolExecutions
                                 .FirstOrDefaultAsync(s => s.StepToolId.Equals(stepToolId) &&
                                                           s.CardId.Equals(cardId));
        }

        public async Task UpdateAsync(StepToolExecution stepToolExecution)
        {
            _context.StepToolExecutions.Update(stepToolExecution);
            await _context.SaveChangesAsync();
        }
    }
}
