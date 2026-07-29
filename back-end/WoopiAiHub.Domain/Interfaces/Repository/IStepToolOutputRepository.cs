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
        Task<bool> DeleteByStepToolIdsAsync(IEnumerable<int> stepToolIds);

        /// <summary>
        /// Retrieves all outputs for a card with the full join needed for CSV export
        /// (StepTool → Step → Tool), already ordered by Step.Order then StepTool.Order.
        /// </summary>
        Task<List<StepToolOutput>> FindForExportByCardIdAsync(int cardId);
    }
}

