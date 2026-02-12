using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ITeamRepository
    {
        bool CreateUniqueTeam(Team team);
        TeamDto? FindById(int id);
        bool Update(Team team);
        bool DeleteByIds(List<int> ids);
        IQueryable<TeamDto> FindAllPaged(PagedDataDto pagedDataDto);
        List<Team> FindByIds(IEnumerable<int> ids);
        Team FindByIdReturnModel(int id);
        IQueryable<TeamDto> FindAll();
        ICollection<Team> FindByIdsAndUser(IEnumerable<int> ids,
                                           string emailUser);
        IQueryable<TeamDto> FindAllByUser(string userEmail);
        IQueryable<TeamSimpleDto> FindAllSimple();
    }
}
