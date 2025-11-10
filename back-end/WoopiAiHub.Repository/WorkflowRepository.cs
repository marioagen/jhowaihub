using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
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
        /// Updates a list of workflows.
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRange(ICollection<Workflow> workflows)
        {
            _context.Workflows.UpdateRange(workflows);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Retrieves a workflow associated with a specific team ID.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<WorkflowDto?> FindByTeamId(int teamId, WorkflowFilterDto? workflowFilterDto)
        {
            return await _context.Workflows
                .Include(w => w.Teams)
                .Include(w => w.Steps)
                .Where(s => s.Teams.Any(t => t.Id == teamId) && s.Enable.Equals(true))
                .Select(FindWorkflowProjection(workflowFilterDto?.Input, workflowFilterDto?.IsAllUsers, workflowFilterDto?.Login))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a list of workflows by its ID.
        /// </summary>
        /// <param name="ICollection<int>"></param>
        /// <returns></returns>
        public async Task<List<Workflow>> FindByIdsAsync(ICollection<int> ids)
        {
            return await _context.Workflows
                .Include(w => w.Steps)
                    .ThenInclude(s => s.StepTools)
                        .ThenInclude(t => t.Tool)
                            .ThenInclude(tt => tt.ToolType)
                .Include(w => w.Teams)
                .Where(w => ids.Contains(w.Id) && w.Enable.Equals(true))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a list of workflows by its team ids.
        /// </summary>
        /// <param name="List<int>"></param>
        /// <returns></returns>
        public async Task<List<WorkflowDto>> FindByUsersTeams(List<int> teamIds)
        {
            return await _context.Workflows
                .Include(w => w.Teams)
                .Where(s => s.Teams.Any(t => teamIds.Contains(t.Id)) && s.Enable.Equals(true))
                .Select(w => new WorkflowDto
                {
                    Id = w.Id,
                    Name = w.Name,
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a list of workflows by its team ids.
        /// </summary>
        /// <param name="List<int>"></param>
        /// <returns></returns>
        public async Task<ICollection<Workflow>> FindByStep(List<int> stepsIds)
        {
            return await _context.Workflows
                .Include(w => w.Steps)
                .Include(w => w.Teams)
                .Where(w => w.Steps.Any(s => stepsIds.Contains(s.Id)))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a list of workflows by its team ids.
        /// </summary>
        /// <param name="List<int>"></param>
        /// <returns></returns>
        public async Task<ICollection<Workflow>> FindByTeams(List<int> teamsIds)
        {
            return await _context.Workflows
                .Include(w => w.Teams)
                .Where(w => w.Teams.Any(s => teamsIds.Contains(s.Id)))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WorkflowDto?> FindById(int id, WorkflowFilterDto? workflowFilterDto)
        {
            return await _context.Workflows
                .Include(w => w.Teams)
                .Include(w => w.Steps)
                .Where(w => w.Id == id && w.Enable.Equals(true))
                .Select(FindWorkflowProjection(workflowFilterDto?.Input, workflowFilterDto?.IsAllUsers, workflowFilterDto?.Login))
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
            var workflows = _context.Workflows.Where(a => a.Id == id && a.Enable.Equals(true));

            if (await workflows.AnyAsync())
            {
                await workflows.ExecuteUpdateAsync(b => b.SetProperty(u => u.Enable, false));
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a workflow by its ID and includes related entities.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Workflow?> FindByIdReturnModel(int id)
        {
            return await _context.Workflows
                 .AsSplitQuery()
                 .Include(w => w.Teams)
                 .Include(w => w.Steps)
                     .ThenInclude(s => s.Profile)
                 .Include(w => w.Steps)
                     .ThenInclude(s => s.Status)
                 .Include(w => w.Steps)
                     .ThenInclude(s => s.Cards)
                 .Include(w => w.Steps)
                     .ThenInclude(s => s.StepTools)
                         .ThenInclude(p => p.Parameters)
                 .Include(w => w.Steps)
                     .ThenInclude(s => s.StepTools)
                         .ThenInclude(p => p.Outputs)
                 .Include(w => w.Steps)
                      .ThenInclude(s => s.StepTools)
                          .ThenInclude(st => st.Dependencies)
                 .FirstOrDefaultAsync(w => w.Id == id && w.Enable.Equals(true));
        }

        /// <summary>
        /// Creates a projection for the Workflow entity to WorkflowDto.
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<Workflow, WorkflowDto>> FindWorkflowProjection(String? input = null,
                Boolean? allUsers = true,
                String? login = null
            )
        {
            return w => new WorkflowDto
            {
                Id = w.Id,
                Name = w.Name,
                Created = w.Created,
                Teams = w.Teams.Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
                Steps = w.Steps.Select(s => new StepDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Order = s.Order,
                    WorkflowId = s.WorkflowId,
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
                    Cards = s.Cards
                        .Where(c => c.Enable &&
                            (
                                string.IsNullOrWhiteSpace(input)
                                || c.Name.Contains(input)
                                || c.Document.Name.Contains(input)
                                || c.Document.Description.Contains(input)
                            ) &&
                            (
                                allUsers == null
                                || allUsers == true
                                || (c.AssignedUser != null && c.AssignedUser.Email == login)
                            )
                        )
                        .Select(c => new CardDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Created = c.Created,
                            Description = c.Document!.Description,
                            Owner = c.Document.EmailCreator,
                            DocumentId = c.Document.Id,
                            StatusDocument = c.Document.Status,
                            Percentage = c.Step!.StepTools.Any(st => st.Executions.Any(e => e.CardId == c.Id))
                            ? (
                                c.Step.StepTools.Count(st => st.Executions.Any(e => e.Status == StatusExecution.Ready && e.CardId == c.Id)) * 100
                                /
                                c.Step.StepTools.Count(st => st.Executions.Any(e => e.CardId == c.Id))
                              )
                            : 100,
                            AssignedUser = c.AssignedUser != null ?
                            new UserDto
                            {
                                Name = c.AssignedUser.Name,
                                Email = c.AssignedUser.Email,
                                Created = c.AssignedUser.Created,
                                Id = c.AssignedUser.Id
                            }
                            : null
                    }).ToList(),                    
                    StepTools = s.StepTools
                        .Select(st => new StepToolDto
                        {
                            Id = st.Id,
                            ToolId = st.ToolId,
                            Order = st.Order,
                            PositionX = st.PositionX,
                            PositionY = st.PositionY,
                            DependsOnStepToolId = st.DependsOnStepToolId,
                            Parameters = st.Parameters.Select(p => new StepToolParameterDto
                            {
                                Id = p.Id,
                                Value = p.Value,
                                WebhookId = p.WebhookId,
                                RequiredFile = p.RequiredFile
                            }).ToList(),
                            Tool = new ToolDto
                            {
                                Id = st.Tool!.Id,
                                Name = st.Tool.Name,
                                IsEditableInput = st.Tool.IsEditableInput,
                                ToolType = st!.Tool!.ToolType!.Name
                            },
                            Executions = st.Executions.Select(e => new StepToolExecutionDto(
                                e.Id,
                                e.StepToolId,
                                e.CardId,
                                e.Started,
                                e.Completed,
                                e.Status,
                                null,
                                null
                            )).ToList(),
                            Outputs = st.Outputs.Select(o => new StepToolOutputDto(
                                o.Id,
                                o.StepToolId,
                                o.CardId,
                                o.Value,
                                null,
                                null
                            )).ToList(),
                            Dependencies = st.Dependencies.Select(d => new StepToolDependencyDto
                            {
                                StepToolOrder = d.DependsOnStepTool.Order,
                                StepOrder = d.DependsOnStepTool.Step!.Order
                            }).ToList(),
                        })
                        .ToList()
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
                           .Where(w => w.Teams.Any(t => t.Users.Any(u => u.Email == userEmail)) && w.Enable.Equals(true))
                           .Select(t => new WorkflowDto
                           {
                               Id = t.Id,
                               Name = t.Name,
                               Created = t.Created,
                               Teams = t.Teams.Select(t => new TeamDto
                               {
                                   Id = t.Id,
                                   Name = t.Name,
                               }).ToList(),
                               Steps = t.Steps.Select(t => new StepDto
                               {
                                   Id = t.Id,
                                   Name = t.Name,
                               }).ToList()
                           })
                           .ToList();
        }

        /// <summary>
        /// Finds all workflows.
        /// </summary>
        /// <returns></returns>
        public ICollection<WorkflowDto> FindAll()
        {
            return _context.Workflows
                           .AsNoTracking()
                           .Where(w => w.Enable.Equals(true))
                           .Select(t => new WorkflowDto
                           {
                               Id = t.Id,
                               Name = t.Name,
                               Created = t.Created,
                               Teams = t.Teams.Select(t => new TeamDto
                               {
                                   Id = t.Id,
                                   Name = t.Name,
                               }).ToList(),
                               Steps = t.Steps.Select(t => new StepDto
                               {
                                   Id = t.Id,
                                   Name = t.Name,
                               }).ToList()
                           })
                           .ToList();
        }

        /// <summary>
        /// Finds all workflows associated with a specific user team by their email address.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public IQueryable<WorkflowDto> FindAllWithFilter(WorkflowPagedDto workflowPagedDto)
        {
            var search = workflowPagedDto.Search?.ToLower();
            var login = workflowPagedDto.Login?.ToLower();
            var userTeamIds = _context.Users
                 .Where(u => u.Email.Equals(login))
                 .SelectMany(u => u.Teams.Select(t => t.Id))
                 .ToList();

            var query = _context.Workflows
                .Include(w => w.Teams)
                    .ThenInclude(t => t.Users)
                .AsNoTracking()
                .Where(w => w.Teams.Any(t => userTeamIds.Contains(t.Id)) && w.Enable.Equals(true));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                             EF.Functions.Like(i.Name, $"%{search}%"));
            }

            if (!workflowPagedDto.IsAllUsers)
            {
                query = query
                    .Include(w => w.Steps)
                        .ThenInclude(s => s.Cards)
                    .Where(w => w.Steps.Any(s => s.Cards.Any(c =>
                        c.AssignedUser != null &&
                        EF.Functions.Like(c.AssignedUser.Email, login)
                    )));
            }

            return query.Select(w => new WorkflowDto
            {
                Id = w.Id,
                Name = w.Name,
                Teams = w.Teams.Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
            });
        }
    }
}
