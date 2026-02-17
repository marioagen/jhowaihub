using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStatusRepository
    {
        Task<Status?> FindById(int id);
        Task<ICollection<StatusDto>> FindAll();
        Task<Status?> FindByName(string name);
    }
}
