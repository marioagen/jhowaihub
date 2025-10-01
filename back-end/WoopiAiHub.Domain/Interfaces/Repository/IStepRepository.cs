using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepRepository
    {
        Task<bool> Create(Step step);
        Task<bool> Update(Step step);
        Task<Step?> FindById(int id);
        ICollection<Step> FindByIds(IEnumerable<int> ids);
        bool DeleteByIds(IEnumerable<int> ids);
        Task<Step?> FindByOrderAndWorkflowId(int order,
                                             int workflowId);
    }
}
