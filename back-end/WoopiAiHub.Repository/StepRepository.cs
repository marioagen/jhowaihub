using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
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
        /// Retrieves a step by its ID without tracking, including related profile, status, cards, and workflow.
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
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Asynchronously retrieves a step by its unique identifier, including its associated tools.
        /// </summary>
        /// <remarks>The returned step includes its related tools loaded from the database. This method
        /// performs a database query and may return null if no step with the specified identifier exists.</remarks>
        /// <param name="id">The unique identifier of the step to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the step with its associated
        /// tools if found; otherwise, null.</returns>
        public async Task<Step?> FindByIdWithTools(int id)
        {
            return await _context.Steps
                .AsNoTracking()
                .Include(s => s.StepTools)
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
                .Include(s => s.Profile)
                .AsNoTracking()
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
        public async Task<List<StepDto>> FindStepsByWorkflowId(
            int id,
            string input = "",
            bool allUsers = false,
            string login = "",
            string order = "",
            DocumentFilter documentFilter = DocumentFilter.All)
        {
            var steps = await _context.Steps
                .AsNoTracking()
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
                        Color = s.Status.Color
                    },
                    HasStepTools = s.StepTools.Any(),
                    Cards = s.Cards
                        .Where(c =>
                            (
                                string.IsNullOrWhiteSpace(input)
                                || c.Name.Contains(input)
                                || c.Document!.Name.Contains(input)
                                || c.Document.Description.Contains(input)
                                || c.Document.EmailCreator.Contains(input)
                            )
                            &&
                            (
                                allUsers
                                || (c.AssignedUser != null && c.AssignedUser.Email == login)
                            )
                            &&
                            (
                                documentFilter == DocumentFilter.All
                                || (documentFilter == DocumentFilter.Singles && c.DocumentBatchId == null)
                                || (documentFilter == DocumentFilter.Batches && c.DocumentBatchId != null)
                            )
                        )
                        .GroupBy(c => c.DocumentBatchId ?? -c.Id)
                        .Select(g =>
                            g.OrderBy(c => c.Id)
                             .Select(first => new CardDto
                             {
                                 Id = first.Id,
                                 Name = first.Name,
                                 Created = first.Created,
                                 Description = first.Document!.Description,
                                 Owner = first.Document.EmailCreator,
                                 DocumentId = first.Document.Id,
                                 StatusDocument = first.Document.Status,
                                 Percentage =
                                     s.StepTools.Any(st =>
                                         st.Executions.Any(e => g.Any(card => e.CardId == card.Id)))
                                     ?
                                     (
                                         s.StepTools.Count(st =>
                                             st.Executions.Any(e => e.Status == StatusExecution.Ready)
                                             && g.All(card =>
                                                 st.Executions.Any(e =>
                                                     e.CardId == card.Id &&
                                                     e.Status == StatusExecution.Ready)))
                                         * 100
                                         /
                                         s.StepTools.Count(st =>
                                             st.Executions.Any(e =>
                                                 g.Any(card => e.CardId == card.Id)))
                                     )
                                     : 100,
                                 ToolName =
                                     s.StepTools
                                         .Where(st =>
                                             st.Executions.Any(e =>
                                                 g.Any(card => e.CardId == card.Id)
                                                 && e.Status == StatusExecution.Running))
                                         .Select(st => st.Tool!.Name)
                                         .FirstOrDefault() ?? "",
                                 AssignedUser =
                                     first.AssignedUser == null
                                         ? null
                                         : new UserDto
                                         {
                                             Id = first.AssignedUser.Id,
                                             Name = first.AssignedUser.Name,
                                             Email = first.AssignedUser.Email,
                                             Created = first.AssignedUser.Created
                                         },
                                 Status = new StatusDto
                                 {
                                     Id = first.Status!.Id,
                                     Name = first.Status.Name,
                                     Color = first.Status.Color
                                 },
                                 IsBatchParent = first.DocumentBatchId != null
                             }).First()
                        ).ToList()
                }).ToListAsync();

            steps.ForEach(step =>
                step.Cards = ApplyCardOrdering(step.Cards, order));

            return steps;
        }

        /// <summary>
        /// Retrieves a list of steps in the specified workflow that precede the current step of the given card.
        /// </summary>
        /// <remarks>Use this method to determine the steps that a card has already passed through or
        /// could have passed through in a workflow. The returned steps are ordered according to their position in the
        /// workflow.</remarks>
        /// <param name="workflowId">The unique identifier of the workflow to search within.</param>
        /// <param name="order">order.</param>
        /// <returns>A list of <see cref="StepDto"/> objects representing the steps that occur before the current step of the
        /// specified card in the workflow. Returns an empty list if no previous steps are found.</returns>
        public async Task<List<StepDto>> FindPreviousStepsByWorkflowIdAndOrder(int workflowId, int order)
        {
            return await _context.Steps
                .Where(s => s.WorkflowId == workflowId && s.Order < order)
                .Select(s => new StepDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Order = s.Order,
                    WorkflowId = s.WorkflowId
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// finds the step associated with a specific card ID.
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<Step?> FindStepByCardId(int cardId)
        {
            return _context.Steps
                           .Include(s => s.Cards)
                           .FirstOrDefaultAsync(s => s.Cards.Any(c => c.Id == cardId));
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
