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
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers.DeadLetter
{
    [Collection(nameof(MessagingCollection))]
    public class OcrDeadLetterConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardServices> _cardServicesMock;
        private readonly Mock<IMessageConsumer<ProcessOcrDto>> _consumerMock;
        private readonly Mock<ILogger<OcrDeadLetterConsumer>> _loggerMock;

        public OcrDeadLetterConsumerTests()
        {
            _mocker = new AutoMocker();
            _cardServicesMock = new Mock<ICardServices>();
            _consumerMock = new Mock<IMessageConsumer<ProcessOcrDto>>();
            _loggerMock = new Mock<ILogger<OcrDeadLetterConsumer>>();

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
                OcrQueue = "ocr-queue"
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
        [Trait("OcrDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_ValidMessage_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);
            var processOcrDto = new ProcessOcrDto { Data = metaData, Email = email };

            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "ocr-queue.dlq"),
                It.IsAny<Func<ProcessOcrDto, Task>>()))
                .Callback<string, Func<ProcessOcrDto, Task>>(async (queue, handler) =>
                {
                    await handler(processOcrDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<OcrDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should handle null email gracefully")]
        [Trait("OcrDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_NullEmail_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 2;
            string email = string.Empty;
            var metaData = new MetaDataAutomationDto(cardId, 0);
            var processOcrDto = new ProcessOcrDto { Data = metaData, Email = email };

            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "ocr-queue.dlq"),
                It.IsAny<Func<ProcessOcrDto, Task>>()))
                .Callback<string, Func<ProcessOcrDto, Task>>(async (queue, handler) =>
                {
                    await handler(processOcrDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<OcrDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should log error when SetFailingCard throws exception")]
        [Trait("OcrDeadLetterConsumer", "Failure")]
        public async Task ExecuteAsync_SetFailingCardThrowsException_LogsError()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);
            var processOcrDto = new ProcessOcrDto { Data = metaData, Email = email };

            var expectedException = new Exception("Test error");
            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .ThrowsAsync(expectedException);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "ocr-queue.dlq"),
                It.IsAny<Func<ProcessOcrDto, Task>>()))
                .Callback<string, Func<ProcessOcrDto, Task>>(async (queue, handler) =>
                {
                    try
                    {
                        await handler(processOcrDto);
                    }
                    catch
                    {
                        // Expected
                    }
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<OcrDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }
    }
}
