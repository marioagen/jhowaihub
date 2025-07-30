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
                                       .ToListAsync();
        }

        /// <summary>
        /// Find users by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<User> FindByEmailAsync(String email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Asynchronously retrieves a user by their unique reference identifier.
        /// </summary>
        /// <param name="referenceUserId">The unique identifier of the user to retrieve. This value must not be empty.</param>
        /// <returns>A <see cref="User"/> object representing the user with the specified identifier,  or <see langword="null"/>
        /// if no matching user is found.</returns>
        public async Task<User> FindByReferenceAsync(Guid referenceUserId)
        {
            return await _context.Users.Where(u => u.Id == referenceUserId)
                                       .Include(t => t.Teams)
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
        public IQueryable<UserPagedDto> FindAllPaged(PagedDataDto pagedDataDto)
        {
            var query = _context.Users.Where(p => p.IsActive == true)
                .Include(t => t.Teams)
                .Select(t => new UserPagedDto
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
                        .ToList(),
                    Profiles = t.Profiles!
                        .Select(u => new ProfileDto
                        {
                            Id = u.Id,
                            Name = u.Name
                        })
                        .ToList()
                })
                .AsQueryable()
                .AsNoTracking();

            return query;
        }

        /// <summary>
        /// Check if an email already exists in the database, excluding a specific user if provided.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="excludeUserId"></param>
        /// <returns></returns>
        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            var query = _context.Users.AsQueryable();

            var normalizedEmail = email.Trim().ToLowerInvariant();

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        }
    }
}
