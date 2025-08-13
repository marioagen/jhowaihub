using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepRepository
    {
        Task<Step?> FindById(int id);
    }
}
