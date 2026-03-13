using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StepToolRepository : IStepToolRepository
    {
        private readonly ApplicationDbContext _context;

        public StepToolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Delete steps by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
                return false;

            var deletedCount = _context.StepTools
                .Where(a => ids!.Contains(a.Id))
                .ExecuteDelete();

            return deletedCount > 0;
        }

        /// <summary>
        /// Finds a step by its order and workflow ID.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public Task<StepToolDto?> FindById(int id)
        {
            return _context.StepTools
            .Select(q => new StepToolDto
            {
                Id = q.Id,
                Name = q.Tool!.Name,
                StepId = q.StepId,
                ToolId = q.ToolId,
                Order = q.Order,
                PositionX = q.PositionX,
                PositionY = q.PositionY,
                DependsOnStepToolId = q.DependsOnStepToolId,
                Parameters = q.Parameters.Select(sp => new StepToolParameterDto
                {
                    Id = sp.Id,
                    Type = sp.StepTool!.Tool!.InputData!.Name,
                    Value = sp.Value,
                    RequiredFile = sp.RequiredFile,
                    WebhookId = sp.WebhookId
                }).ToList(),
                Dependencies = q.Dependencies.Select(d => new StepToolDependencyDto
                {
                    StepToolOrder = d.DependsOnStepTool.Order,
                    StepOrder = d.DependsOnStepTool.Step!.Order
                }).ToList(),
                Step = new StepDto
                {
                    Name = q.Step!.Name,
                    Order = q.Step.Order,
                },
                Tool = new ToolDto
                {
                    Name = q.Tool.Name,
                    ToolType = q.Tool.ToolType!.Name,
                }
            }).FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves a step tool by its unique identifier, including its associated parameters.
        /// </summary>
        /// <remarks>The returned step tool includes its related parameters loaded from the data context.
        /// This method performs a query that may return <see langword="null"/> if no matching step tool
        /// exists.</remarks>
        /// <param name="id">The unique identifier of the step tool to retrieve.</param>
        /// <returns>A <see cref="StepTool"/> instance with its parameters if found; otherwise, <see langword="null"/>.</returns>
        public async Task<StepTool?> FindByIdWithParameters(int id)
        {
            return await _context.StepTools
                .Include(st => st.Parameters)
                .FirstOrDefaultAsync(st => st.Id == id);
        }

        /// <summary>
        /// Finds a step by its order and workflow ID.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public IQueryable<StepToolDto> FindByIds(ICollection<int> ids)
        {
            var query = _context.StepTools
            .Select(q => new StepToolDto
            {
                Id = q.Id,
                Name = q.Tool!.Name,
                StepId = q.StepId,
                ToolId = q.ToolId,
                Order = q.Order,
                PositionX = q.PositionX,
                PositionY = q.PositionY,
                DependsOnStepToolId = q.DependsOnStepToolId,
                Parameters = q.Parameters.Select(sp => new StepToolParameterDto
                {
                    Id = sp.Id,
                    Type = sp.StepTool!.Tool!.InputData!.Name,
                    Value = sp.Value,
                    RequiredFile = sp.RequiredFile,
                    WebhookId = sp.WebhookId
                }).ToList(),

            }).AsQueryable()
            .AsNoTracking();

            return query;
        }

        /// <summary>
        /// Creates a new step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> Create(StepTool stepTool)
        {
            _context.StepTools.Add(stepTool);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Creates a new step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> CreateRangeAsync(List<StepTool> stepTools)
        {
            await _context.StepTools.AddRangeAsync(stepTools);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates the specified StepTool entity in the data store asynchronously.
        /// </summary>
        /// <remarks>If the specified entity does not exist in the data store, no update is performed and
        /// the method returns <see langword="false"/>. This method does not throw an exception if the entity is not
        /// found.</remarks>
        /// <param name="stepTool">The StepTool entity to update. Must not be null and should have a valid primary key value corresponding to
        /// an existing entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the update
        /// was successful; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> Update(StepTool stepTool)
        {
            _context.StepTools.Update(stepTool);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Get all questions paged
        /// </summary>
        /// <param name="questionPagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<StepToolDto> FindAll()
        {
            var query = _context.StepTools
            .Select(q => new StepToolDto
            {
                Id = q.Id,
                Name = q.Tool!.Name,
                StepId = q.StepId,
                ToolId = q.ToolId,
                Order = q.Order,
                PositionX = q.PositionX,
                PositionY = q.PositionY,
                DependsOnStepToolId = q.DependsOnStepToolId,
                Parameters = q.Parameters.Select(sp => new StepToolParameterDto
                {
                    Id = sp.Id,
                    Type = sp.StepTool!.Tool!.InputData!.Name,
                    Value = sp.Value,
                    RequiredFile = sp.RequiredFile,
                    WebhookId = sp.WebhookId                    
                }).ToList(),

            })
            .AsQueryable()
            .AsNoTracking();

            return query;
        }

        /// <summary>
        /// Retrieves a list of <see cref="StepTool"/> objects associated with the specified step IDs.
        /// </summary>
        /// <remarks>This method performs a database query and uses no tracking to improve read
        /// performance. Ensure that the provided <paramref name="stepIds"/> collection is not null or empty to avoid
        /// unnecessary processing.</remarks>
        /// <param name="stepIds">A collection of step IDs for which to retrieve the associated <see cref="StepTool"/> objects. The collection
        /// must not be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="StepTool"/>
        /// objects associated with the specified step IDs, ordered by step ID and then by their defined order. If no
        /// matching step tools are found, an empty list is returned.</returns>
        public async Task<List<StepTool>> FindStepToolsByStepIdsAsync(IEnumerable<int> stepIds)
        {
            if (stepIds == null || !stepIds.Any())
                return new List<StepTool>();

            return await _context.StepTools
                .Include(st => st.Step)
                .AsNoTracking()
                .Where(st => stepIds.Contains(st.StepId))
                .OrderBy(st => st.StepId)
                .ThenBy(st => st.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves the first <see cref="StepTool"/> that depends on the specified step tool ID.
        /// </summary>
        /// <param name="id">The ID of the step tool to find dependents for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first  <see
        /// cref="StepTool"/> that depends on the specified ID, or <see langword="null"/> if no such  dependent exists.</returns>
        public async Task<StepTool?> FindDependentAsync(int id)
        {
            return await _context.StepTools.Include(u => u.Tool)
                                             .ThenInclude(t => t!.ToolType)
                                           .Include(s => s.Step)
                                           .Include(d => d.DependsOnStepTool)
                                           .Include(st => st.Dependencies)
                                           .FirstOrDefaultAsync(s => s.DependsOnStepToolId.Equals(id));
        }

        /// <summary>
        /// Retrieves a <see cref="StepTool"/> entity that matches the specified step ID and order.
        /// </summary>
        /// <remarks>This method performs an asynchronous query to locate a <see cref="StepTool"/> entity
        /// in the database that matches the given <paramref name="stepId"/> and <paramref name="order"/>.</remarks>
        /// <param name="stepId">The unique identifier of the step to search for.</param>
        /// <param name="order">The order value associated with the step tool.</param>
        /// <returns>A <see cref="StepTool"/> object if a matching entity is found; otherwise, <see langword="null"/>.</returns>
        public async Task<StepTool?> FindByStepIdAndOrderAsync(int stepId, int order)
        {
            return await _context.StepTools.Include(u => u.DependsOnStepTool)
                                           .Include(t => t.Tool)
                                            .ThenInclude(s => s!.ToolType)
                                           .Include(st => st.Dependencies)
                                           .FirstOrDefaultAsync(s => s.StepId == stepId && s.Order == order);
        }

        /// <summary>
        /// Finds the next pending tool for the specified step, ordered by execution sequence.
        /// </summary>
        /// <remarks>The returned StepTool includes related dependencies, executions, and tool
        /// information. Only tools with at least one pending execution are considered. This method is intended for
        /// scenarios where steps may have multiple tools and execution order matters.</remarks>
        /// <param name="stepId">The identifier of the step for which to locate the next pending tool.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the next pending StepTool for
        /// the step, or null if none are pending.</returns>
        public async Task<StepTool?> FindNextPending(int stepId, int cardId)
        {
            return await _context.StepTools
                .Include(st => st.DependsOnStepTool)
                .Include(st => st.Dependencies)
                .Include(st => st.Executions)
                .Include(st => st.Tool)
                    .ThenInclude(t => t!.ToolType)
                .Where(st => st.StepId == stepId && st.Executions.Any(e => e.CardId == cardId && e.Status == StatusExecution.Pending))
                .OrderBy(st => st.Order)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a collection of step tools associated with the specified step ID.
        /// </summary>
        /// <remarks>The returned collection is retrieved without tracking changes in the database
        /// context.</remarks>
        /// <param name="stepId">The unique identifier of the step for which to retrieve the associated tools.</param>
        /// <returns>A collection of <see cref="StepTool"/> objects associated with the specified step ID,  ordered by their
        /// defined order. Returns an empty collection if no tools are associated with the step.</returns>
        public ICollection<StepTool> FindStepToolsByStepId(int stepId)
        {
            return _context.StepTools
                .AsNoTracking()
                .Where(st => st.StepId == stepId)
                .OrderBy(st => st.Order)
                .ToList();
        }

        /// <summary>
        /// Delete by stepId
        /// </summary>
        /// <param name="stepId"></param>
        /// <returns></returns>
        public bool DeleteByStepId(int stepId)
        {
            var stepTools = _context.StepTools.Where(st => st.StepId == stepId);

            if (stepTools.Any())
            {
                _context.StepTools.RemoveRange(stepTools);
                _context.SaveChanges();
                return true;
            }

            return false;
        }
    }
}

