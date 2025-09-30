using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ICardServices
    {
        Task<bool> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto,
                                       string tenant,
                                       string email);
        Task<bool> AssignUser(UpdateAssignedUserDto updateAssingnedUserDto);
        Task<bool> UnassignUser(int cardId);
    }
}
