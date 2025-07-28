using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class ProfileServices : IProfileServices
    {
        private readonly IProfileRepository _profileRepository;
        public ProfileServices(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        /// <summary>
        /// Retrieves a paginated list of profiles based on the specified paging and sorting criteria.
        /// </summary>
        /// <param name="pagedDataDto">An object containing paging and sorting information, including the page number, page size,  and sorting
        /// direction.</param>
        /// <returns>A <see cref="ProfilePagedResultDto"/> containing the paginated list of profiles and metadata  about the
        /// paging operation.</returns>
        public ProfilePagedResultDto FindAllPaged(PagedDataDto pagedDataDto)
        {
            if (pagedDataDto.Page > 0)
            {
                var totalList = _profileRepository.FindAllPaged(pagedDataDto);

                totalList = pagedDataDto.IsAscending ?
                    totalList.OrderBy(p => p.Name) :
                    totalList.OrderByDescending(p => p.Name);

                var result = Pagination(totalList, pagedDataDto);
                return result;
            }
            else
            {
                var ex = new ArgumentException("The number of pages must be greater than 0");
                throw ex;
            }
        }

        /// <summary>
        /// Paginates a list of profiles based on the specified paging and search criteria.
        /// </summary>
        /// <param name="totalList">The complete list of profiles as an <see cref="IQueryable{T}"/> to be paginated.</param>
        /// <param name="pagedDataDto">The paging and search criteria, including the page number, page size, and optional search term.</param>
        /// <returns>A <see cref="ProfilePagedResultDto"/> containing the paginated list of profiles, the current page number,
        /// the total number of pages, and the total number of rows.</returns>
        private static ProfilePagedResultDto Pagination(IQueryable<ProfileDto> totalList,
                                                        PagedDataDto pagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(pagedDataDto.Search))
            {
                totalList = totalList.Where(i =>
                    i.Name.ToLower().Contains(pagedDataDto.Search.ToLower()) ||
                    i.Id.ToString().Contains(pagedDataDto.Search)
                );
            }

            var totalListCount = totalList.Count();

            if (pagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                pagedDataDto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / pagedDataDto.PageSize);
                currentPage = pagedDataDto.Page <= pageCount ? pagedDataDto.Page : 1;
                totalList = totalList.Skip((currentPage - 1) * pagedDataDto.PageSize)
                                     .Take(pagedDataDto.PageSize);
            }

            return new ProfilePagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount,
            };
        }
    }
}
