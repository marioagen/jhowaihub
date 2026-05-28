using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Application.Validation;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs.Response.Account;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.Infrastructure.Multitenancy;
using WoopiAiHub.UnitTests.Fixtures;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(AccountCollection))]
    public class AccountServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly AccountFixture _fixture;
        private readonly AccountServices _accountServices;
        private readonly Mock<IJwtTokenServices> _mockJwtTokenServices;

        public AccountServicesTests(AccountFixture accountFixture)
        {
            this._fixture = accountFixture;
            _mocker = new AutoMocker();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["JWT:Key"]).Returns(Guid.NewGuid().ToString());
            configMock.Setup(x => x["KeyAccess"]).Returns("mockKey");
            configMock.Setup(x => x["Azure:ClientId"]).Returns("clientMock");
            configMock.Setup(x => x.GetSection("KeyAccess").Value).Returns("mockedKey");
            configMock.Setup(x => x["JWT:Key"]).Returns(Guid.NewGuid().ToString());
            configMock.Setup(x => x["Jwt:Issuer"]).Returns("http://localhost");
            configMock.Setup(x => x["Jwt:Audience"]).Returns("http://localhost");

            var mockJwtAccessTokenSection = new Mock<IConfigurationSection>();
            mockJwtAccessTokenSection.Setup(x => x.Value).Returns("60");
            configMock.Setup(x => x.GetSection("JWT:AccessTokenExpirationMinutes")).Returns(mockJwtAccessTokenSection.Object);

            var mockJwtRefreshTokenSection = new Mock<IConfigurationSection>();
            mockJwtRefreshTokenSection.Setup(x => x.Value).Returns("7");
            configMock.Setup(x => x.GetSection("JWT:RefreshTokenExpirationDays")).Returns(mockJwtRefreshTokenSection.Object);

            _mocker.Use(configMock);
            _mockJwtTokenServices = _mocker.GetMock<IJwtTokenServices>();

            _mocker.GetMock<IUserTenantAccessCacheServices>()
                .Setup(s => s.FindAllowedTenantsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<TenantAccessDto>());

            _mocker.Use<ITenantBindingValidator>(new TenantBindingValidator(
                _mocker.GetMock<IUserTenantAccessCacheServices>().Object,
                Mock.Of<ILogger<TenantBindingValidator>>()));

            _accountServices = _mocker.CreateInstance<AccountServices>();
        }

        [Fact(DisplayName = "Test authenticate LoginSSO Sucess")]
        [Trait("Authenticate", "Success")]
        public async Task Authenticate_LoginSSO_Success()
        {
            // Arrange
            var tenants = new List<TenantAccessDto>
                {
                    new TenantAccessDto("Tenant1", true ),
                    new TenantAccessDto ("Tenant2",true )
                };

            var permissions = new List<string> { "read", "write" };
            var permissionDic = new Dictionary<string, List<string>>
            {
                { "group1", permissions}
            };
            var profiles = new List<string> { "admin", "profile2" };
            var user = AccountFixture.FindValidUser();
            var authenticateHeaderDto = AccountFixture.FindValidAuthenticateHeaderDto();
            var authenticateDto = AccountFixture.FindValidAuthenticateDto();
            authenticateDto.Tenant = tenants.First().Name;

            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();
            graphApiResponse.Content!.Mail = authenticateDto.Login;
            var responseCheckAccess = _fixture.FindValidResponseCheckAccessDto();
            responseCheckAccess.Tenants = tenants;

            var _mockTenantContextService = _mocker.GetMock<ITenantContextService>();
            var _mockHttpContext = _mocker.GetMock<HttpContext>();
            var _mockHttpResponse = _mocker.GetMock<HttpResponse>();
            var _mockResponseCookies = _mocker.GetMock<IResponseCookies>();
            var _mockUserRepository = _mocker.GetMock<IUserRepository>();
            var _mockRefreshTokenServices = _mocker.GetMock<IRefreshTokenServices>();
            var iGraphApi = _mocker.GetMock<IGraphApi>();
            var iMarketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            var _mockPermissionRepository = _mocker.GetMock<IPermissionRepository>();
            var _mockHttpContextAccessor = _mocker.GetMock<IHttpContextAccessor>();
            var _passwordHasherMock = _mocker.GetMock<IPasswordHasher>();

            _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);
            _mockHttpResponse.Setup(x => x.Cookies).Returns(_mockResponseCookies.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));
            iMarketPlaceApi.Setup(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(responseCheckAccess);
            _mockRefreshTokenServices
                .Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockPermissionRepository
                .Setup(x => x.FindUserPermissionsAsync(It.IsAny<string>()))
                .ReturnsAsync(permissionDic);
            _mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockUserRepository
                .Setup(x => x.FindUserProfilesByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(profiles);
            _mockUserRepository
                .Setup(x => x.Update(It.IsAny<User>()))
                .Returns(true);
            _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(true);

            // Act
            var result = await _accountServices.LoginSSO(authenticateDto, authenticateHeaderDto);

            // Assert
            iGraphApi.Verify(r => r.FindEmailUserAzure(It.IsAny<string>()), Times.Once);
            iMarketPlaceApi.Verify(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _passwordHasherMock.Verify(a => a.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
            _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);

            var authData = Assert.IsType<AccessDataAuthDto>(result);
            AssertTenantClaim(authData.Token, authenticateDto.Tenant);
        }

        [Fact(DisplayName = "Test authenticate Sucess")]
        [Trait("Authenticate", "Success")]
        public async Task Authenticate_Login_Success()
        {
            // Arrange
            var loginDto = AccountFixture.FindValidLoginDto();
            var permissions = new List<string> { "read", "write" };
            var permissionDic = new Dictionary<string, List<string>>
            {
                { "group1", permissions}
            };
            var profiles = new List<string> { "admin", "profile2" };
            var user = AccountFixture.FindValidUser();
            var authenticateHeaderDto = AccountFixture.FindValidAuthenticateHeaderDto();
            var authenticateDto = AccountFixture.FindValidAuthenticateDto();

            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();
            graphApiResponse.Content!.Mail = authenticateDto.Login;
            var responseCheckAccess = _fixture.FindValidResponseCheckAccessDto();
            loginDto.Tenant = responseCheckAccess.Tenants.First().Name;

            var _mockTenantContextService = _mocker.GetMock<ITenantContextService>();
            var _mockHttpContext = _mocker.GetMock<HttpContext>();
            var _mockHttpResponse = _mocker.GetMock<HttpResponse>();
            var _mockResponseCookies = _mocker.GetMock<IResponseCookies>();
            var _mockUserRepository = _mocker.GetMock<IUserRepository>();
            var _mockRefreshTokenServices = _mocker.GetMock<IRefreshTokenServices>();
            var iGraphApi = _mocker.GetMock<IGraphApi>();
            var iMarketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            var _mockPermissionRepository = _mocker.GetMock<IPermissionRepository>();
            var _mockHttpContextAccessor = _mocker.GetMock<IHttpContextAccessor>();
            var _mockPasswordHasher = _mocker.GetMock<IPasswordHasher>();

            _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);
            _mockHttpResponse.Setup(x => x.Cookies).Returns(_mockResponseCookies.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));
            iMarketPlaceApi.Setup(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(responseCheckAccess));
            _mockRefreshTokenServices
                .Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockPermissionRepository
                .Setup(x => x.FindUserPermissionsAsync(It.IsAny<string>()))
                .ReturnsAsync(permissionDic);
            _mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockUserRepository
                .Setup(x => x.FindUserProfilesByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(profiles);
            _mockUserRepository
                .Setup(x => x.Update(It.IsAny<User>()))
                .Returns(true);
            _mockPasswordHasher.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(true);

            // Act
            var result = await _accountServices.Login(loginDto);

            // Assert
            iMarketPlaceApi.Verify(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockPasswordHasher.Verify(a => a.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
            _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);

            var authData = Assert.IsType<AccessDataAuthDto>(result);
            AssertTenantClaim(authData.Token, loginDto.Tenant);
        }

        [Fact(DisplayName = "Test Authenticate fail")]
        [Trait("Authenticate", "Fail")]
        public async Task Authenticate_Login_Fail()
        {
            // Arrange
            var authenticateHeaderDto = AccountFixture.FindValidAuthenticateHeaderDto();
            var authenticateDto = AccountFixture.FindValidAuthenticateDto();
            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();

            var iGraphApi = _mocker.GetMock<IGraphApi>();

            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));
            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => accountServices.LoginSSO(authenticateDto, authenticateHeaderDto));
            iGraphApi.Verify(r => r.FindEmailUserAzure(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "Test Authenticate fail when Authotization is empty")]
        [Trait("Authenticate", "Fail")]
        public async Task Authenticate_Login_Fail_When_Authotization_Is_Empty()
        {
            // Arrange
            var authenticateHeaderDto = AccountFixture.FindValidAuthenticateHeaderDto();
            authenticateHeaderDto.Authorization = string.Empty;
            var authenticateDto = AccountFixture.FindValidAuthenticateDto();
            var responseCheckAccess = _fixture.FindValidResponseCheckAccessDto();
            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();
            var _mockTenantContextService = _mocker.GetMock<ITenantContextService>();
            var _mockHttpContext = _mocker.GetMock<HttpContext>();
            var _mockUserRepository = _mocker.GetMock<IUserRepository>();

            var iGraphApi = _mocker.GetMock<IGraphApi>();
            var iMarketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();

            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));
            iMarketPlaceApi.Setup(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(responseCheckAccess));
            _mockTenantContextService
                .Setup(x => x.InitializeTenantAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockTenantContextService
                .Setup(x => x.TrySetTenantConnectionAsync(_mockHttpContext.Object, It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            _mockUserRepository
               .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
               .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => _accountServices.LoginSSO(authenticateDto, authenticateHeaderDto));
        }

        [Fact(DisplayName = "Test Authenticate by internalKey")]
        [Trait("AuthenticateApi", "Success")]
        public void AuthenticateApi_Success()
        {
            // Act
            _mockJwtTokenServices
                .Setup(x =>
                    x.GenerateTokenWithParameters(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        "mockKey",
                        null
                    )
                )
                .Returns("token-test");

            var result = _accountServices.AuthenticateApi("mockKey");

            // Assert
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Test Authenticate by internalKey")]
        [Trait("AuthenticateApi", "Fail")]
        public void AuthenticateApi_Fail()
        {
            // Act/Assert
            Assert.Throws<ArgumentException>(() => _accountServices.AuthenticateApi(""));
        }

        [Fact(DisplayName = "Test FindClientId by valid id")]
        [Trait("Authenticate", "Success")]
        public void FindClientId_Success()
        {
            // Act
            var result = _accountServices.FindClientId();

            // Assert
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Test FindClientId by invalid id")]
        [Trait("Authenticate", "Fail")]
        public void FindClientId_Fail()
        {
            //Arrange
            var configMockError = new Mock<IConfiguration>();
            configMockError.Setup(x => x["Azure:ClientId"]).Returns(string.Empty);
            _mocker.Use(configMockError.Object);
            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act/Assert
            Assert.Throws<ArgumentException>(() => accountServices.FindClientId());
        }

        [Fact(DisplayName = "Test FindKeyAccess by valid key")]
        [Trait("Authenticate", "Success")]
        public async Task RefreshTokenAsync_WhenValidRefreshToken_ShouldReturnAccessToken()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var userEmail = "user@example.com";
            var permissions = new List<string> { "read", "write" };
            var permissionDic = new Dictionary<string, List<string>>
            {
                { "group1", permissions}
            };

            var expectedRefreshToken = "new-refresh-token";
            var responseCheckAccess = _fixture.FindValidResponseCheckAccessDto();
            var profiles = new List<string> { "admin", "profile2" };
            var tenant = responseCheckAccess.Tenants.First().Name;
            var user = AccountFixture.FindValidUser();

            var _mockUserRepository = _mocker.GetMock<IUserRepository>();
            var _mockRefreshTokenServices = _mocker.GetMock<IRefreshTokenServices>();
            var _mockTenantContextService = _mocker.GetMock<ITenantContextService>();
            var _mockPermissionRepository = _mocker.GetMock<IPermissionRepository>();
            var _mockHttpContextAccessor = _mocker.GetMock<IHttpContextAccessor>();

            var _mockHttpContext = _mocker.GetMock<HttpContext>();
            var _mockHttpResponse = _mocker.GetMock<HttpResponse>();
            var _mockResponseCookies = _mocker.GetMock<IResponseCookies>();
            var iMarketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();

            iMarketPlaceApi.Setup(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(responseCheckAccess));

            _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);
            _mockHttpResponse.Setup(x => x.Cookies).Returns(_mockResponseCookies.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            _mockRefreshTokenServices
                .Setup(x => x.FindUserByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(userEmail);

            _mockTenantContextService
                .Setup(x => x.InitializeTenantAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockTenantContextService
                .Setup(x => x.TrySetTenantConnectionAsync(_mockHttpContext.Object, It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            _mockPermissionRepository
                .Setup(x => x.FindUserPermissionsAsync(userEmail))
                .ReturnsAsync(permissionDic);

            _mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockRefreshTokenServices
                .Setup(x => x.RevokeAsync(refreshToken))
                .Returns(Task.CompletedTask);

            _mockRefreshTokenServices
                .Setup(x => x.SaveAsync(userEmail, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockUserRepository
                .Setup(x => x.FindUserProfilesByEmailAsync(userEmail))
                .ReturnsAsync(profiles);

            _mockResponseCookies
                .Setup(x => x.Append(
                    "refreshToken",
                    It.IsAny<string>(),
                    It.IsAny<CookieOptions>()))
                .Verifiable();

            // Act
            var result = await _accountServices.RefreshTokenAsync(refreshToken, tenant);

            // Assert
            Assert.NotNull(result);
            AssertTenantClaim(result!, tenant);
            _mockRefreshTokenServices.Verify(x => x.FindUserByRefreshTokenAsync(refreshToken), Times.Once);
            _mockTenantContextService.Verify(x => x.InitializeTenantAsync(It.IsAny<string>()), Times.Once);
            _mockTenantContextService.Verify(x => x.TrySetTenantConnectionAsync(_mockHttpContext.Object, It.IsAny<string>()), Times.Once);
            _mockPermissionRepository.Verify(x => x.FindUserPermissionsAsync(userEmail), Times.Once);
            _mockRefreshTokenServices.Verify(x => x.RevokeAsync(refreshToken), Times.Once);
            _mockRefreshTokenServices.Verify(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact(DisplayName = "RefreshToken throws AppException when tenant is not in marketplace list")]
        [Trait("RefreshToken", "Fail")]
        public async Task RefreshTokenAsync_InvalidTenant_ThrowsAppException()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var userEmail = "user@example.com";
            var responseCheckAccess = _fixture.FindValidResponseCheckAccessDto();

            var mockRefreshTokenServices = _mocker.GetMock<IRefreshTokenServices>();
            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();

            mockRefreshTokenServices
                .Setup(x => x.FindUserByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(userEmail);

            marketPlaceApiMock
                .Setup(api => api.CheckAccessByHub(It.IsAny<string>(), userEmail))
                .ReturnsAsync(responseCheckAccess);

            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                accountServices.RefreshTokenAsync(refreshToken, "TenantNotInList"));

            // Assert
            Assert.Equal(Login.TenantNotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Login ShouldThrowAppException_WhenTenantNotFound")]
        [Trait("Login", "Fail")]
        public async Task Login_ShouldThrowAppException_WhenTenantNotFound()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "password123",
                Tenant = "TenantNotExist"
            };

            var tenants = new List<TenantAccessDto>
                {
                    new TenantAccessDto("Tenant1", true ),
                    new TenantAccessDto ("Tenant2",true )
                };

            var userAccess = new ResponseCheckAccessDto
            {
                HasAccess = true,
                Tenants = tenants
            };

            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApiMock
                .Setup(api => api.CheckAccessByHub(It.IsAny<string>(), loginDto.Email))
                .ReturnsAsync(userAccess);

            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() => accountServices.Login(loginDto));

            // Assert
            Assert.Equal("Tenant not found", exception.Message);
            marketPlaceApiMock.Verify(api => api.CheckAccessByHub(It.IsAny<string>(), loginDto.Email), Times.Once);
        }

        [Fact(DisplayName = "Login ShouldThrowAppException_WhenDatabaseNotCreated")]
        [Trait("Login", "Fail")]
        public async Task Login_ShouldThrowAppException_WhenDatabaseNotCreated()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "password123",
                Tenant = "Tenant1"
            };

            var tenants = new List<TenantAccessDto>
                {
                    new TenantAccessDto("Tenant1", false ),
                    new TenantAccessDto ("Tenant2", true )
                };

            var userAccess = new ResponseCheckAccessDto
            {
                HasAccess = true,
                Tenants = tenants
            };

            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApiMock
                .Setup(api => api.CheckAccessByHub(It.IsAny<string>(), loginDto.Email))
                .ReturnsAsync(userAccess);

            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() => accountServices.Login(loginDto));

            // Assert
            Assert.Equal("Tenant database is not ready or cannot be accessed.", exception.Message);
            marketPlaceApiMock.Verify(api => api.CheckAccessByHub(It.IsAny<string>(), loginDto.Email), Times.Once);
        }

        [Fact(DisplayName = "Login should throw AppException when Tenant is empty and user has no tenants")]
        [Trait("Login", "Fail")]
        public async Task Login_ShouldThrowAppException_WhenTenantIsEmptyAndUserHasNoTenants()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "password123",
                Tenant = string.Empty
            };

            var userAccess = new ResponseCheckAccessDto
            {
                HasAccess = true,
                Tenants = new List<TenantAccessDto>()
            };

            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApiMock
                .Setup(api => api.CheckAccessByHub(It.IsAny<string>(), loginDto.Email))
                .ReturnsAsync(userAccess);

            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => accountServices.Login(loginDto));

            Assert.NotNull(exception);
            Assert.Equal("User without access.", exception.Message);
            Assert.Equal(Domain.Utils.ErrorLabels.Login.UserWithoutAccess, exception.LabelError);
            marketPlaceApiMock.Verify(api => api.CheckAccessByHub(It.IsAny<string>(), loginDto.Email), Times.Once);
        }

        [Fact(DisplayName = "LoginSSO should throw AppException when Tenant is empty and user has no tenants")]
        [Trait("LoginSSO", "Fail")]
        public async Task LoginSSO_ShouldThrowAppException_WhenTenantEmptyAndNoTenants()
        {
            // Arrange
            var authenticateDto = new AuthenticateDto
            {
                Login = "user@example.com",
                Tenant = string.Empty
            };

            var authenticateHeaderDto = new AuthenticateHeaderDto
            {
                Authorization = "Bearer token"
            };

            var userGraph = new UserGraphApiResponse
            {
                Mail = authenticateDto.Login,
                UserPrincipalName = authenticateDto.Login
            };

            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();
            graphApiResponse.Content!.Mail = authenticateDto.Login;

            var iGraphApi = _mocker.GetMock<IGraphApi>();
            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));

            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            var userAccess = new ResponseCheckAccessDto
            {
                HasAccess = true,
                Tenants = new List<TenantAccessDto>()
            };
            marketPlaceApiMock
                .Setup(m => m.CheckAccessByHub(It.IsAny<string>(), authenticateDto.Login))
                .ReturnsAsync(userAccess);

            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => accountServices.LoginSSO(authenticateDto, authenticateHeaderDto));

            Assert.Equal("User without access.", ex.Message);
            Assert.Equal(Login.UserWithoutAccess, ex.LabelError);

            iGraphApi.Verify(g => g.FindEmailUserAzure(It.IsAny<string>()), Times.Once);
            marketPlaceApiMock.Verify(m => m.CheckAccessByHub(It.IsAny<string>(), authenticateDto.Login), Times.Once);
        }

        [Fact(DisplayName = "Login should return tenants list when Tenant is empty and user has access to multiple tenants")]
        [Trait("Login", "Success")]
        public async Task Login_ShouldReturnTenantsList_WhenTenantIsEmptyAndUserHasMultipleTenants()
        {
            // Arrange
            var tenants = new List<TenantAccessDto>
                {
                    new TenantAccessDto("Tenant1", true ),
                    new TenantAccessDto ("Tenant2",true )
                };

            var authenticateDto = new AuthenticateDto
            {
                Login = "user@example.com",
                Tenant = string.Empty
            };

            var authenticateHeaderDto = new AuthenticateHeaderDto
            {
                Authorization = "Bearer token"
            };

            var userAccess = new ResponseCheckAccessDto
            {
                HasAccess = true,
                Tenants = tenants
            };

            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();
            graphApiResponse.Content!.Mail = authenticateDto.Login;

            var iGraphApi = _mocker.GetMock<IGraphApi>();
            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));

            _mocker.GetMock<IMarketPlaceApi>()
                   .Setup(m => m.CheckAccessByHub(It.IsAny<string>(), authenticateDto.Login))
                   .ReturnsAsync(userAccess);

            var accountServices = _mocker.CreateInstance<AccountServices>();

            // Act
            var result = await accountServices.LoginSSO(authenticateDto, authenticateHeaderDto);

            // Assert
            Assert.NotNull(result);
            _mocker.GetMock<IGraphApi>().Verify(g => g.FindEmailUserAzure(It.IsAny<string>()), Times.Once);
            _mocker.GetMock<IMarketPlaceApi>().Verify(m => m.CheckAccessByHub(It.IsAny<string>(), authenticateDto.Login), Times.Once);
        }

        private static void AssertTenantClaim(string accessToken, string expectedTenant)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(accessToken) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            var tenantClaim = jwtToken!.Claims.FirstOrDefault(c => c.Type == JwtClaimNames.Tenant);
            Assert.NotNull(tenantClaim);
            Assert.Equal(expectedTenant, tenantClaim!.Value);
        }
    }
}
