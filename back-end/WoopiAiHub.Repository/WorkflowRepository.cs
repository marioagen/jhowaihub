using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
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
        public async Task<WorkflowDto?> FindByTeamId(int teamId, WorkflowFilterDto? workflowFilterDto)
        {
            return await _context.Workflows
                .Where(w => w.TeamId == teamId)
                .Select(FindWorkflowProjection(workflowFilterDto?.Input, workflowFilterDto?.IsAllUsers, workflowFilterDto?.Login))
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
                .Select(FindWorkflowProjection())
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
                 .AsSplitQuery()
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
                 .FirstOrDefaultAsync(w => w.Id == id);
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
                TeamId = w.TeamId,
                Created = w.Created,
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
                    Cards = s.Cards
                    .Where(c => c.Enable &&
                            (
                                (string.IsNullOrWhiteSpace(input) || c.Name.Contains(input)) ||
                                (string.IsNullOrWhiteSpace(input) ||
                                (c.Document.Name.Contains(input) || c.Document.Description.Contains(input)))
                            ) &&
                            (
                                allUsers == false ||
                                (c.AssignedUser != null && c.AssignedUser.Email == login)
                            ))
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
                    WorkflowId = s.WorkflowId,
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
                                        ToolType = st.Tool.ToolType.Name,
                                        IsConnector = st.Tool.ToolType!.Name.Contains(ConnectorNames.N8N)
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
                                    )).ToList()
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
