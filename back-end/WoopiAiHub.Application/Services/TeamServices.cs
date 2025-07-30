using Microsoft.EntityFrameworkCore;
using PdfSharp;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WoopiAiHub.Application.Services
{
    public class TeamServices : ITeamServices
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        public TeamServices(ITeamRepository teamRepository,
                            IUserRepository userRepository)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
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
        /// Retrieves a collection of teams based on the specified list of team IDs.
        /// </summary>
        /// <remarks>This method ensures that all provided IDs correspond to existing teams. If any ID
        /// does not match an existing team, or if the list of IDs is empty, an exception is thrown.</remarks>
        /// <param name="ids">A list of team IDs to search for. Each ID must correspond to an existing team.</param>
        /// <returns>A collection of <see cref="Team"/> objects that match the provided IDs.</returns>
        /// <exception cref="ArgumentException">Thrown if no teams are found, or if the number of teams found does not match the number of IDs provided.</exception>
        public ICollection<Team> FindByIdsAndUser(ICollection<int> ids,
                                                  string emailUser)
        {
            var teams = _teamRepository.FindByIdsAndUser(ids,
                                                         emailUser);

            if (teams == null || teams.Count == 0)
                throw new ArgumentException("No teams were found");

            if (teams.Count != ids.Count)
                throw new ArgumentException("Some teams were not found");

            return teams;
        }

        /// <summary>
        /// Retrieves a paginated list of teams, optionally filtered by the specified user's email.
        /// </summary>
        /// <remarks>The results are sorted by team name in ascending or descending order, based on the
        /// value of  <see cref="PagedDataDto.IsAscending"/>.</remarks>
        /// <param name="pagedDataDto">The pagination and sorting information, including the page number, page size, and sort direction. The <see
        /// cref="PagedDataDto.Page"/> property must be greater than 0.</param>
        /// <param name="emailUser">An optional email address to filter the teams by user. If null or empty, all teams are retrieved.</param>
        /// <returns>A <see cref="TeamPagedResultDto"/> containing the paginated list of teams and pagination metadata.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pagedDataDto"/> has a <see cref="PagedDataDto.Page"/> value less than or equal to
        /// 0.</exception>
        public TeamPagedResultDto FindAllPaged(PagedDataDto pagedDataDto,
                                               string? emailUser = null)
        {
            if (pagedDataDto.Page <= 0)
                throw new ArgumentException("The number of pages must be greater than 0");

            IQueryable<TeamDto> query;

            if (!string.IsNullOrEmpty(emailUser))
            {
                query = _teamRepository.FindAllByUser(emailUser);
            }
            else
            {
                query = _teamRepository.FindAllPaged(pagedDataDto);
            }

            query = pagedDataDto.IsAscending
                ? query.OrderBy(t => t.Name)
                : query.OrderByDescending(t => t.Name);

            return Pagination(query, pagedDataDto);
        }

        /// <summary>
        /// Creates a new team with a unique name.
        /// </summary>
        /// <param name="teamCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> CreateUniqueTeam(TeamCreateDto teamCreateDto)
        {
            if (string.IsNullOrEmpty(teamCreateDto.Name))
            {
                throw new AppException(Domain.Enum.ErrorCode.Duplicated, "Team name cannot be empty");
            }

            var team = new Team(teamCreateDto.Name, 0, DateTime.Now)
            {
                Users = new List<User>()
            };

            if (teamCreateDto.UserIds != null)
            {
                team.Users.Clear();
                var users = await _userRepository.FindByIdsAsync(teamCreateDto.UserIds);

                foreach (var user in users)
                {
                    team.AddUser(user);
                }
            }

            var createResult = _teamRepository.CreateUniqueTeam(team);
            if (!createResult)
            {
                throw new AppException(Domain.Enum.ErrorCode.Duplicated, "Duplicated Team Name");
            }
            return createResult;
        }

        /// <summary>
        /// Updates an existing team based on the provided DTO.
        /// </summary>
        /// <param name="teamUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> Update(TeamUpdateDto teamUpdateDto)
        {

            var team = _teamRepository.FindByIdReturnModel(teamUpdateDto.Id);
            if (team == null)
                return false;

            team.Update(teamUpdateDto.Name);

            if (teamUpdateDto.UserIds != null)
            {
                team.Users.Clear();
                var users = await _userRepository.FindByIdsAsync(teamUpdateDto.UserIds);

                foreach (var user in users)
                {
                    team.AddUser(user);
                }
            }

            var updateResult = _teamRepository.Update(team);
            if (!updateResult)
            {
                throw new AppException(Domain.Enum.ErrorCode.Duplicated, "Duplicated Team Name");
            }
            return updateResult;
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
