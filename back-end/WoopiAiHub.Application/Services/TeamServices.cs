using Humanizer;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class TeamServices : ITeamServices
    {
        private readonly ITeamRepository _teamRepository;

        public TeamServices(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Retrieves a team by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TeamDto FindById(int id)
        {
            var team = _teamRepository.FindById(id);
            if (team == null)
            {
                throw new ArgumentException("Team not found");
            }
            return team;
        }

        /// <summary>
        /// Retrieves all teams paged and their users.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public TeamPagedResultDto FindAllPaged(PagedDataDto pagedDataDto)
        {
            if (pagedDataDto.Page > 0)
            {
                var totalList = _teamRepository.FindAllPaged(pagedDataDto);

                totalList = pagedDataDto.IsAscending ?
                    totalList.OrderBy(team => team.Name) :
                    totalList.OrderByDescending(team => team.Name);

                var result = Pagination(totalList, pagedDataDto);
                return result;
            }
            else
            {
                var ex = new ArgumentException("The number of pages must be greater than 0");
                throw ex;
            }
        }

        /// <summary>
        /// Creates a new team with a unique name.
        /// </summary>
        /// <param name="teamCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool CreateUniqueTeam(TeamCreateDto teamCreateDto)
        {
            if (string.IsNullOrEmpty(teamCreateDto.Name))
            {
                throw new ArgumentException("Team name cannot be empty");
            }

            var team = new Team(teamCreateDto.Name, 0, DateTime.Now)
            {
                Users = new List<User>()
            };

            var createResult = _teamRepository.CreateUniqueTeam(team);
            if (!createResult)
            {
                throw new ArgumentException("Duplicated Team Name");
            }
            return createResult;
        }

        /// <summary>
        /// Updates an existing team based on the provided DTO.
        /// </summary>
        /// <param name="teamUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool Update(TeamUpdateDto teamUpdateDto)
        {
            if (string.IsNullOrEmpty(teamUpdateDto.Name))
            {
                throw new ArgumentException("Team name cannot be empty");
            }

            var team = _teamRepository.FindById(teamUpdateDto.Id);
            if (team == null)
            {
                throw new ArgumentException("Team not found");
            }

            var teamUpdate = GenerateTeamToUpdate(teamUpdateDto, team);

            var updateResult = _teamRepository.Update(teamUpdate);
            if (!updateResult)
            {
                throw new ArgumentException("Duplicated Team Name");
            }
            return true;
        }

        /// <summary>
        /// Generates a team object to update based on the provided DTO and existing team.
        /// </summary>
        /// <param name="teamUpdateDto"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        private static Team GenerateTeamToUpdate(TeamUpdateDto teamUpdateDto, TeamDto team)
        {
            if (team.Users != null && team.Users.Any())
            {
                var userIdsToKeep = teamUpdateDto.UserIds.Select(id => new Guid(id.ToString())).ToHashSet();
                var usersToRemove = team.Users.Where(u => !userIdsToKeep.Contains(u.Id)).ToList();

                foreach (var user in usersToRemove)
                {
                    teamUpdateDto.UserIds.Remove(user.Id);
                }
            }

            var users = teamUpdateDto.UserIds?
                .Select(id => new User(new Guid(id.ToString()), string.Empty, string.Empty, true, DateTime.Now))
                .ToList();

            var teamToUpdate = new Team(teamUpdateDto.Name, teamUpdateDto.Id, DateTime.Now)
            {
                Users = users
            };

            return teamToUpdate;
        }

        /// <summary>
        /// Deletes a list of teams by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
             return _teamRepository.DeleteByIds(ids);
        }

        /// <summary>
        /// Ordenates the list of Teams and returns a paged result.
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        private static TeamPagedResultDto Pagination(IQueryable<TeamDto> totalList,
                                                      PagedDataDto pagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(pagedDataDto.Search))
            {
                totalList = totalList.Where(i => i.Name.ToLower()
                                                        .Contains(pagedDataDto.Search.ToLower()) ||
                                                 i.Id.ToString().Contains(pagedDataDto.Search));
            }

            var totalListCount = totalList.Count();

            if (pagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                pagedDataDto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / pagedDataDto.PageSize);
                currentPage = pagedDataDto.Page <= pageCount ? pagedDataDto.Page : 1;
                totalList = totalList.Skip((currentPage - 1) * pagedDataDto.PageSize)
                                     .Take(pagedDataDto.PageSize);
            }

            return new TeamPagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount,
            };
        }
    }
}
