using Google.Api;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Linq;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StepRepository : IStepRepository
    {
        private readonly ApplicationDbContext _context;
        public StepRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> Create(Step step)
        {
            _context.Steps.Add(step);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task CreateRange(ICollection<Step> steps)
        {
            await _context.Steps.AddRangeAsync(steps);
        }

        /// <summary>
        /// Updates an existing step in the database.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task<bool> Update(Step step)
        {
            _context.Steps.Update(step);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Retrieves a step by its ID, including related profile, status, and cards.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Step?> FindById(int id)
        {
            return await _context.Steps
                           .Include(s => s.Profile)
                           .Include(s => s.Status)
                           .Include(s => s.Cards)
                           .Include(s => s.Workflow)
                           .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves all steps associated with a specific workflow ID.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ICollection<Step> FindByIds(IEnumerable<int> ids)
        {
            return _context.Steps.AsNoTracking().Where(t => ids.Contains(t.Id))
                .ToList();
        }

        /// <summary>
        /// Retrieves all steps with cards associated with ids list
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ICollection<Step> FindByIdsWithCards(IEnumerable<int> ids)
        {
            return _context.Steps.Where
                (t => ids.Contains(t.Id))
                .Include(t => t.Cards)
                .ToList();
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

            var deletedCount = _context.Steps
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
        public Task<Step?> FindByOrderAndWorkflowId(int order,
                                                    int workflowId)
        {
            return _context.Steps
                           .FirstOrDefaultAsync(s => s.Order == order &&
                                                     s.WorkflowId == workflowId);
        }

        /// <summary>
        /// Finds steps by workflow ID with optional filtering and ordering.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <param name="allUsers"></param>
        /// <param name="login"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<List<StepDto>> FindStepsByWorkflowId(int id, string input = "", bool allUsers = false, string login = "", string order = "")
        {
            var steps = await _context.Steps
                .Where(s => s.WorkflowId == id)
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
                    HasStepTools = s.StepTools.Count > 0,
                    //No longer requires Enabled
                    Cards = s.Cards
                         .Where(c =>
                             (
                                 string.IsNullOrWhiteSpace(input)
                                 || c.Name.Contains(input)
                                 || c.Document!.Name.Contains(input)
                                 || c.Document.Description.Contains(input)
                                 || c.Document.EmailCreator.Contains(input)
                             ) &&
                             (
                                 allUsers == true
                                 || (c.AssignedUser != null && c.AssignedUser.Email == login)
                             ) &&
                             c.Document!.Enable
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
                            ToolName = c.Step!.StepTools
                                              .Where(st => st.Executions.Any(e => e.CardId == c.Id && e.Status == StatusExecution.Running))
                                              .Select(st => st.Tool!.Name)
                                              .FirstOrDefault() ?? string.Empty,
                            AssignedUser = c.AssignedUser != null ?
                            new UserDto
                            {
                                Name = c.AssignedUser.Name,
                                Email = c.AssignedUser.Email,
                                Created = c.AssignedUser.Created,
                                Id = c.AssignedUser.Id
                            }
                            : null
                        }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            steps.ForEach(step => step.Cards = ApplyCardOrdering(step.Cards, order));

            return steps;
        }

        /// <summary>
        /// Applies ordering to a collection of CardDto based on the specified orderBy string.
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        private static ICollection<CardDto> ApplyCardOrdering(ICollection<CardDto> cards, string? orderBy)
        {
            return orderBy?.ToLower() switch
            {
                "created desc" => cards.OrderByDescending(c => c.Created).ToList(),
                "created asc" => cards.OrderBy(c => c.Created).ToList(),
                "name desc" => cards.OrderByDescending(c => c.Name).ToList(),
                "name asc" => cards.OrderBy(c => c.Name).ToList(),
                _ => cards
            };
        }
    }
}
