using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly Context.ApplicationDbContext _context;
        public PermissionRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find permissions by ids and convert to a Permission list
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<Permission>> FindByIdsAsync(List<int> ids)
        {
            return await _context.Permissions.Where(u => ids.Contains(u.Id))
                                             .ToListAsync();
        }

        /// <summary>
        /// Find all permissions and convert to a PermissionDto list
        /// </summary>
        /// <returns></returns>
        public ICollection<PermissionDto> FindAll()
        {
            return _context.Permissions
                .Select(q => new PermissionDto
                {
                    Id = q.Id,
                    Created = q.Created,
                    Name = q.Name,
                    Group = q.Group,
                    Description = q.Description
                })
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// Asynchronously retrieves the permissions for a user, grouped by permission group.
        /// </summary>
        /// <remarks>This method queries the database to retrieve the user's permissions and groups them
        /// by their associated  permission group. The returned dictionary will contain only distinct permission names
        /// within each group.</remarks>
        /// <param name="email">The email address of the user whose permissions are to be retrieved.  This parameter cannot be <see
        /// langword="null"/> or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a dictionary where the keys  are
        /// permission group names and the values are lists of distinct permission names within each group.</returns>
        public async Task<Dictionary<string, List<string>>> FindUserPermissionsAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Email == email);

            var groupedPermissions = user.Permissions
                .GroupBy(p => p.Group)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => p.Name).Distinct().ToList()
                );

            return groupedPermissions;
        }
    }
}
