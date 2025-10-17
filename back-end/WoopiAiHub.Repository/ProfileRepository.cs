using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new profile if it does not already exist.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool CreateUniqueProfile(Domain.Models.Profile profile)
        {
            var exists = _context.Profiles.Any(t => t.Name == profile.Name);
            if (!exists)
            {
                _context.Profiles.Add(profile);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find all profiles
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ProfileDto>> FindAll()
        {
            return await _context.Profiles
                .AsNoTracking()
                .Select(t => new ProfileDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Created = t.Created
                })                
                .ToListAsync();
        }

        /// <summary>
        /// Find a profile by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProfileDto?> FindById(int id)
        { 
            return await _context.Profiles
                .Include(t => t.Permissions)
                .Select(t => new ProfileDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Created = t.Created,
                    Permissions = t.Permissions
                        .Select(u => new PermissionDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Group = u.Group,
                            Description = u.Description,
                            Created = u.Created
                        })
                        .ToList(),
                     Users = t.Users
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Created = u.Created
                        }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Update a profile if it does not already exist with the same name.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool Update(Domain.Models.Profile profile)
        {
            _context.Profiles.Update(profile);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Delete profiles by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var profiles = _context.Profiles.Where(a => ids.Contains(a.Id));

            if (profiles.Any())
            {
                _context.Profiles.RemoveRange(profiles);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Find all profiles with pagination and include their permissions.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<ProfileDto> FindAllPaged(PagedDataDto pagedDataDto)
        {
            var query = _context.Profiles
                .Include(t => t.Permissions)
                .Where(p => p.Name != Profile.IAFileName)
                .Select(t => new ProfileDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Created = t.Created,
                    Permissions = t.Permissions
                        .Select(u => new PermissionDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Created = u.Created,
                            Description = u.Description,
                            Group = u.Group
                        })
                        .ToList(),
                        Users = t.Users
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Created = u.Created
                        }).ToList()
                })
                .AsQueryable()
                .AsNoTracking();

            return query;
        }

        /// <summary>
        /// Find profiles by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ICollection<Domain.Models.Profile> FindByIds(IEnumerable<int> ids)
        {
            return _context.Profiles.Where(t => ids.Contains(t.Id)).ToList();
        }

        /// <summary>
        /// Find a profile by its ID and returns a model.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Domain.Models.Profile FindByIdReturnModel(int id)
        {
            return _context.Profiles.Where(u => u.Id == id)
                                    .Include(t => t.Permissions)
                                    .Include(t => t.Users)
                                    .FirstOrDefault();
        }
    }
}
