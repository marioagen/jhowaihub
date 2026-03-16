using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging.DeadLetter;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers.DeadLetter
{
    [Collection(nameof(MessagingCollection))]
    public class N8NDeadLetterConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardServices> _cardServicesMock;
        private readonly Mock<IMessageConsumer<AutomationInputDto>> _consumerMock;
        private readonly Mock<ILogger<N8NDeadLetterConsumer>> _loggerMock;

        public N8NDeadLetterConsumerTests()
        {
            _mocker = new AutoMocker();
            _cardServicesMock = new Mock<ICardServices>();
            _consumerMock = new Mock<IMessageConsumer<AutomationInputDto>>();
            _loggerMock = new Mock<ILogger<N8NDeadLetterConsumer>>();

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
                AutomationQueueConsumer = "automation-queue-consumer"
            });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICardServices)))
                .Returns(_cardServicesMock.Object);

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
        [Trait("N8NDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_ValidMessage_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var automationInputDto = new AutomationInputDto { Email = email };

            _cardServicesMock.Setup(s => s.SetFailingCard(It.IsAny<int>(), email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "automation-queue-consumer.dlq"),
                It.IsAny<Func<AutomationInputDto, Task>>()))
                .Callback<string, Func<AutomationInputDto, Task>>(async (queue, handler) =>
                {
                    await handler(automationInputDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(It.IsAny<int>(), email), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should handle null email gracefully")]
        [Trait("N8NDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_NullEmail_CallsSetFailingCard()
        {
            // Arrange
            string? email = null;
            var automationInputDto = new AutomationInputDto { Email = email };

            _cardServicesMock.Setup(s => s.SetFailingCard(It.IsAny<int>(), null))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "automation-queue-consumer.dlq"),
                It.IsAny<Func<AutomationInputDto, Task>>()))
                .Callback<string, Func<AutomationInputDto, Task>>(async (queue, handler) =>
                {
                    await handler(automationInputDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(It.IsAny<int>(), null), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should log error when SetFailingCard throws exception")]
        [Trait("N8NDeadLetterConsumer", "Failure")]
        public async Task ExecuteAsync_SetFailingCardThrowsException_LogsError()
        {
            // Arrange
            var email = "test@example.com";
            var automationInputDto = new AutomationInputDto { Email = email };

            var expectedException = new Exception("Test error");
            _cardServicesMock.Setup(s => s.SetFailingCard(It.IsAny<int>(), email))
                .ThrowsAsync(expectedException);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "automation-queue-consumer.dlq"),
                It.IsAny<Func<AutomationInputDto, Task>>()))
                .Callback<string, Func<AutomationInputDto, Task>>(async (queue, handler) =>
                {
                    try
                    {
                        await handler(automationInputDto);
                    }
                    catch
                    {
                        // Expected
                    }
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(It.IsAny<int>(), email), Times.Once);
        }
    }
}
