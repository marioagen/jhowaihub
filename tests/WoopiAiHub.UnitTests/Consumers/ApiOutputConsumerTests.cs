using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.UnitTests.Fixture;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class ApiOutputConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly ApiOutputDto _apiOutputDto;
        private readonly Mock<IApiOutputServices> _apiOutputServices;
        private readonly Mock<IAutomationServices> _automationServices;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<ApiOutputDto>> _consumerMock;
        private readonly Mock<ILogger<ApiOutputConsumer>> _loggerMock;

        public ApiOutputConsumerTests()
        {
            _mocker = new AutoMocker();

            _apiOutputDto = MessagingFixture.FindValidApiOutputDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                ApiRequestQueueResponse = "apiRequestQueueResponse",
            });

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:TemplateConnection",
                  "Password=123;Persist Security Info=True;User ID=sa;Initial Catalog=___NEWDB___;Data Source=test;TrustServerCertificate=True;" }
            };

            var configuration = new ConfigurationBuilder()
                               .AddInMemoryCollection(inMemorySettings)
                               .Build();
            _mocker.Use<IConfiguration>(configuration);
            _mocker.Use<IOptions<MessageQueues>>(messageQueues);

            _apiOutputServices = new Mock<IApiOutputServices>();
            _automationServices = new Mock<IAutomationServices>();
            _usageDailyServices = new Mock<IUsageDailyServices>();

            _tenantCacheServices = new Mock<ITenantCacheServices>();
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                             .ReturnsAsync(tenant);

            var serviceProviderMock = new Mock<IServiceProvider>();
            
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IApiOutputServices)))
                               .Returns(_apiOutputServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAutomationServices)))
                               .Returns(_automationServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            _loggerMock = new Mock<ILogger<ApiOutputConsumer>>();
            _consumerMock = new Mock<IMessageConsumer<ApiOutputDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumerAsync", "ApiOutputConsumer unit tests")]
        public async Task ApiOutputConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidautomationServicesDto();

            _apiOutputServices
                .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .Returns(Task.CompletedTask);

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _apiOutputServices.Verify(x => x.ProcessMessage(_apiOutputDto), Times.Once);
            _automationServices.Verify(x => x.ContinueExecution(automationServicesDto), Times.Once);
            _usageDailyServices.Verify(x => x.AddByValuesAsync(It.IsAny<string>(), _apiOutputDto.Email!, 1, It.IsAny<string>(), null), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "ApiOutputConsumer unit tests")]
        public async Task ApiOutputConsumer_ConsumeAsync_ShouldCatchException_WhenProcessingMessage()
        {
            // Arrange
            var expectedException = new ArgumentException("StepToolExecution not found");

            _apiOutputServices
               .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
               .ThrowsAsync(expectedException);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<ApiOutputConsumer>>();
            _mocker.Use(loggerMock);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _apiOutputServices.Verify(x => x.ProcessMessage(_apiOutputDto), Times.Once);

            loggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Must set HttpContext TenantConnection when processing message")]
        [Trait("ConsumerAsync", "ApiOutputConsumer unit tests")]
        public async Task ApiOutputConsumer_ConsumeAsync_ShouldSetTenantConnection()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidautomationServicesDto();
            string? capturedConnectionString = null;

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContext);

            var tenantInfo = new TenantInfoDto
            {
                DatabaseName = "test-tenant-db",
                Email = "test@example.com",
                Name = "test-tenant"
            };

            var tenantCacheServicesMock = new Mock<ITenantCacheServices>();
            tenantCacheServicesMock.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                                   .ReturnsAsync(tenantInfo);

            _apiOutputServices
                .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
                .ReturnsAsync(automationServicesDto)
                .Callback(() =>
                {
                    capturedConnectionString = httpContext.Items["TenantConnection"] as string;
                });

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .Returns(Task.CompletedTask);

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IApiOutputServices)))
                               .Returns(_apiOutputServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAutomationServices)))
                               .Returns(_automationServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(tenantCacheServicesMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(capturedConnectionString);
            Assert.Contains("test-tenant", capturedConnectionString);
        }

        [Fact(DisplayName = "Must not call ContinueExecution when ProcessMessage throws exception")]
        [Trait("ConsumerAsync", "ApiOutputConsumer unit tests")]
        public async Task ApiOutputConsumer_ConsumeAsync_ShouldNotCallContinueExecution_WhenProcessMessageFails()
        {
            // Arrange
            _apiOutputServices
               .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
               .ThrowsAsync(new Exception("Processing failed"));

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _apiOutputServices.Verify(x => x.ProcessMessage(_apiOutputDto), Times.Once);
            _automationServices.Verify(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()), Times.Never);
            _usageDailyServices.Verify(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null), Times.Never);
        }
    }
}
