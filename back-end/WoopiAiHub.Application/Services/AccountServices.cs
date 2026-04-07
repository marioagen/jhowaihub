using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Response.Account;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Infrastructure.Multitenancy;
using Newtonsoft.Json;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.DTOs.Response;

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
        private const string _messageHttpContextNotAvailable = "HttpContext is not available.";

        public AccountServices(IGraphApi graphApi,
                               IMarketPlaceApi marketPlaceApi,
                               IConfiguration config,
                               ILogger<AccountServices> logger,
                               IUserRepository userRepository,
                               IPermissionRepository permissionRepository,
                               ITenantContextService tenantContextService,
                               IHttpContextAccessor httpContextAccessor,
                               IPasswordHasher passwordHasher,
                               IRefreshTokenServices refreshTokenServices)
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
        public async Task<object> Login(LoginDto loginDto)
        {
            var userAccess = await CheckMarketplaceAccess(loginDto.Email);
            if (userAccess != null && userAccess.HasAccess)
            {
                if (string.IsNullOrEmpty(loginDto.Tenant))
                {
                    if (userAccess.Tenants.Count == 0)
                    {
                        throw new AppException(null,
                                               "User without access.",
                                               Domain.Utils.ErrorLabels.Login.UserWithoutAccess);
                    }
                    if (userAccess.Tenants.Count > 1)
                    {
                        return new
                        {
                            userAccess.Tenants
                        };
                    }

                    loginDto.Tenant = userAccess.Tenants.First().Name;
                }

                var tenant = FindAndValidateTenant(loginDto.Tenant, userAccess.Tenants);

                return await ProceedLogin(loginDto, tenant, true);
            }

            throw new AppException(null,
                                   "User without access.",
                                   Domain.Utils.ErrorLabels.Login.UserWithoutAccess);
        }

        /// <summary>
        /// Proceed login
        /// </summary>
        /// <param name="loginDto"></param>
        /// <param name="tenant"></param>
        /// <param name="checkPassword"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="AppException"></exception>
        private async Task<AccessDataAuthDto> ProceedLogin(LoginDto loginDto, string tenant, bool checkPassword)
        {
            var httpContext = _httpContextAccessor.HttpContext ??
                             throw new InvalidOperationException(_messageHttpContextNotAvailable);

            await _tenantContextService.InitializeTenantAsync(tenant);
            await _tenantContextService.TrySetTenantConnectionAsync(httpContext,
                                                                    tenant);

            var user = await _userRepository.FindByEmailAsync(loginDto.Email);
            if (user == null)
                throw new AppException(null,
                                       "User not found.",
                                       Domain.Utils.ErrorLabels.Login.UserNotFound);
            if (checkPassword)
            {
                bool isPasswordValid = _passwordHasher.Verify(loginDto.Password, user.PasswordHash, user.Salt);
                if (!isPasswordValid)
                {
                    throw new AppException(null,
                                           "Incorrect password.",
                                           Domain.Utils.ErrorLabels.Login.UserIncorrectPassword);
                }
            }

            var permissions = await _permissionRepository.FindUserPermissionsAsync(user.Email);
            var tokenJWT = await GenerateTokensAsync(user.Id, user.Email, permissions);
            this.SetRefreshTokenCookie(tokenJWT.RefreshToken);

            return new AccessDataAuthDto
            {
                Tenant = tenant,
                Token = tokenJWT.AccessToken,
                Email = user.Email,
                Name = user.Name
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
        public async Task<object> LoginSSO(AuthenticateDto authenticateDto, AuthenticateHeaderDto authenticateHeaderDto)
        {
            var emailUserAzureRequest = await _graphApi.FindEmailUserAzure(authenticateHeaderDto.Authorization);

            if (emailUserAzureRequest.Content is not null &&
               (emailUserAzureRequest.Content.UserPrincipalName.Equals(authenticateDto.Login) ||
                emailUserAzureRequest.Content.Mail.Equals(authenticateDto.Login)))
            {
                var userAccess = await CheckMarketplaceAccess(authenticateDto.Login);
                if (userAccess != null && userAccess.HasAccess)
                {
                    if (string.IsNullOrEmpty(authenticateDto.Tenant))
                    {
                        if (userAccess.Tenants.Count == 0)
                        {
                            throw new AppException(null,
                                                   "User without access.",
                                                   Domain.Utils.ErrorLabels.Login.UserWithoutAccess);
                        }
                        if (userAccess.Tenants.Count > 1)
                        {
                            return new
                            {
                                userAccess.Tenants
                            };
                        }

                        authenticateDto.Tenant = userAccess.Tenants.First().Name;
                    }

                    var tenant = FindAndValidateTenant(authenticateDto.Tenant, userAccess.Tenants);

                    var loginDto = new LoginDto
                    {
                        Email = authenticateDto.Login,
                        Password = string.Empty
                    };
                    return await ProceedLogin(loginDto, tenant, false);
                }

                throw new AppException(null,
                                       "User without access.",
                                       Domain.Utils.ErrorLabels.Login.UserWithoutAccess);
            }

            _logger.LogError(emailUserAzureRequest.Error is null ?
                           $"The user does not have permission." :
                           $"An error occurred in the request to the GraphApi. Error: {emailUserAzureRequest.Error?.Content}");

            throw new AppException(null,
                                   "User without access.",
                                   Domain.Utils.ErrorLabels.Login.UserTokenMicrosoftInvalid);
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

        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <remarks>This method validates the provided refresh token, checks the user's marketplace
        /// access, and generates new tokens if the user is authorized. The new refresh token is stored in a secure
        /// HTTP-only cookie, and the old refresh token is revoked.</remarks>
        /// <param name="refreshToken">The refresh token used to generate a new access token. This value cannot be null or empty.</param>
        /// <returns>A new access token as a string, or <see langword="null"/> if the provided refresh token is invalid or does
        /// not correspond to a user.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the HTTP context is not available during the operation.</exception>
        /// <exception cref="ArgumentException">Thrown if the user is not authorized to access the marketplace.</exception>
        public async Task<string?> RefreshTokenAsync(string refreshToken, string headerTenant)
        {
            var userEmail = await _refreshTokenServices.FindUserByRefreshTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var userAccess = await CheckMarketplaceAccess(userEmail);
            if (userAccess != null && userAccess.HasAccess)
            {
                var tenant = userAccess.Tenants.FirstOrDefault(t => t.Name.Equals(headerTenant));

                var httpContext = _httpContextAccessor.HttpContext ??
                                  throw new InvalidOperationException(_messageHttpContextNotAvailable);

                await _tenantContextService.InitializeTenantAsync(tenant!.Name);
                await _tenantContextService.TrySetTenantConnectionAsync(httpContext,
                                                                        tenant.Name);
                var permissions = await _permissionRepository.FindUserPermissionsAsync(userEmail);

                var user = await _userRepository.FindByEmailAsync(userEmail);
                var tokens = await GenerateTokensAsync(user.Id, userEmail, permissions);

                await _refreshTokenServices.RevokeAsync(refreshToken);
                await _refreshTokenServices.SaveAsync(userEmail, tokens.RefreshToken);

                if (_httpContextAccessor.HttpContext == null)
                    throw new InvalidOperationException(_messageHttpContextNotAvailable);

                this.SetRefreshTokenCookie(tokens.RefreshToken);

                return tokens.AccessToken;
            }

            throw new ArgumentException("User does not have authorization");
        }

        /// <summary>
        /// Revokes the specified refresh token and removes it from the client's cookies.
        /// </summary>
        /// <remarks>This method revokes the provided refresh token by delegating the operation to the
        /// underlying  refresh token service. If the HTTP context is available, it also clears the "refreshToken" 
        /// cookie from the client's browser by setting it to an expired state.</remarks>
        /// <param name="refreshToken">The refresh token to be revoked. Cannot be null or empty.</param>
        /// <returns><see langword="true"/> if the token was successfully revoked and the cookie was cleared;  otherwise, <see
        /// langword="false"/> if the HTTP context is unavailable.</returns>
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            await _refreshTokenServices.RevokeAsync(refreshToken);

            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                this.RemoveRefreshTokenCookie();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Searches for a tenant by name within the provided collection and validates its accessibility.
        /// </summary>
        /// <param name="tenant">The name of the tenant to locate. Comparison is case-insensitive.</param>
        /// <param name="tenants">A collection of <see cref="TenantAccessDto"/> objects representing available tenants.</param>
        /// <returns>The name of the tenant if found and validated.</returns>
        /// <exception cref="AppException">Thrown if the tenant is not found in the collection, or if the tenant's database is not ready or cannot be
        /// accessed.</exception>
        private static string FindAndValidateTenant(string tenant, ICollection<TenantAccessDto> tenants)
        {
            var tenantFound = tenants.FirstOrDefault(t => t.Name.Equals(tenant, StringComparison.OrdinalIgnoreCase));
            if (tenantFound == null)
            {
                throw new AppException(null,
                       "Tenant not found",
                        Domain.Utils.ErrorLabels.Login.TenantNotFound);
            }

            if (!tenantFound.IsDatabaseCreated)
            {
                throw new AppException(ErrorCode.BusinessWarningOutput,
                        "Tenant database is not ready or cannot be accessed.",
                        Domain.Utils.ErrorLabels.Login.TenantDatabaseNotReady);
            }
            return tenantFound.Name;
        }

        /// <summary>
        /// Returns an client id from appsettings
        /// </summary>
        /// <returns></returns>
        private async Task<ResponseCheckAccessDto> CheckMarketplaceAccess(string login)
        {
            var keyAccess = _config.GetSection("KeyAccess").Get<string>()!;
            return await _marketPlaceApi.CheckAccessByHub(keyAccess, login);
        }

        /// <summary>
        /// Generates an access token for the api with configurable expiration (default 1 hour).
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateToken(string user, int? tokenExpirationTime = null)
        {
            var key = _config["JWT:Key"] ?? throw new ArgumentException("JWT key is not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expirationMinutes = tokenExpirationTime ?? _config.GetValue("JWT:AccessTokenExpirationMinutes", 60);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        /// <summary>
        /// Asynchronously generates a new access token and refresh token for the specified user.
        /// </summary>
        /// <remarks>The access token includes claims for the user's email, a unique identifier, and
        /// issued-at timestamp.  If the user has an "admin" profile, an additional claim for the "Admin" role is
        /// included.  Permissions are encoded as claims in the format "perm:{resource}" with the associated actions as
        /// the value. The refresh token is stored using the refresh token service for later validation.</remarks>
        /// <param name="userEmail">The email address of the user for whom the tokens are being generated. Cannot be null or empty.</param>
        /// <param name="permissions">A dictionary representing the user's permissions, where the key is the resource name and the value is a list
        /// of actions the user is allowed to perform on that resource.</param>
        /// <returns>A tuple containing the generated access token and refresh token. The access token is a JWT string with a
        /// short expiration time,  and the refresh token is a string used to obtain a new access token after
        /// expiration.</returns>
        /// <exception cref="ArgumentException">Thrown if the JWT key is not configured in the application settings.</exception>
        private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(Guid userId,
                                                                                          string userEmail,
                                                                                          Dictionary<string, List<string>> permissions)
        {
            var key = _config["JWT:Key"] ?? throw new ArgumentException("JWT key is not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userProfile = await _userRepository.FindUserProfilesByEmailAsync(userEmail);
            bool isAdmin = userProfile.Contains("admin");
            var permissionsList = BuildPermissionsList(permissions, isAdmin);
            var permissionsJson = JsonConvert.SerializeObject(permissionsList);
            var claims = new List<Claim>
            {
                new Claim("userId", userId.ToString()),
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(JwtRegisteredClaimNames.Sub, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("isAdmin", isAdmin.ToString().ToLower()),
                new Claim("permissions", permissionsJson)
            };

            var expirationMinutes = _config.GetValue("JWT:AccessTokenExpirationMinutes", 60);
            var jwtToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = GenerateRefreshToken();
            await _refreshTokenServices.SaveAsync(userEmail, refreshToken);

            return (AccessToken: accessToken, RefreshToken: refreshToken);
        }

        /// <summary>
        /// Get the permission list
        /// </summary>
        /// <returns></returns>
        private static List<Dictionary<string, string>> BuildPermissionsList(Dictionary<string, List<string>> permissions, bool isAdmin)
        {
            var permissionsList = new List<Dictionary<string, string>>();
            if (isAdmin)
                return permissionsList;

            foreach (var kv in permissions)
            {
                var resource = kv.Key;
                foreach (var action in kv.Value)
                {
                    permissionsList.Add(new Dictionary<string, string> { { resource, action } });
                }
            }
            return permissionsList;
        }

        /// <summary>
        /// Generates the refresh token to renew the API token
        /// </summary>
        /// <returns></returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Base64UrlEncode(randomNumber);
        }

        /// <summary>
        /// Performs conversion to base 64
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        /// <summary>
        /// Sets a secure HTTP-only cookie containing the specified refresh token.
        /// </summary>
        /// <remarks>The cookie is configured with the following properties: <list type="bullet">
        /// <item><description><see cref="CookieOptions.HttpOnly"/> is set to <see langword="true"/> to prevent
        /// client-side access.</description></item> <item><description><see cref="CookieOptions.Secure"/> is set to
        /// <see langword="true"/> to ensure the cookie is transmitted over HTTPS only.</description></item>
        /// <item><description><see cref="CookieOptions.SameSite"/> is set to <see cref="SameSiteMode.None"/> to allow
        /// cross-site requests.</description></item> <item><description>The cookie's <see cref="CookieOptions.Path"/>
        /// is set to the root ("/").</description></item> <item><description>The cookie expires 7 days from the time it
        /// is set.</description></item> </list></remarks>
        /// <param name="refreshToken">The refresh token to store in the cookie.</param>
        /// <returns><see langword="true"/> if the cookie was successfully set; otherwise, <see langword="false"/> if the current
        /// HTTP context is unavailable.</returns>
        private bool SetRefreshTokenCookie(string refreshToken)
        {
            if (_httpContextAccessor.HttpContext == null)
                return false;
            var expirationDays = _config.GetValue("JWT:RefreshTokenExpirationDays", 7);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(expirationDays)
            });
            return true;
        }

        /// <summary>
        /// Removes the "refreshToken" cookie from the HTTP response by setting it with an expired timestamp.
        /// </summary>
        /// <remarks>This method ensures that the "refreshToken" cookie is effectively invalidated by
        /// appending it with an expiration date in the past. The method requires a valid HTTP context to perform the
        /// operation.</remarks>
        /// <returns><see langword="true"/> if the cookie was successfully removed; otherwise, <see langword="false"/> if the
        /// HTTP context is unavailable.</returns>
        private bool RemoveRefreshTokenCookie()
        {
            if (_httpContextAccessor.HttpContext == null)
                return false;
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            return true;
        }
    }
}
