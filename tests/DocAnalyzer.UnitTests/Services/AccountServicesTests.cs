using DocAnalyzer.Application.Services;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.Interfaces.Refit;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.UnitTests.Fixtures;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace DocAnalyzer.UnitTests.Services
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

            _mocker.Use(configMock);

            _accountServices = _mocker.CreateInstance<AccountServices>();
        }

        [Fact(DisplayName = "Test authenticate Sucess")]
        [Trait("Authenticate", "Success")]
        public async Task Authenticate_Login_Success()
        {
            // Arrange
            var authenticateHeaderDto = AccountFixture.FindValidAuthenticateHeaderDto();
            var authenticateDto = AccountFixture.FindValidAuthenticateDto();

            var graphApiResponse = _fixture.FindValidUserGraphApiResponse();
            graphApiResponse.Content!.Mail = authenticateDto.Login;
            var responseCheckAccess = _fixture.FindValidResponseCheckAccessDto();

            var iGraphApi = _mocker.GetMock<IGraphApi>();
            var iMarketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();

            iGraphApi.Setup(a => a.FindEmailUserAzure(It.IsAny<string>())).Returns(Task.FromResult(graphApiResponse));
            iMarketPlaceApi.Setup(a => a.CheckAccess(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(responseCheckAccess));

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x.GetSection("KeyAccess").Value).Returns("mockedKey");
            configMock.Setup(x => x["JWT:Key"]).Returns(Guid.NewGuid().ToString());
            configMock.Setup(x => x["Jwt:Issuer"]).Returns("http://localhost");
            configMock.Setup(x => x["Jwt:Audience"]).Returns("http://localhost");
            _mocker.Use(configMock.Object);

            var accountServices = _mocker.CreateInstance<AccountServices>();
            // Act
            var result = await accountServices.Authenticate(authenticateDto, authenticateHeaderDto);

            // Assert
            iGraphApi.Verify(r => r.FindEmailUserAzure(It.IsAny<string>()), Times.Once);
            iMarketPlaceApi.Verify(a => a.CheckAccess(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => accountServices.Authenticate(authenticateDto, authenticateHeaderDto));
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

            // Act/Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _accountServices.Authenticate(authenticateDto, authenticateHeaderDto));
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
    }
}
