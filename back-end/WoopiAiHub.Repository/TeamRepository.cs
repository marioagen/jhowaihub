using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;

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
        /// Find all teams and include their users.
        /// </summary>
        /// <returns></returns>
        public ICollection<Team> FindAll()
        {
            return _context.Teams
                .Include(t => t.Users)
                .AsNoTracking()
                .ToList();
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
            var types = _context.Teams.Where(a => ids.Contains(a.Id));

            if (types.Any())
            {
                _context.Teams.RemoveRange(types);
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
                .Include(t => t.Users)
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

        public List<Team> FindByIds(IEnumerable<int> ids)
        {
            return _context.Teams.Where(t => ids.Contains(t.Id)).ToList();
        }
    }
}
