using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
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
            _mocker.Use(configMock);

            _accountServices = _mocker.CreateInstance<AccountServices>();
        }

        [Fact(DisplayName = "Test authenticate LoginSSO Sucess")]
        [Trait("Authenticate", "Success")]
        public async Task Authenticate_LoginSSO_Success()
        {
            // Arrange
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

            var configMock = new Mock<IConfiguration>();

            _mocker.Use(configMock.Object);

            // Act
            var result = await _accountServices.LoginSSO(authenticateDto, authenticateHeaderDto);

            // Assert
            iGraphApi.Verify(r => r.FindEmailUserAzure(It.IsAny<string>()), Times.Once);
            iMarketPlaceApi.Verify(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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

            _mockPasswordHasher.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(true);

            // Act
            var result = await _accountServices.Login(loginDto);

            // Assert
            iMarketPlaceApi.Verify(a => a.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockPasswordHasher.Verify(a => a.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
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

            _mockRefreshTokenServices
                .Setup(x => x.RevokeAsync(refreshToken))
                .Returns(Task.CompletedTask);

            _mockRefreshTokenServices
                .Setup(x => x.SaveAsync(userEmail, refreshToken))
                .Returns(Task.CompletedTask);

            _mockRefreshTokenServices
                .Setup(x => x.SaveAsync(userEmail, expectedRefreshToken))
                .Returns(Task.CompletedTask);

            _mockUserRepository
                .Setup(x => x.FindUserProfilesByEmailAsync(userEmail))
                .ReturnsAsync(profiles);

            _mockResponseCookies
                .Setup(x => x.Append(
                    "refreshToken",
                    expectedRefreshToken,
                    It.IsAny<CookieOptions>()))
                .Verifiable();

            // Act
            var result = await _accountServices.RefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            _mockRefreshTokenServices.Verify(x => x.FindUserByRefreshTokenAsync(refreshToken), Times.Once);
            _mockTenantContextService.Verify(x => x.InitializeTenantAsync(It.IsAny<string>()), Times.Once);
            _mockTenantContextService.Verify(x => x.TrySetTenantConnectionAsync(_mockHttpContext.Object, It.IsAny<string>()), Times.Once);
            _mockPermissionRepository.Verify(x => x.FindUserPermissionsAsync(userEmail), Times.Once);
            _mockRefreshTokenServices.Verify(x => x.RevokeAsync(refreshToken), Times.Once);
            _mockRefreshTokenServices.Verify(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeast(2));
        }
    }
}
