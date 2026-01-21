using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IPromptServices
    {
        bool CreateUniquePrompt(PromptCreateDto promptCreateDto, string email);
        bool Update(PromptUpdateDto promptUpdateDto, string emailCreator);
        PagedResultDto<PromptDto> FindAllPaged(PagedDataDto pagedDataDto, string emailCreator);
        PagedResultDto<PromptDto> FindByIdUserPaged(PagedDataDto pagedDataDto, string emailCreator);
        bool DeleteByIds(List<int> ids);
        PromptDto? FindById(int id);
        IQueryable<PromptDto> FindAll(string emailCreator);
        Task ProcessChatCompletionResult(ChatCompletionResponseDto chatCompletionResponseDto);
        bool ImportPrompts(List<ImportedPromptDto> importedPrompts, string email);
        Task<List<PromptTemplateDto>> FindPromptTemplates(string? query, string? orderBy);
        Task<bool> ImportPromptsByIds(List<Guid> templateIds, string email);
        Task<ICollection<PromptInternalDto>> FindAllInternal();
    }
}
