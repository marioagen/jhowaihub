using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class ToolDataRepository : IToolDataRepository
    {
        private readonly ApplicationDbContext _context;

        public ToolDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Return all active tool data records from the database.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ToolDataDto>> FindAllAsync()
        {
            return await _context.ToolDatas
                .Where(td => td.IsActive)
                .Select(td => new ToolDataDto
                {
                    Id = td.Id,
                    Name = td.Name,
                }).ToListAsync();
        }
    }
}
