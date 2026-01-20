using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IPromptRepository
    {
        bool CreateUniquePrompt(Prompt prompt);
        bool CreateByRange(List<Prompt> prompts);
        bool Delete(List<int> ids);
        bool Update(Prompt prompt);
        PromptDto? FindById(int id);
        IQueryable<PromptDto> FindAllWithOwnerStatus(Guid idUser);
        IQueryable<PromptDto> FindByIdUser(Guid idUser);
        Task<ICollection<PromptBaseDto>> FindAllBasic();
    }
}
