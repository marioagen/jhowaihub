using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository.Audit
{
    public class AuditorRepository : IAuditorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRepository _userRepository;

        public AuditorRepository(
            ApplicationDbContext context,
            IWorkflowRepository workflowRepository,
            IUserRepository userRepository)
        {
            _context = context;
            _workflowRepository = workflowRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Returns the first N cards for the auditor (load-more pattern: take 10, then 20, 30…). One row per card with CardId, CardName, Workflows, ActionsCount, StatusName.
        /// Optional filters: search (matches CardId when numeric, or CardName/WorkflowName by contains), and statusId (exact match on StatusId).
        /// </summary>
        public async Task<ICollection<AuditorDocumentDto>> GetDocumentsAsync(int take, string? search, int? statusId)
        {
            const int defaultTake = 10;
            if (take <= 0) take = defaultTake;

            int? searchAsCardId = null;
            if (!string.IsNullOrWhiteSpace(search) && int.TryParse(search.Trim(), out var parsedId))
                searchAsCardId = parsedId;

            var query = _context.Cards.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search!.Trim();
                query = query.Where(c =>
                    (searchAsCardId != null && c.Id == searchAsCardId.Value)
                    || c.Name.Contains(searchTerm)
                    || (c.Step != null && c.Step.Workflow != null && c.Step.Workflow.Name.Contains(searchTerm)));
            }

            if (statusId.HasValue)
                query = query.Where(c => c.StatusId == statusId.Value);

            return await query
                .OrderBy(c => c.Id)
                .Take(take)
                .Select(c => new AuditorDocumentDto
                {
                    CardId = c.Id,
                    CardName = c.Name,
                    Workflows = c.Step != null && c.Step.Workflow != null
                        ? new List<AuditorWorkflowInfoDto> { new() { Id = c.Step.Workflow.Id, Name = c.Step.Workflow.Name } }
                        : new List<AuditorWorkflowInfoDto>(),
                    ActionsCount = _context.AuditCards.Count(a => a.CardId == c.Id),
                    StatusName = c.Status != null ? c.Status.Name : string.Empty
                })
                .ToListAsync();
        }

        /// <summary>
        /// Returns a single document by id.
        /// </summary>
        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    ReferenceFile = d.ReferenceFile
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns all workflows. Query can be refined later.
        /// </summary>
        public Task<ICollection<WorkflowDto>> GetWorkflowsAsync()
        {
            var list = _workflowRepository.FindAll();
            return Task.FromResult<ICollection<WorkflowDto>>(list);
        }

        /// <summary>
        /// Returns a single workflow by id.
        /// </summary>
        public Task<WorkflowDto?> GetWorkflowByIdAsync(int id)
        {
            return _workflowRepository.FindById(id, null);
        }

        /// <summary>
        /// Returns all users. Query can be refined later.
        /// </summary>
        public Task<ICollection<UserDto>> GetUsersAsync()
        {
            return _userRepository.FindAllAsync();
        }

        /// <summary>
        /// Returns a single user by id.
        /// </summary>
        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    Created = u.Created
                })
                .FirstOrDefaultAsync();
        }
    }
}
