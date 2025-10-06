using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IPromptServices
    {
        bool CreateUniquePrompt(PromptCreateDto promptCreateDto,
                                string emailCreator);
        bool Update(PromptUpdateDto promptUpdateDto,
                    string emailCreator);

        PagedResultDto<PromptDto> FindAllPaged(PagedDataDto pagedDataDto,
                                               string emailCreator);
        PagedResultDto<PromptDto> FindByEmailPaged(PagedDataDto pagedDataDto,
                                                   string emailCreator);

        bool DeleteByIds(List<int> ids);

        PromptDto? FindById(int id);
        IQueryable<PromptDto> FindAll(string emailCreator);
    }
}
