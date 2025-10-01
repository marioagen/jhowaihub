using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolExecutionRepository
    {
        Task<bool> CreateRangeAsync(List<StepToolExecution> stepToolExecution);
        Task UpdateAsync(StepToolExecution stepToolExecution);
        Task<StepToolExecution?> FindByStepToolIdAndCardIdAsync(int stepToolId, int cardId);
        Task<StepToolExecution?> FindRunningOcrByCardIdAsync(int cardId);
        Task<int> ExecutionsByStepIdCountAsync(int stepId,
                                               int cardId);
        Task<ICollection<(int StepToolId, int CardId)>> FindExistingExecutionsAsync(IEnumerable<int> cardIds);
    }
}
