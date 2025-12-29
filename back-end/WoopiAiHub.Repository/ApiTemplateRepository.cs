using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class ApiTemplateRepository(ApplicationDbContext context) : IApiTemplateRepository
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Creates a new template.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(ApiTemplate template)
        {
            _context.ApiTemplates.Add(template);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates an existing template.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(ApiTemplate template)
        {
            _context.ApiTemplates.Update(template);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Finds an template by its ID and returns his data transfer object.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiTemplateDto?> FindById(Guid id)
        {
            return await _context.ApiTemplates
                .Where(w => w.Id == id)
                .Select(item => new ApiTemplateDto
                {
                    Id = item.Id,
                    Created = item.Created,
                    Name = item.Name,
                    Method = item.Method,
                    Url = item.Url,
                    QueryTemplate = item.QueryTemplate,
                    HeaderTemplate = item.HeaderTemplate,
                    BodyTemplate = item.BodyTemplate
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Deletes a template by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteById(Guid id)
        {
            var template = await _context.ApiTemplates.FirstOrDefaultAsync(a => a.Id == id);

            _context.ApiTemplates.Remove(template!);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Retrieves a template entity by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiTemplate?> FindByIdReturnModel(Guid id)
        {
            return await _context.ApiTemplates
                 .FirstOrDefaultAsync(w => w.Id == id);
        }

        /// <summary>
        /// Finds all templates associated by the paged filter data transfer object.
        /// </summary>
        /// <param name="templateFilterDto"></param>
        /// <returns></returns>
        public async Task<ICollection<ApiTemplate>> FindAll(ApiTemplateFilterDto templateFilterDto)
        {
            var query = ApplyFilters(templateFilterDto.Input, templateFilterDto.Method, templateFilterDto.OrderBy);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Finds all templates associated by the paged filter data transfer object.
        /// </summary>
        /// <param name="templateFilterDto"></param>
        /// <returns></returns>
        public IQueryable<ApiTemplateDto> FindAllPaged(ApiTemplatePagedFilterDto templateFilterDto)
        {
            var query = ApplyFilters(templateFilterDto.Input, templateFilterDto.Method, templateFilterDto.OrderBy);

            return query.Select(w => new ApiTemplateDto
            {
                Id = w.Id,
                Created = w.Created,
                Name = w.Name,
                Method = w.Method,
                Url = w.Url,
                QueryTemplate = w.QueryTemplate,
                HeaderTemplate = w.HeaderTemplate,
                BodyTemplate = w.BodyTemplate
            });
        }

        /// <summary>
        /// Filters and orders the collection of API templates based on the specified input, HTTP method, and ordering
        /// criteria.
        /// </summary>
        /// <param name="input">The substring to search for within the template names. The search is case-insensitive. If null or empty, no
        /// name filtering is applied.</param>
        /// <param name="method">The HTTP method to filter by (for example, "get" or "post"). The comparison is case-insensitive. If null or
        /// empty, no method filtering is applied.</param>
        /// <param name="orderBy">The ordering criteria to apply to the results. Supported values are "created asc", "created desc", "name
        /// asc", and "name desc" (case-insensitive). If null, empty, or unrecognized, no ordering is applied.</param>
        /// <returns></returns>
        private IQueryable<ApiTemplate> ApplyFilters(string? input, string? method, string? orderBy)
        {
            input = input?.ToLower();
            method = method?.ToLower();
            orderBy = orderBy?.ToLower();

            var query = _context.ApiTemplates
                .AsNoTracking();

            if (!string.IsNullOrEmpty(input))
            {
                query = query.Where(i =>
                    EF.Functions.Like(i.Name, $"%{input}%"));
            }

            if (!string.IsNullOrEmpty(method))
            {
                query = query.Where(i => i.Method.ToLower().Equals(method));
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

            return query;
        }
    }
}
