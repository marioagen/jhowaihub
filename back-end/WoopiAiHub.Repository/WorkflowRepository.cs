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
        /// Creates a new workflow.
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        public async Task<bool> Create(Workflow workflow)
        {
            _context.Workflows.Add(workflow);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates an existing workflow.
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        public async Task<bool> Update(Workflow workflow)
        {
            _context.Workflows.Update(workflow);
            return await _context.SaveChangesAsync() > 0;
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
                .Select(GetWorkflowProjection())
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
        /// Deletes a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteById(int id)
        {
            return await _context.Workflows
                .Where(w => w.Id == id)
                .ExecuteDeleteAsync() > 0;
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
                        Name = s.Status.Name,
                        Color = s.Status.Color,
                    },
                    Cards = s.Cards.Select(c => new CardDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Created = c.Created,
                        Description = c.Document.Description,
                        Owner = c.Document.EmailCreator,
                        DocumentId = c.Document.Id,
                        StatusDocument = c.Document.Status,
                    }).ToList(),
                    WorkflowId = s.WorkflowId
                }).ToList()
            };
        }

        /// <summary>
        /// Finds all workflows associated with a specific user by their email address.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public ICollection<WorkflowDto> FindAllByUser(string userEmail)
        {
            return _context.Workflows
                           .AsNoTracking()
                           .Where(w => w.Team.Users.Any(u => u.Email == userEmail))
                           .Select(t => new WorkflowDto
                           {
                               Id = t.Id,
                               Name = t.Name,
                               Created = t.Created,
                               Team = new TeamDto
                               {
                                   Id = t.Team.Id,
                                   Name = t.Team.Name,
                               },
                           })
                           .ToList();
        }
    }
}
