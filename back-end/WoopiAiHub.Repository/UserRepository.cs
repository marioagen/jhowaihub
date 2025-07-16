using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly Context.ApplicationDbContext _context;
        public UserRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create an user in the database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(User user)
        {
            var existUser = await _context.Users.AnyAsync(p => p.Email == user.Email && p.IsActive == true);
            if (!existUser)
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Find users by ids and convert to a User list
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<User>> FindByIdsAsync(List<Guid> ids)
        {
            return await _context.Users.Where(u => ids.Contains(u.Id))
                                       .AsNoTracking()
                                       .ToListAsync();
        }

        public async Task<User> FindByReferenceAsync(Guid referenceUserId)
        {
            return await _context.Users.Where(u => u.Id == referenceUserId)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Delete users by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeactivateRange(List<Guid> ids)
        {
            var usersInDb = _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToList();

            foreach (var user in usersInDb)
            {
                user.Deactivate();
            }

            _context.Users.UpdateRange(usersInDb);
            _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Update an user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Find all teams with pagination and include their users.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<UserDtoPaged> FindAllPaged(PagedDataDto pagedDataDto)
        {
            var query = _context.Users.Where(p => p.IsActive == true)
                .Include(t => t.Teams)
                .Select(t => new UserDtoPaged
                {
                    Id = t.Id,
                    Name = t.Name,
                    Created = t.Created,
                    Email = t.Email,
                    IsActive = t.IsActive,
                    Teams = t.Teams!
                        .Select(u => new TeamDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Created = u.Created
                        })
                        .ToList()
                })
                .AsQueryable()
                .AsNoTracking();

            return query;
        }

    }
}
