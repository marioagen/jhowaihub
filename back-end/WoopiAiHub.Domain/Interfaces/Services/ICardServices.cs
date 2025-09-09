using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ICardServices
    {
        Task<bool> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto);
        Task<bool> UpdateAssignedUser(UpdateAssignedUserDto updateAssingnedUserDto);
    }
}
