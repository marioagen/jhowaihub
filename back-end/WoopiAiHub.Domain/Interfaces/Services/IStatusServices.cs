using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IStatusServices
    {
        Task<ICollection<StatusDto>> FindAll();
        Task<ICollection<StatusDto>> FindStatusForWorkflowSteps();
    }
}
