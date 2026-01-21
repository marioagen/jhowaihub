using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class TeamRepository : ITeamRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public TeamRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new team if it does not already exist.
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public bool CreateUniqueTeam(Team team)
        {
            var exists = _context.Teams.Any(t => t.Name == team.Name);
            if (!exists)
            {
                _context.Teams.Add(team);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieve all teams associated with a specific user email, including their active users.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public IQueryable<TeamDto> FindAll()
        {
            return _context.Teams
                           .Include(u => u.Users)
                           .Include(w => w.Workflows)
                           .Select(t => new TeamDto
                           {
                               Id = t.Id,
                               Name = t.Name,
                               Created = t.Created,
                               Users = t.Users!
                                       .Where(u => u.IsActive)
                                       .Select(u => new UserDto
                                       {
                                           Id = u.Id,
                                           Name = u.Name,
                                           Email = u.Email,
                                           IsActive = u.IsActive,
                                           Created = u.Created
                                       })
                                       .ToList(),
                               Workflows = t.Workflows != null
                                    ? t.Workflows.Select(w => new WorkflowDto
                                    {
                                        Id = w.Id,
                                        Name = w.Name,
                                        Created = w.Created
                                    }).ToList()
                                    : new List<WorkflowDto>()
                           })
                           .OrderBy(x => x.Name)
                           .AsNoTracking();
        }

        /// <summary>
        /// Find a team by its ID and include its users.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TeamDto? FindById(int id)
        {
            return _context.Teams
                .Include(t => t.Users)
                .Include(t => t.Profiles)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Created = t.Created,
                    Users = t.Users!
                        .Where(u => u.IsActive)
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Email = u.Email,
                            IsActive = u.IsActive,
                            Created = u.Created
                        })
                        .ToList(),
                    Profiles = t.Profiles!
                        .Select( p => new ProfileDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                        })
                        .ToList(),
                })
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Find a team by its ID and include its users.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Team FindByIdReturnModel(int id)
        {
            return _context.Teams.Where(u => u.Id == id)
                                        .Include(t => t.Users)
                                        .Include(T => T.Profiles)
                                            .ThenInclude(p => p.StepProfilePermissions)
                                        .FirstOrDefault();
        }

        /// <summary>
        /// Update a team if it does not already exist with the same name.
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public bool Update(Team team)
        {
            _context.Teams.Update(team);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Delete teams by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var teams = _context.Teams
                .Where(a => ids.Contains(a.Id));

            if (teams.Any())
            {
                _context.Teams.RemoveRange(teams);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Find all teams with pagination and include their users.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<TeamDto> FindAllPaged(PagedDataDto pagedDataDto)
        {
            var query = _context.Teams
                                .Include(u => u.Users)
                                .Select(t => new TeamDto
                                {
                                    Id = t.Id,
                                    Name = t.Name,
                                    Created = t.Created,
                                    Users = t.Users!
                                        .Where(u => u.IsActive)
                                        .Select(u => new UserDto
                                        {
                                            Id = u.Id,
                                            Name = u.Name,
                                            Email = u.Email,
                                            IsActive = u.IsActive,
                                            Created = u.Created
                                        })
                                        .ToList()
                                })
                                .AsQueryable()
                                .AsNoTracking();

            return query;
        }

        /// <summary>
        /// Retrieve all teams associated with a specific user email, including their active users.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public IQueryable<TeamDto> FindAllByUser(string userEmail)
        {
            return _context.Teams
                           .Include(u => u.Users)
                           .Include(w => w.Workflows)
                           .Select(t => new TeamDto
                           {
                               Id = t.Id,
                               Name = t.Name,
                               Created = t.Created,
                               Users = t.Users!
                                       .Where(u => u.IsActive)
                                       .Select(u => new UserDto
                                       {
                                           Id = u.Id,
                                           Name = u.Name,
                                           Email = u.Email,
                                           IsActive = u.IsActive,
                                           Created = u.Created
                                       })
                                       .ToList(),
                               Workflows = t.Workflows != null
                                    ? t.Workflows.Select(w => new WorkflowDto
                                    {
                                        Id = w.Id,
                                        Name = w.Name,
                                        Created = w.Created
                                    }).ToList()
                                    : new List<WorkflowDto>()
                           })
                           .Where(t => t.Users.Any(u => u.Email == userEmail && u.IsActive))
                           .AsNoTracking();
        }

        /// <summary>
        /// Retrieves a list of teams that match the specified identifiers.
        /// </summary>
        /// <param name="ids">A collection of team identifiers to search for. Each identifier must correspond to a valid team.</param>
        /// <returns>A list of <see cref="Team"/> objects whose identifiers match the specified <paramref name="ids"/>. If no
        /// matches are found, an empty list is returned.</returns>
        public List<Team> FindByIds(IEnumerable<int> ids)
        {
            return _context.Teams.Where(t => ids.Contains(t.Id))
                    .Include(t => t.Profiles)
                    .Include(t => t.Workflows)
                    .ToList();
        }

        /// <summary>
        /// Retrieves a collection of teams that match the specified team IDs and are associated with the specified user.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="emailUser"></param>
        /// <returns></returns>
        public ICollection<Team> FindByIdsAndUser(IEnumerable<int> ids,
                                                  string emailUser)
        {
            return _context.Teams
                           .Include(t => t.Workflows)
                           .ThenInclude(w => w!.Steps)
                           .ThenInclude(s => s.StepTools)
                           .ThenInclude(st => st.Tool)
                           .ThenInclude(t => t!.ToolType)
                           .Where(t => ids.Contains(t.Id) &&
                                       t.Users.Any(s => s.Email.Equals(emailUser)))
                           .ToList();
        }
    }
}
