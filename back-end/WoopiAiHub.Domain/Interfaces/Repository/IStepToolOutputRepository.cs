using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolOutputRepository
    {
        Task<bool> CreateAsync(StepToolOutput stepToolOutput);
        Task<List<StepToolOutput>> FindAllByStepToolListIdsAsync(IEnumerable<int> stepToolIds, int cardId);
        bool DeleteByIds(IEnumerable<int> ids);
        Task<string> FindByStepToolId(int stepToolId, int cardId);
        Task<List<StepToolOutput>> FindByCardIdAsync(int cardId);
        Task<bool> HasOutputsByStepToolIds(IEnumerable<int> stepToolIds);
        bool DeleteByCardIds(IEnumerable<int> cardIds);
    }
}

