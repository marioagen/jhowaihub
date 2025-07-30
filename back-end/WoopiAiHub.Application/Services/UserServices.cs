using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly ITeamRepository _teamRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserServices(IUserRepository userRepository,
                            IMarketPlaceApi marketPlaceApi,
                            IConfiguration config,
                            ITeamRepository teamRepository,
                            IProfileRepository profileRepository,
                            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _teamRepository = teamRepository;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _profileRepository = profileRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Create an user by dto
        /// </summary>
        /// <param name="userCreateDto"></param>
        /// <returns></returns>
        public async Task<bool> Create(UserCreateDto userCreateDto, HeadersDto headersDto)
        {
            if (string.IsNullOrEmpty(userCreateDto.Name) || 
                string.IsNullOrEmpty(userCreateDto.Email) ||
                string.IsNullOrEmpty(userCreateDto.Password))
            {
                throw new ArgumentException("Data cannot be empty");
            }
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var requestAssignLicensesByHub = new RequestAssignLicensesByHub
            {
                UserEmail = userCreateDto.Email,
                Tenant = headersDto.Tenant,
            };
            var userEnabledReference = await _marketPlaceApi.AssignLicensesByHub(KeyAccess, requestAssignLicensesByHub);
            if (userEnabledReference == Guid.Empty)
                return false;

            var existingUser = await _userRepository.FindByReferenceAsync(userEnabledReference);

            if (existingUser != null)
            {
                existingUser.Reactivate(userCreateDto.Name,
                                        userCreateDto.Email);

                var hashedPassword = _passwordHasher.Hash(userCreateDto.Password, existingUser.Salt);
                existingUser.SetPassword(hashedPassword, existingUser.Salt);

                _userRepository.Update(existingUser);

                return true;
            }
            else
            {
                User user = new User(
                      userEnabledReference,
                      userCreateDto.Name,
                      userCreateDto.Email,
                      true,
                      DateTime.Now
                );

                var salt = _passwordHasher.GenerateSalt();
                var hashedPassword = _passwordHasher.Hash(userCreateDto.Password, salt);
                user.SetPassword(hashedPassword, salt);

                if (userCreateDto.TeamIds.Count > 0)
                {
                    var teams = _teamRepository.FindByIds(userCreateDto.TeamIds);

                    foreach (var team in teams)
                    {
                        user.AddTeam(team);
                    }
                }

                if (userCreateDto.ProfileIds.Count > 0)
                {
                    var profiles = _profileRepository.FindByIds(userCreateDto.ProfileIds);

                    foreach (var profile in profiles)
                    {
                        user.AddProfile(profile);
                    }
                }

                return await _userRepository.CreateAsync(user);
            }
        }

        // <summary>
        /// Delete users by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeactivateRange(List<Guid> ids)
        {
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var users = await _userRepository.FindByIdsAsync(ids);
            var allExists = ids.All(id => users.Any(u => u.Id == id));

            if (allExists)
            {
                var requestDto = new DeactivateUsersDto { ReferenceUsers = ids };
                var mktDeactivate = await _marketPlaceApi.DeactivateUsersEnabledByReference(KeyAccess, requestDto);
                if (mktDeactivate)
                {
                    var result = _userRepository.DeactivateRange(ids);
                    return result;
                }
            }

            return false;
        }

        /// <summary>
        /// Update user by dto
        /// </summary>
        /// <param name="userUpdateDto"></param>
        /// <returns></returns>
        public async Task<bool> Update(UserUpdateDto userUpdateDto, HeadersDto headersDto)
        {
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var requestAssignLicensesByHub = new RequestAssignLicensesByHub
            {
                UserEmail = userUpdateDto.Email,
                Tenant = headersDto.Tenant,
                IdUser = userUpdateDto.Id
            };

            var updateMkt = await _marketPlaceApi.AssignLicensesByHub(KeyAccess, requestAssignLicensesByHub);
            if (updateMkt != Guid.Empty)
            {

                var user = await _userRepository.FindByReferenceAsync(userUpdateDto.Id);
                if (user == null)
                    return false;

                user.Update(userUpdateDto.Name,
                            userUpdateDto.Email);

                if (!string.IsNullOrEmpty(userUpdateDto.Password))
                {
                    var saltBytes = user.Salt;
                    if (saltBytes == null || saltBytes.Length == 0)
                    {
                        saltBytes = _passwordHasher.GenerateSalt();
                    }
                    var hashedPassword = _passwordHasher.Hash(userUpdateDto.Password, saltBytes);
                    user.SetPassword(hashedPassword, saltBytes);
                }

                AddTeams(userUpdateDto, user);

                AddProfiles(userUpdateDto, user);

                var updateResult = _userRepository.Update(user);
                if (!updateResult)
                {
                    throw new ArgumentException("Duplicated User");
                }
                return updateResult;
            }

            return false;
        }

        /// <summary>
        /// Retrieves all usrs paged
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public UserPagedResultDto FindAllPaged(PagedDataDto pagedDataDto)
        {
            if (pagedDataDto.Page > 0)
            {
                var totalList = _userRepository.FindAllPaged(pagedDataDto);

                totalList = pagedDataDto.IsAscending ?
                    totalList.OrderBy(user => user.Name) :
                    totalList.OrderByDescending(user => user.Name);

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
        /// Ordenates the list of users and returns a paged result.
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        private static UserPagedResultDto Pagination(IQueryable<UserPagedDto> totalList,
                                                     PagedDataDto pagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(pagedDataDto.Search))
            {
                totalList = totalList.Where(i => 
                    i.Name.ToLower().Contains(pagedDataDto.Search.ToLower()) ||
                    i.Email.ToLower().Contains(pagedDataDto.Search.ToLower()) ||
                    i.Id.ToString().Contains(pagedDataDto.Search) ||
                    i.Teams.Any(t => t.Name.ToLower().Contains(pagedDataDto.Search.ToLower())
                ));
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

            return new UserPagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount,
            };
        }

        /// <summary>
        /// Checks if an email is already in use by another user.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="excludeUserId"></param>
        /// <returns></returns>
        public async Task<bool> IsEmailInUseAsync(UserEmailDto userEmailDto)
        {
            if (string.IsNullOrEmpty(userEmailDto.Email))
            {
                throw new ArgumentException("Null or empty email");
            }

            return await _userRepository.EmailExistsAsync(userEmailDto.Email, userEmailDto.UserId);
        }

        /// <summary>
        /// Adds profiles to the user based on the provided UserUpdateDto.
        /// </summary>
        /// <param name="userUpdateDto"></param>
        /// <param name="user"></param>
        private void AddProfiles(UserUpdateDto userUpdateDto, User user)
        {
            if (userUpdateDto.ProfileIds != null)
            {
                user.Profiles.Clear();
                var profiles = _profileRepository.FindByIds(userUpdateDto.ProfileIds);
                foreach (var profile in profiles)
                {
                    user.AddProfile(profile);
                }
            }
        }

        /// <summary>
        /// Adds teams to the user based on the provided UserUpdateDto.
        /// </summary>
        /// <param name="userUpdateDto"></param>
        /// <param name="user"></param>
        private void AddTeams(UserUpdateDto userUpdateDto, User user)
        {
            if (userUpdateDto.TeamIds != null)
            {
                user.Teams.Clear();
                var teams = _teamRepository.FindByIds(userUpdateDto.TeamIds);
                foreach (var team in teams)
                {
                    user.AddTeam(team);
                }
            }
        }
    }
}
