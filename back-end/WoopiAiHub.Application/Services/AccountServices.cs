using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        public AccountServices(IGraphApi graphApi,
                               IMarketPlaceApi marketPlaceApi,
                               IConfiguration config,
                               ILogger<AccountServices> logger,
                               IKeyGeneratorApi keyGeneratorApi,
                               ITenantServices tenantServices
                               )
        {
            _graphApi = graphApi;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _logger = logger;
            _keyGeneratorApi = keyGeneratorApi;
            _tenantServices = tenantServices;
        }

        /// <summary>
        /// Checks if the Azure token is valid, if it is valid it checks and verifies 
        /// that the user has permission to access the application and returns a token
        /// </summary>
        /// <param name="authenticateDto"></param>
        /// <param name="authenticateHeaderDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<AccessDataAuthDto> Authenticate(AuthenticateDto authenticateDto,
                                                          AuthenticateHeaderDto authenticateHeaderDto)
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
                var KeyAccess = _config.GetSection("KeyAccess").Get<string>()!;
                var userAccess = await _marketPlaceApi.CheckAccess(KeyAccess,
                                                                   authenticateDto.Login);

                if (userAccess != null && userAccess.HasAccess)
                {
                    var accessDataAuth = new AccessDataAuthDto
                    {
                        Token = GenerateToken(emailUserAzureRequest.Content.Mail ??
                                              emailUserAzureRequest.Content.UserPrincipalName),
                        Tenant = userAccess.Tenant
                    };

                    return accessDataAuth;
                }
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
    }
}
