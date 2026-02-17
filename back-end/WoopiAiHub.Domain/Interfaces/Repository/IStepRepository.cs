using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepRepository
    {
        Task<bool> Create(Step step);
        Task<bool> Update(Step step);
        Task<Step?> FindById(int id);
        Task<Step?> FindByIdWithTools(int id);
        ICollection<Step> FindByIds(IEnumerable<int> ids);
        bool DeleteByIds(IEnumerable<int> ids);
        Task<Step?> FindByOrderAndWorkflowId(int order,
                                             int workflowId);
        Task CreateRange(ICollection<Step> steps);
        ICollection<Step> FindByIdsWithCards(IEnumerable<int> ids);
        Task<List<StepDto>> FindStepsByWorkflowId(int id, 
                                                  string input = "", 
                                                  bool allUsers = false, 
                                                  string login = "", 
                                                  string order = "");
        Task<List<StepDto>> FindPreviousStepsByWorkflowIdAndOrder(int workflowId,
                                                                 int order);
        Task<Step?> FindStepByCardId(int cardId);
    }
}
