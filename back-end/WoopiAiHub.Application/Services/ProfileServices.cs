using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class ProfileServices : IProfileServices
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IStepProfilePermissionsServices _stepProfilePermissionsServices;

        public ProfileServices(IProfileRepository profileRepository,
                               IPermissionRepository permissionRepository,
                               IStepProfilePermissionsServices stepProfilePermissionsServices)
        {
            _profileRepository = profileRepository;
            _permissionRepository = permissionRepository;
            _stepProfilePermissionsServices = stepProfilePermissionsServices;
        }

        /// <summary>
        /// Retrieves a profile by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ProfileDto> FindById(int id)
        {
            var profile = await _profileRepository.FindById(id);
            if (profile == null)
            {
                throw new ArgumentException("Profile not found");
            }
            return profile;
        }

        /// <summary>
        /// Retrieves all profiles paged.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public ProfilePagedResultDto FindAllPaged(PagedDataDto pagedDataDto)
        {
            if (pagedDataDto.Page <= 0)
            {
                var ex = new ArgumentException("The number of pages must be greater than 0");
                throw ex;
            }

            var totalList = _profileRepository.FindAllPaged(pagedDataDto);

                totalList = pagedDataDto.IsAscending ?
                    totalList.OrderBy(profile => profile.Name) :
                    totalList.OrderByDescending(profile => profile.Name);

                var result = Pagination(totalList, pagedDataDto);
                return result;
        }

        /// <summary>
        /// Creates a new profile with a unique name.
        /// </summary>
        /// <param name="profileCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> CreateUniqueProfile(ProfileCreateDto profileCreateDto)
        {
            if (string.IsNullOrEmpty(profileCreateDto.Name))
            {
                throw new ArgumentException("Profile name cannot be empty");
            }

            var profile = new Profile(profileCreateDto.Name, 0, DateTime.Now)
            {
                Permissions = new List<Permission>()
            };

            if (profileCreateDto.PermissionsIds != null)
            {
                var permissions = await _permissionRepository.FindByIdsAsync(profileCreateDto.PermissionsIds);

               foreach (var permission in permissions)
               {
                    profile.AddPermission(permission);
               }
            }

            var createResult = _profileRepository.CreateUniqueProfile(profile);
            if (!createResult)
            {
                throw new InvalidOperationException("Duplicated Profile");
            }

            if (profileCreateDto.PermissionsWorkflow != null)
            {
                await _stepProfilePermissionsServices.Create(profile.Id, profileCreateDto.PermissionsWorkflow);
            }

            return createResult;
        }

        /// <summary>
        /// Updates an existing profile based on the provided DTO.
        /// </summary>
        /// <param name="profileUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> Update(ProfileUpdateDto profileUpdateDto)
        {

            var profile = _profileRepository.FindByIdReturnModel(profileUpdateDto.Id);
            if (profile == null)
                return false;

            profile.Update(profileUpdateDto.Name);

            if (profileUpdateDto.PermissionsIds != null)
            {
                profile.Permissions.Clear();
                var permissions = await _permissionRepository.FindByIdsAsync(profileUpdateDto.PermissionsIds);

                foreach (var permission in permissions)
                {
                    profile.AddPermission(permission);
                }
            }

            var updateResult = _profileRepository.Update(profile);
            if (!updateResult)
            {
                throw new InvalidOperationException("Duplicated Profile");
            }

            await _stepProfilePermissionsServices.Delete(profile.Id);
            if (profileUpdateDto.PermissionsWorkflow != null)
            {
                await _stepProfilePermissionsServices.Create(profile.Id, profileUpdateDto.PermissionsWorkflow);
            }

            return updateResult;
        }

        /// <summary>
        /// Deletes a list of profiles by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            return _profileRepository.DeleteByIds(ids);
        }

        /// <summary>
        /// Retrieves all profiles.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ProfileDto>> FindAll()
        {
            return await _profileRepository.FindAll();
        }

        /// <summary>
        /// Ordenates the list of profiles and returns a paged result.
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        private static ProfilePagedResultDto Pagination(IQueryable<ProfileDto> totalList,
                                                        PagedDataDto pagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(pagedDataDto.Search))
            {
                totalList = totalList.Where(i => i.Name.ToLower()
                                                        .Contains(pagedDataDto.Search.ToLower()) ||
                                                 i.Id.ToString().Contains(pagedDataDto.Search));
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
