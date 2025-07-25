using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response.Account;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Application.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WoopiAiHub.Application.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly IGraphApi _graphApi;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountServices> _logger;
        private readonly IKeyGeneratorApi _keyGeneratorApi;
        private readonly ITenantServices _tenantServices;
        private readonly IUserRepository _userRepository;

        public AccountServices(IGraphApi graphApi,
                               IMarketPlaceApi marketPlaceApi,
                               IConfiguration config,
                               ILogger<AccountServices> logger,
                               IKeyGeneratorApi keyGeneratorApi,
                               ITenantServices tenantServices,
                               IUserRepository userRepository
                               )
        {
            _graphApi = graphApi;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _logger = logger;
            _keyGeneratorApi = keyGeneratorApi;
            _tenantServices = tenantServices;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Checks if the Azure token is valid, if it is valid it checks and verifies
        /// that the user has permission to access the application and returns a token
        /// </summary>
        /// <param name="tokenAzureAd"></param>
        /// <param name="authenticateDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<AccessDataAuthDto> Login(LoginDto loginDto)
        {
            var userAccess = await GetMarketplaceAccesses(loginDto.Email);
            if (userAccess == null && !userAccess.HasAccess)
            {
                return new AccessDataAuthDto
                {
                    Success = false,
                    Message = "Not authorized.",
                    Data = null,
                };
            }

            var user = await _userRepository.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AccessDataAuthDto
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null,
                };
            }

            bool isPasswordValid = Encryption.VerifyHash(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new AccessDataAuthDto
                {
                    Success = false,
                    Message = "Password doesn't match.",
                    Data = null,
                };
            }
            
            // Get Users Permissions -> Must wait another PR
            return new AccessDataAuthDto
            {
                Success = true,
                Message = "User logged",
                Data = new LoginDataDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    Token = GenerateToken(user.Email),
                    Tenant = userAccess.Tenant,
                    IsAdmin = true,
                    Permissions = null,
                },
            };
        }

        /// <summary>
        /// Checks if the Azure token is valid, if it is valid it checks and verifies 
        /// that the user has permission to access the application and returns a token
        /// </summary>
        /// <param name="authenticateDto"></param>
        /// <param name="authenticateHeaderDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<AccessDataAuthDto> LoginSSO(AuthenticateDto authenticateDto, AuthenticateHeaderDto authenticateHeaderDto)
        {
            if (string.IsNullOrWhiteSpace(authenticateHeaderDto.Authorization))
            {
                _logger.LogError($"Token not provided.");
                throw new ArgumentException("Token not provided.");
            }

            var emailUserAzureRequest = await _graphApi.FindEmailUserAzure(authenticateHeaderDto.Authorization);

            if (emailUserAzureRequest.Content is not null &&
               (emailUserAzureRequest.Content.UserPrincipalName.Equals(authenticateDto.Login) ||
                emailUserAzureRequest.Content.Mail.Equals(authenticateDto.Login)))
            {
                var userAccess = await GetMarketplaceAccesses(authenticateDto.Login);
                if (userAccess == null && !userAccess.HasAccess)
                {
                    return new AccessDataAuthDto
                    {
                        Success = false,
                        Message = "Not authorized.",
                        Data = null,
                    };
                }

                return new AccessDataAuthDto
                {
                    Success = true,
                    Message = "User logged",
                    Data = new LoginDataDto
                    {
                        Name = "Askmann",
                        Email = "askmann@mail.com",
                        Token = GenerateToken(emailUserAzureRequest.Content.Mail ?? emailUserAzureRequest.Content.UserPrincipalName),
                        Tenant = userAccess.Tenant,
                        IsAdmin = true,
                        Permissions = null,
                    },
                };
            }

            _logger.LogError(emailUserAzureRequest.Error is null ?
                           $"The user does not have permission." :
                           $"An error occurred in the request to the GraphApi. Error: {emailUserAzureRequest.Error?.Content}");

            throw new ArgumentException("The user does not have permission");
        }

        /// <summary>
        /// Authenticate by Internal Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string AuthenticateApi(string key)
        {
            var appSettingsKeySecret = _config["KeyAccess"];
            if (key != appSettingsKeySecret)
            {
                _logger.LogError($"Key is invalid or not provided.");
                throw new ArgumentException("Key is invalid or not provided.");
            }
            return GenerateToken(key);
        }

        /// <summary>
        /// Generates an access token for the api that lasts for 1 hour
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateToken(string user)
        {
            var key = _config["JWT:Key"] ?? throw new ArgumentException("JWT key is not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Returns an client id from appsettings
        /// </summary>
        /// <returns></returns>
        public string FindClientId()
        {
            var clientId = _config["Azure:ClientId"];

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("Client id is not configured.");
            }

            return clientId;
        }

        /// <summary>
        /// Returns an client id from appsettings
        /// </summary>
        /// <returns></returns>
        private async Task<ResponseCheckAccessDto> GetMarketplaceAccesses(string login)
        {
            var keyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            return await _marketPlaceApi.CheckAccess(keyAccess, login);
        }
    }
}
