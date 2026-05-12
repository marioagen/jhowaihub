using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IPromptRepository
    {
        bool Create(Prompt prompt);
        Prompt? CreateAndReturn(Prompt prompt);
        bool CreateByRange(List<Prompt> prompts);
        bool Delete(List<int> ids);
        bool Update(Prompt prompt);
        PromptDto? FindById(int id);
        IQueryable<PromptDto> FindAllWithOwnerStatus(Guid idUser);
        IQueryable<PromptDto> FindByIdUser(Guid idUser);
        Task<ICollection<PromptIntegrationDto>> FindAllInternal();
        Task<List<PromptApiTemplate>> FindPromptApiTemplatesByIds(List<int> ids);
        Task UpdateAndRemovePromptApisFromPrompt(Prompt prompt, List<PromptApiTemplate> templatesToRemove);
        Prompt? FindByNameAndUser(string name, Guid idUser);
    }
}
