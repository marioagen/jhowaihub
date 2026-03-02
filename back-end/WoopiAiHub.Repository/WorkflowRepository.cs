using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
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
                .Where(w => w.Enable && w.Steps.Any(s => stepsIds.Contains(s.Id)))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a step by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StepDto FindStepById(int id)
        {
            var step = _context.Steps
            .AsNoTracking()
            .Where(s => s.Id == id && s.Workflow.Enable)
            .Select(s => new StepDto
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
                StepTools = s.StepTools
            .OrderBy(st => st.Order)
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
                    ToolType = st.Tool!.ToolType!.Name
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
            })
            .FirstOrDefault();
            return step;
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
                .Where(w => w.Enable && w.Teams.Any(s => teamsIds.Contains(s.Id)))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a list of workflows by documentId and user .
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ICollection<ResponseWorkflowByDocumentDto>> FindWorkflowsByDocument(RequestWorkFlowByDocumentDto dto, CancellationToken ct = default)
        {
            var search = dto.Search?.ToLower();
            var login = dto.Login?.ToLower();
            var query = _context.Workflows
                    .Include(w => w.Documents)
                    .Include(w => w.Steps.Where(s => s.Cards.Any(c => c.DocumentId == dto.DocumentId)).OrderBy(s => s.Order))
                        .ThenInclude(s => s.Cards.Where(c => c.DocumentId == dto.DocumentId).OrderBy(c => c.Id))
                        .ThenInclude(c => c.AssignedUser)
                    .Include(w => w.Steps.Where(s => s.Cards.Any(c => c.DocumentId == dto.DocumentId)).OrderBy(s => s.Order))
                        .ThenInclude(s => s.Cards.Where(c => c.DocumentId == dto.DocumentId).OrderBy(c => c.Id))
                        .ThenInclude(c => c.Status)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => EF.Functions.Like(i.Name, $"%{search}%"));
            }

            if (!string.IsNullOrEmpty(login))
            {
                query = query.Where(d =>
                    d.Teams.Any(c =>
                        c.Users.Any(c =>
                            EF.Functions.Like(c.Email, login)
                        )
                    )
                );
            }

            var result = await query.Where(w => w.Documents.Any(s => s.Id == dto.DocumentId)).ToListAsync(ct);

            var resultDto = result
                .SelectMany(workflow =>
                    workflow.Steps.SelectMany(step =>
                        step.Cards.Select(card => new { Workflow = workflow, Card = card })
                    )
                )
                .Select(x => new ResponseWorkflowByDocumentDto
                {
                    Id = x.Workflow.Id,
                    Name = x.Workflow.Name,
                    CardId = x.Card.Id,
                    DocumentId = x.Card.DocumentId,
                    AssignedUserEmail = x.Card.AssignedUser?.Email ?? string.Empty,
                    StatusName = x.Card.Status?.Name ?? string.Empty
                })
                .ToList();

            return resultDto;
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
        /// Retrieves a workflow by its ID and includes only the necessary related entities.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Workflow?> FindByIdForFlow(int id)
        {
            return await _context.Workflows
                 .AsSplitQuery()
                 .Include(w => w.Steps)
                     .ThenInclude(s => s.StepTools)
                         .ThenInclude(p => p.Parameters)
                 .Include(w => w.Steps)
                      .ThenInclude(s => s.StepTools)
                          .ThenInclude(st => st.Dependencies)
                 .FirstOrDefaultAsync(w => w.Id == id && w.Enable.Equals(true));
        }

        /// <summary>
        /// Retrieves a workflow with the specified identifier for analysis, including its associated steps.
        /// </summary>
        /// <remarks>The returned workflow includes its related steps for comprehensive analysis. Only
        /// workflows that are enabled are considered.</remarks>
        /// <param name="id">The unique identifier of the workflow to retrieve. Must be a positive integer.</param>
        /// <returns>A <see cref="Workflow"/> object representing the workflow and its steps if a matching, enabled workflow is
        /// found; otherwise, <see langword="null"/>.</returns>
        public async Task<Workflow?> FindByIdForAnalyze(int id)
        {
            return await _context.Workflows
                                 .AsSplitQuery()
                                 .Include(w => w.Steps)
                                 .FirstOrDefaultAsync(w => w.Id == id &&
                                                           w.Enable.Equals(true));
        }

        /// <summary>
        /// Retrieves a workflow by ID with all data needed for cloning.
        /// Includes teams, steps (with profile/status), step tools, parameters and dependencies.
        /// Excludes documents, cards, executions and outputs.
        /// </summary>
        public async Task<Workflow?> FindByIdForClone(int id)
        {
            return await _context.Workflows
                .AsNoTracking()
                .AsSplitQuery()
                .Include(w => w.Teams)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Profile)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Status)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.StepTools)
                        .ThenInclude(st => st.Parameters)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.StepTools)
                        .ThenInclude(st => st.Dependencies)
                .FirstOrDefaultAsync(w => w.Id == id && w.Enable);
        }


        /// <summary>
        /// Update output of step in a workflow.
        /// </summary>
        /// <param name="stepToolOutput"></param>
        /// <returns></returns>
        public async Task<bool> UpdateStepToolOutput(StepToolOutput stepToolOutput)
        {
            _context.StepToolOutputs.Update(stepToolOutput);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Find phase 1 data by workflow id
        /// Optimized query that only selects necessary fields (name and teams with id and name).
        /// </summary>
        /// <param name="id">Workflow id</param>
        /// <returns>Phase1Dto containing workflow name and associated teams</returns>
        public async Task<Phase1Dto> FindPhase1ById(int id)
        {
            var workflow = await _context.Workflows
                .Where(w => w.Id == id && w.Enable)
                .Select(w => new Phase1Dto
                {
                    Name = w.Name,
                    Teams = w.Teams.Select(t => new TeamDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                    }).ToList(),
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return workflow!;
        }

        /// <summary>
        /// Find phase2 data by workflow id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<StepDto>> FindPhase2ById(int id)
        {
            var steps = await _context.Workflows
                .AsNoTracking()
                .Where(w => w.Id == id && w.Enable)
                .SelectMany(w => w.Steps)
                .Select(s => new StepDto
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
                    HasStepTools = s.StepTools.Any()
                })
                .ToListAsync();

            return steps;
        }


        /// <summary>
        /// Find phase3 data by workflow id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<StepDto>> FindPhase3ById(int id)
        {
            var steps = await _context.Workflows
                .AsNoTracking()
                .Where(w => w.Id == id && w.Enable)
                .SelectMany(w => w.Steps)
                .Select(s => new StepDto
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
                    HasStepTools = s.StepTools.Any(),
                    StepTools = s.StepTools
                        .OrderBy(st => st.Order)
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
                            Dependencies = st.Dependencies.Select(d => new StepToolDependencyDto
                            {
                                StepToolOrder = d.DependsOnStepTool.Order,
                                StepOrder = d.DependsOnStepTool.Step!.Order
                            }).ToList(),
                        })
                        .ToList()
                })
                .ToListAsync();

            return steps;
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
                    HasStepTools = s.StepTools.Any(),
                    StepTools = s.StepTools
                        .OrderBy(st => st.Order)
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
            var orderBy = workflowPagedDto.OrderBy?.ToLower();

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

            if (workflowPagedDto.TeamId.HasValue)
            {
                query = query.Where(w => w.Teams.Any(t => t.Id == workflowPagedDto.TeamId.Value));
            }

            if (workflowPagedDto.UserId.HasValue)
            {
                query = query.Where(w => w.Teams.Any(t => t.Users.Any(u => u.Id == workflowPagedDto.UserId.Value)));
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

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                if (orderBy == "created desc")
                {
                    query = query.OrderByDescending(w => w.Created);
                }
                else if (orderBy == "created asc")
                {
                    query = query.OrderBy(w => w.Created);
                }
                if (orderBy == "name desc")
                {
                    query = query.OrderByDescending(w => w.Name);
                }
                else if (orderBy == "name asc")
                {
                    query = query.OrderBy(w => w.Name);
                }
            }

            return query.Select(w => new WorkflowDto
            {
                Id = w.Id,
                Name = w.Name,
                Teams = w.Teams
                    .Select(t => new TeamDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                    })
                    .OrderBy(t => t.Name)
                    .ToList(),
            });
        }

        /// <summary>
        /// Finds a StepToolOutput by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StepToolOutput FindByStepToolOutputById(int id)
        {
            var stepToolOutput = _context.StepToolOutputs.Where(p => p.Id == id)
                                                         .FirstOrDefault();
            return stepToolOutput;
        }

        /// <summary>
        /// Finds a Tool by its StepTool ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ToolDto> FindToolByStepToolId(int id)
        {
            return await _context.StepTools.Where(p => p.Id == id)
                                            .Select(s => new ToolDto
                                            {
                                                Id = s.Tool.Id,
                                                Name = s.Tool.Name,
                                            }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Determines whether the specified user is a member of any team associated with the workflows of the document
        /// linked to the given card.
        /// </summary>
        /// <remarks>The method returns <see langword="false"/> if the card does not exist, is not linked
        /// to a document, or if the user is not found in any associated team.</remarks>
        /// <param name="cardId">The unique identifier of the card whose associated document's teams are to be checked.</param>
        /// <param name="userId">The unique identifier of the user to validate as a team member.</param>
        /// <returns><see langword="true"/> if the user is a member of at least one team associated with the workflows of the
        /// document linked to the specified card; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> IsValidTeamUser(int cardId,
                                                Guid userId)
        {
            var isValidTeamUser = await _context.Cards
                                                .Where(c => c.Id == cardId)
                                                .SelectMany(c => c.Document!.Workflows)
                                                .SelectMany(w => w.Teams)
                                                .SelectMany(t => t.Users)
                                                .AnyAsync(u => u.Id == userId);

            return isValidTeamUser;
        }

        /// <summary>
        /// Retrieves all enabled workflows as internal data transfer objects.
        /// </summary>
        /// <returns>A collection of <see cref="WorkflowInternalDto"/> objects representing all workflows that are currently
        /// enabled. The collection is empty if no enabled workflows are found.</returns>
        public ICollection<WorkflowInternalDto> FindAllInternal()
        {
            return _context.Workflows
                            .AsNoTracking()
                            .Where(w => w.Enable.Equals(true))
                            .Select(t => new WorkflowInternalDto
                            {
                                Id = t.Id,
                                Name = t.Name,
                                Created = t.Created,
                            })
                            .ToList();
        }

        /// <summary>
        /// Retrieves a workflow by its ID, including its associated steps.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Workflow?> FindByIdReturnModelWithSteps(int id)
        {
            return await _context.Workflows
                .Include(w => w.Steps)
                    .ThenInclude(s => s.StepTools)
                        .ThenInclude(t => t.Tool)
                            .ThenInclude(tt => tt.ToolType)
                .Include(w => w.Teams)
                .AsNoTracking()
                .FirstAsync(w => w.Id == id && w.Enable.Equals(true));
        }
    }
}
