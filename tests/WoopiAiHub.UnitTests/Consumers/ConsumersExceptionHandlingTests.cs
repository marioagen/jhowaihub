using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Automation;
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
    public class ConsumersExceptionHandlingTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IApiOutputServices> _apiOutputServices;
        private readonly Mock<IN8NServices> _n8nServices;
        private readonly Mock<IAutomationServices> _automationServices;
        private readonly Mock<IFailingCardService> _failingCardService;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<ApiOutputDto>> _apiOutputConsumerMock;
        private readonly Mock<IMessageConsumer<AutomationOutputDto>> _n8nConsumerMock;
        private readonly Mock<ILogger<ApiOutputConsumer>> _apiOutputLoggerMock;
        private readonly Mock<ILogger<N8NConsumer>> _n8nLoggerMock;

        public ConsumersExceptionHandlingTests()
        {
            _mocker = new AutoMocker();

            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                ApiRequestQueueResponse = "apiRequestQueueResponse",
                AutomationQueueResponse = "automationQueueResponse"
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
            _n8nServices = new Mock<IN8NServices>();
            _automationServices = new Mock<IAutomationServices>();
            _failingCardService = new Mock<IFailingCardService>();
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
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IN8NServices)))
                               .Returns(_n8nServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAutomationServices)))
                               .Returns(_automationServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IFailingCardService)))
                               .Returns(_failingCardService.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            _apiOutputLoggerMock = new Mock<ILogger<ApiOutputConsumer>>();
            _n8nLoggerMock = new Mock<ILogger<N8NConsumer>>();
            _apiOutputConsumerMock = new Mock<IMessageConsumer<ApiOutputDto>>();
            _n8nConsumerMock = new Mock<IMessageConsumer<AutomationOutputDto>>();

            _mocker.Use(_apiOutputConsumerMock.Object);
            _mocker.Use(_n8nConsumerMock.Object);
            _mocker.Use(_apiOutputLoggerMock.Object);
            _mocker.Use(_n8nLoggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        #region ApiOutputConsumer Exception Handling Tests

        [Fact(DisplayName = "ApiOutputConsumer should mark card as failing when ContinueExecution throws exception")]
        [Trait("ApiOutputConsumer", "Exception Handling")]
        public async Task ApiOutputConsumer_ShouldMarkCardAsFailingWhenContinueExecutionThrows()
        {
            // Arrange
            var apiOutputDto = MessagingFixture.FindValidApiOutputDto();
            var automationServicesDto = MessagingFixture.FindValidAutomationServicesDto();

            _apiOutputServices
                .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new InvalidOperationException("Automation service error"));

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _failingCardService
                .Setup(x => x.SetFailingCard(automationServicesDto.CardId, apiOutputDto.Email))
                .Returns(Task.CompletedTask);

            _apiOutputConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _failingCardService.Verify(
                x => x.SetFailingCard(automationServicesDto.CardId, apiOutputDto.Email),
                Times.Once,
                "Card should be marked as failing when ContinueExecution throws");

            _apiOutputLoggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "ApiOutputConsumer should handle when SetFailingCard throws exception")]
        [Trait("ApiOutputConsumer", "Exception Handling")]
        public async Task ApiOutputConsumer_ShouldHandleFailingCardServiceException()
        {
            // Arrange
            var apiOutputDto = MessagingFixture.FindValidApiOutputDto();
            var automationServicesDto = MessagingFixture.FindValidAutomationServicesDto();

            _apiOutputServices
                .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("Automation error"));

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _failingCardService
                .Setup(x => x.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Failing card service error"));

            _apiOutputConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act - Should not throw
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _apiOutputLoggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact(DisplayName = "ApiOutputConsumer should not mark card as failing when CardId is zero or negative")]
        [Trait("ApiOutputConsumer", "Exception Handling")]
        public async Task ApiOutputConsumer_ShouldNotMarkCardAsFailingWhenCardIdIsInvalid()
        {
            // Arrange
            var apiOutputDto = MessagingFixture.FindValidApiOutputDto();
            var automationServicesDto = new AutomationServicesDto(0, 0, "tenant", "test@example.com", "ref", 1);

            _apiOutputServices
                .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("Automation error"));

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _apiOutputConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _failingCardService.Verify(
                x => x.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never,
                "Card should not be marked as failing when CardId is invalid");
        }

        [Fact(DisplayName = "ApiOutputConsumer should log error when ProcessMessage throws exception")]
        [Trait("ApiOutputConsumer", "Exception Handling")]
        public async Task ApiOutputConsumer_ShouldLogErrorWhenProcessMessageThrows()
        {
            // Arrange
            var apiOutputDto = MessagingFixture.FindValidApiOutputDto();
            var expectedException = new Exception("Process message error");

            _apiOutputServices
                .Setup(x => x.ProcessMessage(It.IsAny<ApiOutputDto>()))
                .ThrowsAsync(expectedException);

            _apiOutputConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ApiOutputDto, Task>>()))
                         .Callback<string, Func<ApiOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(apiOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiOutputConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _apiOutputLoggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region N8NConsumer Exception Handling Tests

        [Fact(DisplayName = "N8NConsumer should mark card as failing when ContinueExecution throws exception")]
        [Trait("N8NConsumer", "Exception Handling")]
        public async Task N8NConsumer_ShouldMarkCardAsFailingWhenContinueExecutionThrows()
        {
            // Arrange
            var automationOutputDto = MessagingFixture.FindValidAutomationOutputDto();
            var automationServicesDto = MessagingFixture.FindValidAutomationServicesDto();

            _n8nServices
                .Setup(x => x.ProcessMessage(It.IsAny<AutomationOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new TimeoutException("N8N service timeout"));

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _failingCardService
                .Setup(x => x.SetFailingCard(automationServicesDto.CardId, automationOutputDto.Email))
                .Returns(Task.CompletedTask);

            _n8nConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<AutomationOutputDto, Task>>()))
                         .Callback<string, Func<AutomationOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(automationOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _failingCardService.Verify(
                x => x.SetFailingCard(automationServicesDto.CardId, automationOutputDto.Email),
                Times.Once,
                "Card should be marked as failing when ContinueExecution throws");

            _n8nLoggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "N8NConsumer should handle when SetFailingCard throws exception")]
        [Trait("N8NConsumer", "Exception Handling")]
        public async Task N8NConsumer_ShouldHandleFailingCardServiceException()
        {
            // Arrange
            var automationOutputDto = MessagingFixture.FindValidAutomationOutputDto();
            var automationServicesDto = MessagingFixture.FindValidAutomationServicesDto();

            _n8nServices
                .Setup(x => x.ProcessMessage(It.IsAny<AutomationOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("N8N error"));

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _failingCardService
                .Setup(x => x.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Failing card service error"));

            _n8nConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<AutomationOutputDto, Task>>()))
                         .Callback<string, Func<AutomationOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(automationOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NConsumer>();

            // Act - Should not throw
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _n8nLoggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact(DisplayName = "N8NConsumer should not mark card as failing when CardId is zero or negative")]
        [Trait("N8NConsumer", "Exception Handling")]
        public async Task N8NConsumer_ShouldNotMarkCardAsFailingWhenCardIdIsInvalid()
        {
            // Arrange
            var automationOutputDto = MessagingFixture.FindValidAutomationOutputDto();
            var automationServicesDto = new AutomationServicesDto(0, 0, "tenant", "test@example.com", "ref", 1);

            _n8nServices
                .Setup(x => x.ProcessMessage(It.IsAny<AutomationOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _automationServices
                .Setup(x => x.ContinueExecution(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("N8N error"));

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);

            _n8nConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<AutomationOutputDto, Task>>()))
                         .Callback<string, Func<AutomationOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(automationOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _failingCardService.Verify(
                x => x.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never,
                "Card should not be marked as failing when CardId is invalid");
        }

        [Fact(DisplayName = "N8NConsumer should log error when ProcessMessage throws exception")]
        [Trait("N8NConsumer", "Exception Handling")]
        public async Task N8NConsumer_ShouldLogErrorWhenProcessMessageThrows()
        {
            // Arrange
            var automationOutputDto = MessagingFixture.FindValidAutomationOutputDto();
            var expectedException = new Exception("Process message error");

            _n8nServices
                .Setup(x => x.ProcessMessage(It.IsAny<AutomationOutputDto>()))
                .ThrowsAsync(expectedException);

            _n8nConsumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<AutomationOutputDto, Task>>()))
                         .Callback<string, Func<AutomationOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(automationOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _n8nLoggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
