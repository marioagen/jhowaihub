using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class ToolRepository : IToolRepository
    {
        private readonly ApplicationDbContext _context;

        public ToolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Attempts to add a new tool to the database if a tool with the same name does not already exist.
        /// </summary>
        /// <remarks>This method checks for the existence of a tool with the same name before adding the
        /// new tool to ensure uniqueness.</remarks>
        /// <param name="tool">The tool to be added. The <see cref="Tool.Name"/> property must be unique.</param>
        /// <returns></returns>
        public async Task<bool> CreateUniqueAsync(Tool tool)
        {
            var exists = await _context.Tools.AnyAsync(t => t.Name == tool.Name);
            if (!exists)
            {
                _context.Tools.Add(tool);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes the tools with the specified IDs from the database.
        /// </summary>
        /// <remarks>This method removes all tools matching the provided IDs from the database. If no
        /// matching tools are found, no changes are made to the database.</remarks>
        /// <param name="ids">A list of tool IDs to delete. Each ID must correspond to an existing tool in the database.</param>
        /// <returns></returns>
        public bool Delete(List<int> ids)
        {
            var tools = _context.Tools.Where(a => ids.Contains(a.Id));

            if (tools.Any())
            {
                _context.Tools.RemoveRange(tools);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves all tools from the database, including their associated input data, output data, and tool type.
        /// </summary>
        /// <remarks>This method performs a database query to retrieve all tools, along with their related
        /// entities, and returns the results as a collection of <see cref="ToolDto"/> objects. The query is executed
        /// with no tracking to improve performance in read-only scenarios.</remarks>
        /// <returns></returns>
        public async Task<IEnumerable<ToolDto>> FindAllAsync()
        {
            return await _context.Tools
                .AsNoTracking()
                .Include(t => t.InputData)
                .Include(t => t.OutputData)
                .Include(t => t.ToolType)
                .Select(FormatToolProjection())
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a tool by its unique identifier asynchronously.
        /// </summary>
        /// <remarks>The method performs a database query to retrieve the tool, including its associated
        /// input data,  output data, and tool type. The query is executed with no tracking to improve performance for
        /// read-only operations.</remarks>
        /// <param name="id">The unique identifier of the tool to retrieve. Must be a positive integer.</param>
        /// <returns>.</returns>
        public async Task<ToolDto?> FindByIdAsync(int id)
        {
            return await _context.Tools
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Include(t => t.InputData)
                .Include(t => t.OutputData)
                .Include(t => t.ToolType)
                .Select(FormatToolProjection())
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a tool model by its unique identifier.
        /// </summary>
        /// <remarks>The returned tool includes related data for input, output, and tool type,  which are
        /// eagerly loaded using the <c>Include</c> method.</remarks>
        /// <param name="id">The unique identifier of the tool to retrieve. Must be a positive integer.</param>
        /// <returns></returns>
        public async Task<Tool?> FindModelByIdAsync(int id)
        {
            return await _context.Tools.Where(t => t.Id == id)
                            .Include(t => t.InputData)
                            .Include(t => t.OutputData)
                            .Include(t => t.ToolType)
                            .FirstOrDefaultAsync();
        }


        /// <summary>
        /// Asynchronously retrieves a tool model by stepToolId
        /// </summary>
        /// <param name="stepToolId"></param>
        /// <returns></returns>
        public async Task<Tool?> FindModelByStepToolIdAsync(int stepToolId)
        {
            return await _context.Tools.Where(t => t.StepTools.Any(st => st.Id == stepToolId))
                .Include(t => t.InputData)
                .Include(t => t.OutputData)
                .Include(t => t.ToolType)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates the specified tool in the database if no other tool with the same name exists.
        /// </summary>
        /// <remarks>This method checks for name uniqueness among tools before performing the update. If a
        /// tool with the same name but a different ID exists, the update is not performed.</remarks>
        /// <param name="tool">The tool to update. The tool's <see cref="Tool.Id"/> must match an existing tool in the database.</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Tool tool)
        {
            var existTool = await _context.Tools.AnyAsync(t => t.Name == tool.Name && t.Id != tool.Id);

            if (!existTool)
            {
                _context.Tools.Update(tool);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a paginated collection of tools based on the specified paging parameters.
        /// </summary>
        /// <remarks>The returned query is not executed immediately; it is deferred and will be executed 
        /// when enumerated. Ensure that the provided paging parameters are valid to avoid runtime errors.</remarks>
        /// <param name="pagedDataDto">An object containing the paging parameters, such as the page number and page size,  to determine which
        /// subset of tools to retrieve.</param>
        /// <returns></returns>
        public IQueryable<ToolDto> FindAllPaged()
        {
            return  _context.Tools
                    .Include(t => t.InputData)
                    .Include(t => t.OutputData)
                    .Include(t => t.ToolType)
                    .Select(FormatToolProjection())
                    .AsQueryable()
                    .AsNoTracking();
        }

        /// <summary>
        /// Creates a projection for the Tool entity to ToolDto.
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<Tool, ToolDto>> FormatToolProjection()
        {
            return t => new ToolDto
            {
                Id = t.Id,
                Name = t.Name,
                ToolTypeId = t.ToolType!.Id,
                ToolType = t.ToolType!.Name,
                InputData =  t.InputData!.Name,     
                InputDataId = t.InputData!.Id,
                OutputData = t.OutputData!.Name,
                OutputDataId = t.OutputData!.Id,
                IsEditableInput = t.IsEditableInput,
                ConnectorUrl = t.ConnectorUrl,
                IsConnector = t.ToolType!.Name.Contains(ConnectorNames.N8N)
            };
        }
    }
}
