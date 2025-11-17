using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private const string WorkflowStepGroup = "Workflow-Step";
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
                .Where(p => p.Group != WorkflowStepGroup)
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
        /// Find workflow permissions and convert to a PermissionDto list
        /// </summary>
        /// <returns></returns>
        public ICollection<PermissionDto> FindWorkflowPermissions()
        {
            return _context.Permissions
                .Where(p => p.Group == WorkflowStepGroup)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Group = p.Group,
                    Description = p.Description
                })
                .ToList();
        }

        /// <summary>
        /// Search the database for user permissions
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<string>>> FindUserPermissionsAsync(string email)
        {
            var result = await _context.Users
                              .AsNoTracking()
                              .Where(u => u.Email == email)
                              .SelectMany(p => p.Permissions)
                              .Where(p => !string.IsNullOrWhiteSpace(p.Group) &&
                                          !string.IsNullOrWhiteSpace(p.Name))
                              .GroupBy(p => p.Group!.Trim())
                              .ToDictionaryAsync(
                                  g => g.Key!,
                                  g => g.Select(p => p.Name!.Trim())
                                        .Where(n => n.Length > 0)
                                        .Distinct(StringComparer.OrdinalIgnoreCase)
                                        .ToList(),
                                  StringComparer.OrdinalIgnoreCase);

            return result;
        }
    }
}
