using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolExecutionRepository
    {
        Task<StepToolExecution?> FindByIdAsync(int id);
        Task<bool> CreateRangeAsync(List<StepToolExecution> stepToolExecutions);
        Task UpdateAsync(StepToolExecution stepToolExecution);
        Task<StepToolExecution?> FindByStepToolIdAndCardIdAsync(int stepToolId, int cardId);
        Task<StepToolExecution?> FindRunningOcrByCardIdAsync(int cardId);
        Task<int> ExecutionsByStepIdCountAsync(int stepId,
                                               int cardId);
        Task<ICollection<(int StepToolId, int CardId)>> FindExistingExecutionsAsync(IEnumerable<int> cardIds);
        bool DeleteByIds(IEnumerable<int> ids);
    }
}
