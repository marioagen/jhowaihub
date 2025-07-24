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
        /// Find users by ids and convert to a User list
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<Permission>> FindByIdsAsync(List<int> ids)
        {
            return await _context.Permissions.Where(u => ids.Contains(u.Id))
                                             .ToListAsync();
        }

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
    }
}
