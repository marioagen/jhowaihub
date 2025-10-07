using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IPromptServices
    {
        public bool CreateUniquePrompt(PromptCreateDto promptCreateDto);
        bool Update(PromptUpdateDto promptUpdateDto);

        PagedResultDto<PromptDto> FindAllPaged(PagedDataDto pagedDataDto,
                                                Guid idUser);
        PagedResultDto<PromptDto> FindByIdUserPaged(PagedDataDto pagedDataDto,
                                                    Guid idUser);

        bool DeleteByIds(List<int> ids);

        PromptDto? FindById(int id);
        IQueryable<PromptDto> FindAll(Guid idUser);
    }
}
