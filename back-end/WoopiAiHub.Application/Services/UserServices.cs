using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
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

        public UserServices(IUserRepository userRepository,
                            ILogger<UserServices> logger,
                            IMarketPlaceApi marketPlaceApi,
                            IConfiguration config)
        {
            _userRepository = userRepository;
            _logger = logger;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
        }

        /// <summary>
        /// Create an user by dto
        /// </summary>
        /// <param name="UserCreateDto"></param>
        /// <returns></returns>
        public async Task<bool> Create(UserCreateDto userCreateDto,
                                       HeadersDto headersDto)
        {
            var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            var requestAssignLicensesByHub = new RequestAssignLicensesByHub
            {
                UserEmail = userCreateDto.Email,
                Tenant = headersDto.Tenant,
            };
            var userEnabled = await _marketPlaceApi.AssignLicensesByHub(KeyAccess, requestAssignLicensesByHub);

            if(userEnabled != null)
            {
                User user = new User
                (
                        Guid.Parse(userEnabled.ReferenceUser),
                        userCreateDto.Name,
                        headersDto.EmailCreator,
                        userCreateDto.IsActive,
                        DateTime.Now
                );

                return _userRepository.Create(user);
            }
            return false;
        }
    }
}
