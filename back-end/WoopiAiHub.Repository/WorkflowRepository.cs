using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkflowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a workflow associated with a specific team ID.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<WorkflowDto?> FindByTeamId(int teamId)
        {
            return await _context.Workflows
                .Where(w => w.TeamId == teamId)
                .Select(w => new WorkflowDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    TeamId = w.TeamId,
                    Steps = w.Steps.Select(s => new StepDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Order = s.Order,
                        Profile = new ProfileDto
                        {
                            Id = s.Profile!.Id,
                            Name = s.Profile.Name
                        },
                        Status = new StatusDto
                        {
                            Id = s.Status!.Id,
                            Name = s.Status.Name
                        },
                        Cards = s.Cards.Select(c => new CardDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                        }).ToList(),
                    }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WorkflowDto?> FindById(int id)
        {
            return await _context.Workflows
                .Where(w => w.Id == id)
                .Select(GetWorkflowProjection())
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a workflow by its ID and includes related entities.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Workflow?> FindByIdReturnModel(int id)
        {
           return await _context.Workflows
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Profile)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Status)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Cards)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        /// <summary>
        /// Creates a projection for the Workflow entity to WorkflowDto.
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<Workflow, WorkflowDto>> GetWorkflowProjection()
        {
            return w => new WorkflowDto
            {
                Id = w.Id,
                Name = w.Name,
                TeamId = w.TeamId,
                Steps = w.Steps.Select(s => new StepDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Order = s.Order,
                    Profile = new ProfileDto
                    {
                        Id = s.Profile!.Id,
                        Name = s.Profile.Name
                    },
                    Status = new StatusDto
                    {
                        Id = s.Status!.Id,
                        Name = s.Status.Name
                    },
                    Cards = s.Cards.Select(c => new CardDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                    }).ToList(),
                }).ToList()
            };
        }
    }
}
