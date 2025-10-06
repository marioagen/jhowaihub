using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Interfaces.Repository;

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
        public bool CreateUniquePrompt(Prompt prompt)
        {
            var existPrompt = _context.Prompts.Any(p => p.Name == prompt.Name && p.EmailCreator == prompt.EmailCreator);
            if (!existPrompt)
            {
                _context.Prompts.Add(prompt);
                _context.SaveChanges();

                return true;
            }
            return false;
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
        public IQueryable<PromptDto> FindAllWithOwnerStatus(string emailCreator)
        {
            var query = _context.Prompts
                .AsNoTracking()
                .Select(p => new PromptDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text,
                    EmailCreator = p.EmailCreator,
                    Created = p.Created,
                    IsOwner = p.EmailCreator.Equals(emailCreator),
                    Variables = p.Variables.Select(v => new PromptVariableDto
                    {
                        Id = v.Id,
                        Variable = v.Variable,
                        Description = v.Description,
                        Label = v.Label,
                        Order = v.Order,
                    })
                    .ToList()
                });

            return query;
        }

        /// <summary>
        /// Find prompts by email of the creator
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public IQueryable<PromptDto> FindByEmail(string emailCreator)
        {
            var query = _context.Prompts
                .AsNoTracking()
                .Where(p => p.EmailCreator.Equals(emailCreator))
                .Select(p => new PromptDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text,
                    EmailCreator = p.EmailCreator,
                    Created = p.Created,
                    IsOwner = true,
                    Variables = p.Variables.Select(v => new PromptVariableDto
                    {
                        Id = v.Id,
                        Variable = v.Variable,
                        Description = v.Description,
                        Label = v.Label,
                        Order = v.Order,
                    })
                    .ToList()
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
                .Select(p => new PromptDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Text = p.Text,
                    EmailCreator = p.EmailCreator,
                    Created = p.Created,
                    Variables = p.Variables.Select(v => new PromptVariableDto
                    {
                        Id = v.Id,
                        Variable = v.Variable,
                        Description = v.Description,
                        Label = v.Label,
                        Order = v.Order,
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
            var existPrompt = _context.Prompts.Any(p => p.Name == prompt.Name && p.Id != prompt.Id && p.EmailCreator == prompt.EmailCreator);

            if (!existPrompt)
            {
                _context.Prompts.Update(prompt);
                _context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}
