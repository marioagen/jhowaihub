using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
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
                    Id = d.Id,
                    StepToolId = d.StepToolId,
                    DependsOnStepToolId = d.DependsOnStepToolId,
                    DependsOnStepToolName = d.DependsOnStepTool.Tool!.Name,
                    DependsOnStepOrder = d.DependsOnStepTool.Step!.Order
                }).ToList(),
                Step = new StepDto
                {
                    Name = q.Step!.Name,
                    Order = q.Step.Order,
                }

            }).FirstOrDefaultAsync(s => s.Id == id);
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
        /// Updates an existing step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> Update(StepToolDto stepToolDto)
        {
            var stepTool = await _context.StepTools
                .Include(st => st.Parameters)
                .FirstOrDefaultAsync(st => st.Id == stepToolDto.Id);
            if (stepTool == null)
            {
                return false;
            }

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
                                           .FirstOrDefaultAsync(s => s.StepId == stepId && s.Order == order);
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

        /// <summary>
        /// Finds all StepTools that were executed before the specified StepTool, 
        /// including those from previous steps and earlier in the same step.
        /// </summary>
        /// <param name="stepToolId">The ID of the StepTool to find previous tools for.</param>
        /// <returns>A list of StepToolDto objects representing previous StepTools.</returns>
        public async Task<List<StepToolDto>> FindPreviousStepToolsAsync(int stepToolId)
        {
            var currentStepTool = await _context.StepTools
                .Include(st => st.Step)
                .FirstOrDefaultAsync(st => st.Id == stepToolId);

            if (currentStepTool == null)
                return new List<StepToolDto>();

            var currentStepId = currentStepTool.StepId;
            var currentStepOrder = currentStepTool.Step!.Order;
            var currentToolOrder = currentStepTool.Order;
            var workflowId = currentStepTool.Step.WorkflowId;

            var previousStepTools = await _context.StepTools
                .Include(st => st.Step)
                .Include(st => st.Tool)
                .Where(st => st.Step!.WorkflowId == workflowId &&
                            (st.Step.Order < currentStepOrder ||
                             (st.Step.Order == currentStepOrder && st.Order < currentToolOrder)))
                .OrderBy(st => st.Step!.Order)
                .ThenBy(st => st.Order)
                .Select(q => new StepToolDto
                {
                    Id = q.Id,
                    Name = q.Tool!.Name,
                    StepId = q.StepId,
                    ToolId = q.ToolId,
                    Order = q.Order,
                    PositionX = q.PositionX,
                    PositionY = q.PositionY,
                    Step = new StepDto
                    {
                        Name = q.Step!.Name,
                        Order = q.Step.Order,
                    }
                })
                .ToListAsync();

            return previousStepTools;
        }
    }
}

