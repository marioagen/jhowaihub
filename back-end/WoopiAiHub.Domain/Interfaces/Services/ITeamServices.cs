using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ITeamServices
    {
        TeamDto FindById(int id);
        TeamPagedResultDto FindAllPaged(PagedDataDto pagedDataDto);
        bool CreateUniqueTeam(TeamCreateDto teamCreateDto);
        Task<bool> Update(TeamUpdateDto teamUpdateDto);
        bool DeleteByIds(List<int> ids);
    }
}
