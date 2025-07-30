using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                }).ToList();
        }

        public async Task<List<string>> GetUserPermissionsAsync(string email)
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
