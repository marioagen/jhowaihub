using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of profiles that match the specified IDs.
        /// </summary>
        /// <param name="ids">A list of profile IDs to search for. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Profile"/>
        /// objects that match the specified IDs. If no matches are found, an empty list is returned.</returns>
        public async Task<List<Profile>> FindByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Set<Profile>()
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a paginated and optionally filtered list of profiles.
        /// </summary>
        /// <param name="pagedDataDto">An object containing pagination and filtering parameters, including the page number, page size, search term,
        /// and sort order.</param>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="ProfileDto"/> objects representing the profiles that match the
        /// specified search criteria and pagination settings.</returns>
        public IQueryable<ProfileDto> FindAllPaged(PagedDataDto pagedDataDto)
        {
            var query = _context.Profiles
                                .Select(p => new ProfileDto
                                {
                                    Id = p.Id,
                                    Name = p.Name
                                })
                                .AsQueryable();

            return query;
        }
    }
}