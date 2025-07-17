using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ITeamRepository
    {
        bool CreateUniqueTeam(Team team);
        ICollection<Team> FindAll();
        TeamDto? FindById(int id);
        bool Update(Team team);
        bool DeleteByIds(List<int> ids);
        IQueryable<TeamDto> FindAllPaged(PagedDataDto pagedDataDto);
        List<Team> FindByIds(IEnumerable<int> ids);
        Team FindByIdReturnModel(int id);
    }
}
