using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolOutputRepository
    {
        Task<bool> CreateAsync(StepToolOutput stepToolOutput);
        Task<List<StepToolOutput>> FindAllByStepToolListIdsAsync(IEnumerable<int> stepToolIds, int cardId);
        bool DeleteByIds(IEnumerable<int> ids);
        Task<string> FindByStepToolId(int stepToolId, int cardId);
    }
}
