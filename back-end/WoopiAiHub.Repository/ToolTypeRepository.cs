using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
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
                .AsNoTracking()
                .Where(tt => tt.IsActive)
                .Select(tt => new ToolTypeDto { Id = tt.Id, Name = tt.Name, Description = tt.Description, })
                .ToListAsync();
        }

        /// <summary>
        /// Find ToolType by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ToolTypeDto?> FindByAsync(int id)
        {
            return await _context.ToolTypes
                .AsNoTracking()
                .Where(tt => tt.IsActive && tt.Id == id)
                .Select(tt => new ToolTypeDto { Id = tt.Id, Name = tt.Name, Description = tt.Description, })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a tool type model by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tool to retrieve. Must be a positive integer.</param>
        /// <returns></returns>
        public async Task<ToolType?> FindModelByIdAsync(int id)
        {
            return await _context.ToolTypes.FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
