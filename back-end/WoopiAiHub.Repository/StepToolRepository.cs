using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
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
        public bool DeleteByIds(ICollection<int> ids)
        {
            var stepTools = _context.StepTools.Where(a => ids.Contains(a.Id));

            if (stepTools.Any())
            {
                _context.StepTools.RemoveRange(stepTools);
                _context.SaveChanges();
                return true;
            }

            return false;
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
                Name = q.Tool.Name,
                StepId = q.StepId,
                ToolId = q.ToolId,
                Order = q.Order,
                PositionX = q.PositionX,
                PositionY = q.PositionY,
                DependsOnStepToolId = q.DependsOnStepToolId,
                Parameters = q.Parameters.Select(sp => new StepToolParameterDto
                {
                    Id = sp.Id,
                    Type = sp.StepTool.Tool.InputData.Name,
                    Value = sp.Value
                }).ToList(),

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
                Name = q.Tool.Name,
                StepId = q.StepId,
                ToolId = q.ToolId,
                Order = q.Order,
                PositionX = q.PositionX,
                PositionY = q.PositionY,
                DependsOnStepToolId = q.DependsOnStepToolId,
                Parameters = q.Parameters.Select(sp => new StepToolParameterDto
                {
                    Id = sp.Id,
                    Type = sp.StepTool.Tool.InputData.Name,
                    Value = sp.Value
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
                Name = q.Tool.Name,
                StepId = q.StepId,
                ToolId = q.ToolId,
                Order = q.Order,
                PositionX = q.PositionX,
                PositionY = q.PositionY,
                DependsOnStepToolId = q.DependsOnStepToolId,
                Parameters = q.Parameters.Select(sp => new StepToolParameterDto
                {
                    Id = sp.Id,
                    Type = sp.StepTool.Tool.InputData.Name,
                    Value = sp.Value
                }).ToList(),

            })
            .AsQueryable()
            .AsNoTracking();

            return query;
        }

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

        public async Task<StepTool?> FindDependentAsync(int id)
        {
            return await _context.StepTools.FirstOrDefaultAsync(s => s.DependsOnStepToolId.Equals(id));
        }

        public async Task<StepTool?> FindByStepIdAndOrderAsync(int stepId, int order)
        {
            return await _context.StepTools.FirstOrDefaultAsync(s => s.StepId == stepId && s.Order == order);
        }

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

