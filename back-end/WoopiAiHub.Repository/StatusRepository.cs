using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StatusRepository : IStatusRepository
    {
        private readonly ApplicationDbContext _context;
        public StatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a status by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Status?> FindById(int id)
        {
            return await _context.Status.FindAsync(id);
        }

        /// <summary>
        /// Returns all status.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<StatusDto>> FindAll()
        {
            return await _context.Status
                .AsNoTracking()
                .Select(status => new StatusDto
                {
                   Id = status.Id,
                   Name = status.Name,
                   Label = status.Label,
                   Color = status.Color,
                })
                .ToListAsync();
        }
    }
}
