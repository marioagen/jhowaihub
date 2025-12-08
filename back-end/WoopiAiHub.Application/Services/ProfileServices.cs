using AutoMapper;
using System.Linq;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class ProfileServices : IProfileServices
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IWorkflowServices _workflowServices;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IStepProfilePermissionsServices _stepProfilePermissionsServices;

        public ProfileServices(IProfileRepository profileRepository,
                               IPermissionRepository permissionRepository,
                               IWorkflowServices workflowServices,
                               IStepProfilePermissionsServices stepProfilePermissionsServices)
        {
            _profileRepository = profileRepository;
            _permissionRepository = permissionRepository;
            _workflowServices = workflowServices;
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

            var profile = new Domain.Models.Profile(profileCreateDto.Name, 0, DateTime.Now)
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

            if (profileCreateDto.PermissionsWorkflow.Count() > 0)
            {
                await _stepProfilePermissionsServices.Create(profile.Id, profileCreateDto.PermissionsWorkflow);
                await _workflowServices.CreateWorkflowRelationship(profile, profileCreateDto.PermissionsWorkflow.Select(x => x.StepId).ToList());
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

            var oldStepsIds = (profile.StepProfilePermissions ?? Enumerable.Empty<StepProfilePermission>())
                .Select(spp => spp.StepId)
                .ToList();

            var newStepsIds = profileUpdateDto.PermissionsWorkflow?
                .Select(x => x.StepId)
                .ToList()
                ?? new List<int>();

            var addedStepIds = newStepsIds.Except(oldStepsIds).ToList();
            var removedStepIds = oldStepsIds.Except(newStepsIds).ToList();

            var profileId = profile.Id;
            if (addedStepIds.Count() > 0)
            {
                var newPermissionsOnly = profileUpdateDto.PermissionsWorkflow
                    .Where(x => addedStepIds.Contains(x.StepId))
                    .ToList();
                await _stepProfilePermissionsServices.Create(profileId, newPermissionsOnly);
                await _workflowServices.CreateWorkflowRelationship(profile, addedStepIds);
            }

            if(removedStepIds.Count() > 0)
            {
                var toRemovePermissions = profile.StepProfilePermissions
                    .Where(x => removedStepIds.Contains(x.StepId))
                    .ToList();
                await _stepProfilePermissionsServices.DeleteRow(toRemovePermissions);
                await _workflowServices.UpdateTeamProfileRelationshipToWorkflow(removedStepIds, profile);
            }

            return updateResult;
        }

        /// <summary>
        /// Deletes a list of profiles by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeleteByIds(List<int> ids)
        {
            await _stepProfilePermissionsServices.DeleteByIds(ids);
            return await _profileRepository.DeleteByIdsAsync(ids);
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
