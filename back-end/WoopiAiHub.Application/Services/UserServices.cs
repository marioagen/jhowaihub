using Microsoft.Extensions.Configuration;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
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

            var existingUser = await _userRepository.FindByEmailAsync(userCreateDto.Email);

            if (existingUser != null && !existingUser.IsActive)
            {
                return await ReactivateUser(existingUser, userCreateDto, headersDto);
            }
            else if (existingUser != null && existingUser.IsActive)
            {
                throw new AppException(ErrorCode.Duplicated, "Duplicated user", null);
            }
            else
            {
                var userEnabledReference = await AssignLicensesMarketplace(userCreateDto.Email, Guid.Empty, headersDto);

                if (userEnabledReference == Guid.Empty)
                    return false;
                return await CreateUser(userCreateDto, userEnabledReference);
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
            var existingUser = await _userRepository.FindByEmailAsync(userUpdateDto.Email);

            if (existingUser != null && !existingUser.IsActive)
            {
                var userCreateDto = new UserCreateDto
                {
                    Name = userUpdateDto.Name,
                    Email = userUpdateDto.Email,
                    Password = userUpdateDto.Password,
                    TeamIds =  userUpdateDto.TeamIds,
                    ProfileIds = userUpdateDto.ProfileIds,
                };
                return await ReactivateUser(existingUser, userCreateDto, headersDto);
            }
            else if (existingUser != null && existingUser.IsActive)
            {
                throw new AppException(ErrorCode.Duplicated, "Duplicated user", null);
            }
            else
            {
                var marketplaceIdentifier = await AssignLicensesMarketplace(userUpdateDto.Email, userUpdateDto.Id, headersDto);
                if (marketplaceIdentifier != Guid.Empty)
                {
                    return await UpdateUser(userUpdateDto);
                }
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
        /// Find a Team by id
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<ICollection<UserDto>> FindByTeamId(int teamId)
        {
            return await _userRepository.FindByTeamIdAsync(teamId);
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
        /// Adds profiles to the user based on the provided profileIds.
        /// </summary>
        /// <param name="profileIds"></param>
        /// <param name="user"></param>
        private void AddProfiles(ICollection<int>? profileIds, User user)
        {
            if (profileIds != null)
            {
                user.Profiles.Clear();
                var profiles = _profileRepository.FindByIds(profileIds);
                foreach (var profile in profiles)
                {
                    user.AddProfile(profile);
                }
            }
        }

        /// <summary>
        /// Adds teams to the user based on the provided teamIds.
        /// </summary>
        /// <param name="teamIds"></param>
        /// <param name="user"></param>
        private void AddTeams(ICollection<int>? teamIds, User user)
        {
            if (teamIds != null)
            {
                user.Teams.Clear();
                var teams = _teamRepository.FindByIds(teamIds);
                foreach (var team in teams)
                {
                    user.AddTeam(team);
                }
            }
        }

        /// <summary>
        /// Sets the password and salt for a user based on the provided DTO and user object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        /// <param name="salt"></param>
        private void SetSaltAndPassword(string password,
                                        User user,
                                        byte[] salt)
        {

            if (salt == null || salt.Length == 0)
            {
                salt = _passwordHasher.GenerateSalt();
            }
            var hashedPassword = _passwordHasher.Hash(password, salt);
            user.SetPassword(hashedPassword, salt);
        }
        
        /// <summary>
        ///  Creates a new user based on the provided UserCreateDto and userEnabledReference.
        /// </summary>
        /// <param name="userCreateDto"></param>
        /// <param name="userEnabledReference"></param>
        /// <returns></returns>
        private async Task<bool> CreateUser(UserCreateDto userCreateDto,
                                            Guid userEnabledReference)
        {
            User user = new User(
                    userEnabledReference,
                    userCreateDto.Name,
                    userCreateDto.Email,
                    true,
                    DateTime.Now
              );

            SetSaltAndPassword(userCreateDto.Password, user, null);

            if (userCreateDto.TeamIds.Count > 0)
            {
                AddTeams(userCreateDto.TeamIds, user);
            }

            if (userCreateDto.ProfileIds.Count > 0)
            {
                AddProfiles(userCreateDto.ProfileIds, user);
            }

            return await _userRepository.CreateAsync(user);
        }

        /// <summary>
        /// Reactivate an existing user based on the provided UserCreateDto.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userCreateDto"></param>
        /// <returns></returns>
        private async Task<bool> ReactivateUser(User user,
                                                UserCreateDto userCreateDto,
                                                HeadersDto headersDto)
        {
            user.Reactivate(userCreateDto.Name,
                            userCreateDto.Email);


            if (!string.IsNullOrEmpty(userCreateDto.Password))
            {
                SetSaltAndPassword(userCreateDto.Password, user, user.Salt);
            }

            var marketplaceIdentifier = await AssignLicensesMarketplace(userCreateDto.Email, user.Id, headersDto);
            if (marketplaceIdentifier != Guid.Empty)
            {
                _userRepository.Update(user);
            }
            return true;
        }

        /// <summary>
        ///  Updates an existing user based on the provided UserUpdateDto.
        /// </summary>
        /// <param name="userUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<bool> UpdateUser(UserUpdateDto userUpdateDto)
        {
                var user = await _userRepository.FindByReferenceAsync(userUpdateDto.Id);
                if (user == null)
                    return false;

                user.Update(userUpdateDto.Name,
                            userUpdateDto.Email);

                if (!string.IsNullOrEmpty(userUpdateDto.Password))
                {
                    SetSaltAndPassword(userUpdateDto.Password, user, user.Salt);
                }

                AddTeams(userUpdateDto.TeamIds, user);

                AddProfiles(userUpdateDto.ProfileIds, user);

                var updateResult = _userRepository.Update(user);
                return updateResult;
        }

        /// <summary>
        /// Create a user to the marketplace by email and id.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        private async Task<Guid> AssignLicensesMarketplace(string email,
                                                           Guid id,
                                                           HeadersDto headersDto)
        {
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var requestAssignLicensesByHub = new RequestAssignLicensesByHub
            {
                UserEmail = email,
                Tenant = headersDto.Tenant,
                IdUser =  id,
            };
            var userEnabledReference = await _marketPlaceApi.AssignLicensesByHub(KeyAccess, requestAssignLicensesByHub);

            return userEnabledReference;
        }
    }
}
