using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace WoopiAiHub.Repository
{
    public class PromptRepository : IPromptRepository
    {
        private readonly ApplicationDbContext _context;

        public PromptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new prompt
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public bool Create(Prompt prompt)
        {
            _context.Prompts.Add(prompt);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// Create a new prompt and return the created prompt, if a prompt with the same name already exists for the user, it will return null
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public Prompt? CreateAndReturn(Prompt prompt)
        {
            _context.Prompts.Add(prompt);
            _context.SaveChanges();

            return prompt;
        }

        /// <summary>
        /// Find a prompt by name and user id, if not found it will return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="idUser"></param>
        /// <returns></returns>
        public Prompt? FindByNameAndUser(string name, Guid idUser)
        {
            return _context.Prompts.FirstOrDefault(p => p.Name == name && p.IdUser == idUser);
        }

        /// <summary>
        /// Create multiple prompts at once
        /// </summary>
        /// <param name="prompts"></param>
        /// <returns></returns>
        public bool CreateByRange(List<Prompt> prompts)
        {
            if (prompts == null || prompts.Count == 0)
            {
                return false;
            }

            _context.Prompts.AddRange(prompts);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete a prompt
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool Delete(List<int> ids)
        {
            var prompts = _context.Prompts.Where(a => ids.Contains(a.Id));

            if (prompts.Count() > 0)
            {
                _context.Prompts.RemoveRange(prompts);
                _context.SaveChanges();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Find all prompts
        /// </summary>
        /// <returns></returns>
        public IQueryable<PromptDto> FindAllWithOwnerStatus(Guid idUser)
        {
            var query = _context.Prompts
                .Select(p => new PromptDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text,
                    Created = p.Created,
                    IsOwner = p.IdUser.Equals(idUser),
                    IdUser = p.IdUser,
                    IsEdited = p.IsEdited,
                    IsImported = p.IsImported,
                    EnableAccessToMcp = p.EnableAccessToMcp,
                    OwnerName = p.User != null ? p.User.Name : string.Empty,
                    OwnerEmail = p.User != null ? p.User.Email : string.Empty
                }).AsNoTracking();

            return query;
        }

        /// <summary>
        /// Asynchronously retrieves all prompts in the basic format.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<PromptIntegrationDto>> FindAllInternal()
        {
            return await _context.Prompts
                .Select(p => new PromptIntegrationDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Find prompts by user id
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        public IQueryable<PromptDto> FindByIdUser(Guid idUser)
        {
            var query = _context.Prompts
                .AsNoTracking()
                .Where(p => p.IdUser.Equals(idUser))
                .Select(p => new PromptDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text,
                    Created = p.Created,
                    IsOwner = true,
                    IdUser = p.IdUser,
                    IsEdited = p.IsEdited,
                    IsImported = p.IsImported,
                    OwnerName = p.User != null ? p.User.Name : string.Empty,
                    OwnerEmail = p.User != null ? p.User.Email : string.Empty
                });

            return query;
        }

        /// <summary>
        /// Find a prompt by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PromptDto? FindById(int id)
        {
            return _context.Prompts
                .Include(x => x.PromptApiTemplates)
                .AsNoTracking()
                .Select(p => new PromptDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text,
                    Created = p.Created,
                    IdUser = p.IdUser,
                    IsEdited = p.IsEdited,
                    IsImported = p.IsImported,
                    EnableAccessToMcp = p.EnableAccessToMcp,
                    OwnerName = p.User != null ? p.User.Name : string.Empty,
                    OwnerEmail = p.User != null ? p.User.Email : string.Empty,
                    PromptApiTemplates = p.PromptApiTemplates.Select(promptApi => new PromptApiTemplateDto
                    {
                        ApiTemplateId = promptApi.ApiTemplateId,
                        PromptId = promptApi.PromptId,
                        Id = promptApi.Id
                    }).ToList()
                }).FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Update a prompt
        /// </summary>
        /// <param name="Prompt"></param>
        /// <returns></returns>
        public bool Update(Prompt prompt)
        {
            var existPrompt = _context.Prompts.Any(p => p.Id == prompt.Id);
            if (existPrompt)
            {
                _context.Prompts.Update(prompt);
                _context.SaveChanges();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds PromptApiTemplate records that match the given IDs.
        /// </summary>
        /// <param name="ids">List of PromptApiTemplate IDs to look up.</param>
        /// <returns>The matching PromptApiTemplate entities.</returns>
        public async Task<List<PromptApiTemplate>> FindPromptApiTemplatesByIds(List<int> ids)
        {
            return await _context.PromptApiTemplates
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Updates the prompt and removes the given PromptApiTemplate references in a single save.
        /// </summary>
        /// <param name="prompt">The prompt entity to update.</param>
        /// <param name="templatesToRemove">The already-validated PromptApiTemplate entities to remove.</param>
        public async Task UpdateAndRemovePromptApisFromPrompt(Prompt prompt, List<PromptApiTemplate> templatesToRemove)
        {
            _context.Prompts.Update(prompt);
            _context.PromptApiTemplates.RemoveRange(templatesToRemove);
            await _context.SaveChangesAsync();
        }
    }
}
