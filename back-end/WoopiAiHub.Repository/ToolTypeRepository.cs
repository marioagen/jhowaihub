using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class ToolTypeRepository : IToolTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public ToolTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Return all active tool type records from the database.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ToolTypeDto>> FindAllAsync()
        {
            return await _context.ToolTypes
                .Where(tt => tt.IsActive)
                .Select(tt => new ToolTypeDto
                {
                    Id = tt.Id,
                    Name = tt.Name,
                }).ToListAsync();
        }
    }
}
