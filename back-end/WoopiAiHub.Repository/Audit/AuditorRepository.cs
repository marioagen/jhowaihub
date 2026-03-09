using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository.Audit
{
    /// <summary>
    /// Repository for auditor-related queries. Placeholder queries return everything (list) or filter by id (get by id).
    /// </summary>
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
        /// Returns all documents. Query can be refined later.
        /// </summary>
        public async Task<ICollection<DocumentDto>> GetDocumentsAsync()
        {
            return await _context.Documents
                .AsNoTracking()
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    ReferenceFile = d.ReferenceFile
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
