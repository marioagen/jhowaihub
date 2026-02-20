using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
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

        /// <summary>
        /// Retrieves a collection of status information for workflow steps, excluding statuses that are finalized or
        /// rejected.
        /// </summary>
        /// <remarks>This method filters out statuses with names 'Finalize' and 'Rejected' to provide only
        /// relevant statuses for workflow processing.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of StatusDto
        /// objects representing the available statuses for workflow steps.</returns>
        public async Task<ICollection<StatusDto>> FindStatusForWorkflowSteps()
        {
            return await _context.Status
                .AsNoTracking()
                .Where(w => w.Name != StatusNames.Finalize && w.Name != StatusNames.Rejected)
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
