using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository;

namespace WoopiAiHub.Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserServices> _logger;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly ITeamRepository _teamRepository;

        public UserServices(IUserRepository userRepository,
                            ILogger<UserServices> logger,
                            IMarketPlaceApi marketPlaceApi,
                            IConfiguration config,
                            ITeamRepository teamRepository)
        {
            _userRepository = userRepository;
            _teamRepository = teamRepository;
            _logger = logger;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
        }

        /// <summary>
        /// Create an user by dto
        /// </summary>
        /// <param name="userCreateDto"></param>
        /// <returns></returns>
        public async Task<bool> Create(UserCreateDto userCreateDto, HeadersDto headersDto)
        {
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var requestAssignLicensesByHub = new RequestAssignLicensesByHub
            {
                UserEmail = userCreateDto.Email,
                Tenant = headersDto.Tenant,
            };
            var userEnabled = await _marketPlaceApi.AssignLicensesByHub(KeyAccess, requestAssignLicensesByHub);

            if (userEnabled != null)
            {
                User user = new User(
                    userEnabled,
                    userCreateDto.Name,
                    userCreateDto.Email,
                    true,
                    DateTime.Now
                );

                // Busque os times existentes pelo ID
                var teams = _teamRepository.FindByIds(userCreateDto.TeamIds); // Implemente esse método no repositório

                foreach (var team in teams)
                {
                    user.AddTeam(team);
                }

                return _userRepository.Create(user);
            }
            return false;
        }


        // <summary>
        /// Delete users by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeactivateRange(List<Guid> ids)
        {
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var users = _userRepository.FindByIds(ids);
            var allExists = ids.All(id => users.Any(u => u.Id == id));

            if (allExists)
            {
                var requestDto = new DeactivateUsersDto { Reference_user = ids };
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
            var updateByHubDto = new UpdateByHubDto
            {
                Reference_user = userUpdateDto.Id,
                UserEmail = userUpdateDto.Email,
                Tenant = headersDto.Tenant,
            };

            var updateMkt = await _marketPlaceApi.UpdateUserEnabled(KeyAccess, updateByHubDto);
            if (updateMkt)
            {
                // Buscar o usuário atual do banco
                var users = _userRepository.FindByIds(new List<Guid> { userUpdateDto.Id });
                var user = users.FirstOrDefault();
                if (user == null)
                    return false;

                // Atualizar propriedades básicas
                user.Name = userUpdateDto.Name;
                user.Email = userUpdateDto.Email;

                // Atualizar times se informado
                if (userUpdateDto.TeamIds != null)
                {
                    var teams = _teamRepository.FindByIds(userUpdateDto.TeamIds);
                    user.Teams.Clear();
                    foreach (var team in teams)
                    {
                        user.AddTeam(team);
                    }
                }

                var updateResult = _userRepository.Update(user); // Aqui, ajuste o repositório para aceitar User se necessário
                if (!updateResult)
                {
                    throw new ArgumentException("Duplicated User");
                }
                return updateResult;
            }

            return false;
        }

        /// <summary>
        /// Retrieves all teams paged and their users.
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
        /// Ordenates the list of Teams and returns a paged result.
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        private static UserPagedResultDto Pagination(IQueryable<UserDtoPaged> totalList,
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

            return new UserPagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount,
            };
        }

    }
}
