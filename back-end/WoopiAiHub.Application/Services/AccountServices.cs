using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Response.Account;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Infrastructure.Multitenancy;

namespace WoopiAiHub.Application.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly IGraphApi _graphApi;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountServices> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ITenantContextService _tenantContextService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenServices _refreshTokenServices;


        public AccountServices(IGraphApi graphApi,
                               IMarketPlaceApi marketPlaceApi,
                               IConfiguration config,
                               ILogger<AccountServices> logger,
                               IUserRepository userRepository,
                               IPermissionRepository permissionRepository,
                               ITenantContextService tenantContextService,
                               IHttpContextAccessor httpContextAccessor,
                               IPasswordHasher passwordHasher,
                               IRefreshTokenServices refreshTokenServices
                               )
        {
            _graphApi = graphApi;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _logger = logger;
            _userRepository = userRepository;
            _permissionRepository = permissionRepository;
            _tenantContextService = tenantContextService;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = passwordHasher;
            _refreshTokenServices = refreshTokenServices;
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
            var userAccess = await CheckMarketplaceAccess(loginDto.Email);
            if (userAccess != null && userAccess.HasAccess)
            {
                await _tenantContextService.InitializeTenantAsync(userAccess.Tenant);
                await _tenantContextService.TrySetTenantConnectionAsync(_httpContextAccessor.HttpContext,
                                                                        userAccess.Tenant);
                var user = await _userRepository.FindByEmailAsync(loginDto.Email);
                if (user == null)
                    throw new ArgumentException("User not found.");

                bool isPasswordValid = _passwordHasher.Verify(loginDto.Password, user.PasswordHash, user.Salt);
                if (!isPasswordValid)
                {
                    throw new ArgumentException("Invalid password.");
                }

                var permissions = await _permissionRepository.GetUserPermissionsAsync(user.Email);
                var tokenJWT = await GenerateTokensAsync(user.Email, permissions);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", tokenJWT.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                return new AccessDataAuthDto
                {
                    Tenant = userAccess.Tenant,
                    Token = tokenJWT.AccessToken,
                    Email = user.Email,
                    Name = user.Name
                };
            }

            throw new ArgumentException("Not authorized.");
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
            var emailUserAzureRequest = await _graphApi.FindEmailUserAzure(authenticateHeaderDto.Authorization);

            if (emailUserAzureRequest.Content is not null &&
               (emailUserAzureRequest.Content.UserPrincipalName.Equals(authenticateDto.Login) ||
                emailUserAzureRequest.Content.Mail.Equals(authenticateDto.Login)))
            {
                var userAccess = await CheckMarketplaceAccess(authenticateDto.Login);
                if (userAccess != null && userAccess.HasAccess)
                {
                    await _tenantContextService.InitializeTenantAsync(userAccess.Tenant);
                    await _tenantContextService.TrySetTenantConnectionAsync(_httpContextAccessor.HttpContext, userAccess.Tenant);
                    var user = await _userRepository.FindByEmailAsync(authenticateDto.Login);
                    if (user == null)
                        throw new ArgumentException("User not found.");

                    var permissions = await _permissionRepository.GetUserPermissionsAsync(authenticateDto.Login);
                    var tokenJWT = await GenerateTokensAsync(user.Email, permissions);
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", tokenJWT.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });
                    return new AccessDataAuthDto
                    {
                        Tenant = userAccess.Tenant,
                        Token = tokenJWT.AccessToken
                    };
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

        public async Task<string?> RefreshTokenAsync(string refreshToken)
        {
            var userEmail = await _refreshTokenServices.FindUserByRefreshTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var userAccess = await CheckMarketplaceAccess(userEmail);
            if (userAccess != null && userAccess.HasAccess)
            {
                await _tenantContextService.InitializeTenantAsync(userAccess.Tenant);
                await _tenantContextService.TrySetTenantConnectionAsync(_httpContextAccessor.HttpContext, userAccess.Tenant);
                var permissions = await _permissionRepository.GetUserPermissionsAsync(userEmail);

                var tokens = await GenerateTokensAsync(userEmail, permissions);

                await _refreshTokenServices.RevokeAsync(refreshToken);
                await _refreshTokenServices.SaveAsync(userEmail, tokens.RefreshToken);

                if (_httpContextAccessor.HttpContext == null)
                    throw new InvalidOperationException("HttpContext is not available.");

                _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return tokens.AccessToken;
            }

            throw new ArgumentException("Not authorized.");
        }

        /// <summary>
        /// Returns an client id from appsettings
        /// </summary>
        /// <returns></returns>
        private async Task<ResponseCheckAccessDto> CheckMarketplaceAccess(string login)
        {
            var keyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            return await _marketPlaceApi.CheckAccess(keyAccess, login);
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

        private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(string userEmail,
                                                                                         Dictionary<string, List<string>> permissions)
        {

            var key = _config["JWT:Key"] ?? throw new ArgumentException("JWT key is not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userProfile = await _userRepository.GetUserProfilesAsync(userEmail);
            bool isAdmin = userProfile.Contains("admin"); // idealmente encapsular essa lógica

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            foreach (var kv in permissions)
            {
                var resource = kv.Key;
                var actions = string.Join(',', kv.Value);
                claims.Add(new Claim($"perm:{resource}", actions));
            }

            // 3. Criar token JWT (Access Token)
            var now = DateTime.UtcNow;
            var jwtToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(5),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = GenerateRefreshToken();
            await _refreshTokenServices.SaveAsync(userEmail, refreshToken);

            return (AccessToken: accessToken, RefreshToken: refreshToken);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Base64UrlEncode(randomNumber);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

    }
}
