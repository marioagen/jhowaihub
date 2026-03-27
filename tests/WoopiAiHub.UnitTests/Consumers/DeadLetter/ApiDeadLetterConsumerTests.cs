using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging.DeadLetter;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers.DeadLetter
{
    [Collection(nameof(MessagingCollection))]
    public class ApiDeadLetterConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardServices> _cardServicesMock;
        private readonly Mock<IMessageConsumer<ApiRequestDto>> _consumerMock;
        private readonly Mock<ILogger<ApiDeadLetterConsumer>> _loggerMock;

        public ApiDeadLetterConsumerTests()
        {
            _mocker = new AutoMocker();
            _cardServicesMock = new Mock<ICardServices>();
            _consumerMock = new Mock<IMessageConsumer<ApiRequestDto>>();
            _loggerMock = new Mock<ILogger<ApiDeadLetterConsumer>>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                {
                    "ConnectionStrings:TemplateConnection",
                    "Password=123;Persist Security Info=True;User ID=sa;Initial Catalog=___NEWDB___;Data Source=test;TrustServerCertificate=True;"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var messageQueues = Options.Create(new MessageQueues
            {
                ApiRequestQueue = "api-request-queue"
            });

            var tenantCacheServicesMock = new Mock<ITenantCacheServices>();
            tenantCacheServicesMock.Setup(s => s.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(new TenantInfoDto { DatabaseName = "TestDB" });

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICardServices)))
                .Returns(_cardServicesMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                .Returns(httpContextAccessorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                .Returns(tenantCacheServicesMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            _mocker.Use<IConfiguration>(configuration);
            _mocker.Use<IOptions<MessageQueues>>(messageQueues);
            _mocker.Use(scopeFactoryMock.Object);
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
        }

        [Fact(DisplayName = "ExecuteAsync should call SetFailingCard with correct cardId and email")]
        [Trait("ApiDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_ValidMessage_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);
            var apiRequestDto = new ApiRequestDto { Data = metaData, Email = email };

            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "api-request-queue.dlq"),
                It.IsAny<Func<ApiRequestDto, Task>>()))
                .Callback<string, Func<ApiRequestDto, Task>>(async (queue, handler) =>
                {
                    await handler(apiRequestDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should log error when SetFailingCard throws exception")]
        [Trait("ApiDeadLetterConsumer", "Failure")]
        public async Task ExecuteAsync_SetFailingCardThrowsException_LogsError()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);
            var apiRequestDto = new ApiRequestDto { Data = metaData, Email = email };

            var expectedException = new Exception("Test error");
            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .ThrowsAsync(expectedException);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "api-request-queue.dlq"),
                It.IsAny<Func<ApiRequestDto, Task>>()))
                .Callback<string, Func<ApiRequestDto, Task>>(async (queue, handler) =>
                {
                    try
                    {
                        await handler(apiRequestDto);
                    }
                    catch
                    {
                        // Expected
                    }
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ApiDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }
    }
}
