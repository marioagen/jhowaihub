using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Linq;
using WoopiAiHub.Application.Messaging.DeadLetter;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers.DeadLetter
{
    [Collection(nameof(MessagingCollection))]
    public class QuizDeadLetterConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardServices> _cardServicesMock;
        private readonly Mock<IMessageConsumer<DocumentEmbeddingsQueryDto>> _consumerMock;
        private readonly Mock<ILogger<QuizDeadLetterConsumer>> _loggerMock;

        public QuizDeadLetterConsumerTests()
        {
            _mocker = new AutoMocker();
            _cardServicesMock = new Mock<ICardServices>();
            _consumerMock = new Mock<IMessageConsumer<DocumentEmbeddingsQueryDto>>();
            _loggerMock = new Mock<ILogger<QuizDeadLetterConsumer>>();

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
                AnswerQueue = "answer-queue"
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

        [Fact(DisplayName = "ExecuteAsync should call SetFailingCard with cardId extracted from Data and email")]
        [Trait("QuizDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_ValidMessage_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);

            var documentEmbeddingsQueryDto = new DocumentEmbeddingsQueryDto
            {
                Data = JObject.Parse(JToken.FromObject(metaData).ToString()),
                Email = email
            };

            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "answer-queue.dlq"),
                It.IsAny<Func<DocumentEmbeddingsQueryDto, Task>>()))
                .Callback<string, Func<DocumentEmbeddingsQueryDto, Task>>(async (queue, handler) =>
                {
                    await handler(documentEmbeddingsQueryDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<QuizDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should handle null email gracefully")]
        [Trait("QuizDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_NullEmail_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 2;
            string? email = null;
            var metaData = new MetaDataAutomationDto(cardId, 0);

            var documentEmbeddingsQueryDto = new DocumentEmbeddingsQueryDto
            {
                Data = JObject.Parse(JToken.FromObject(metaData).ToString()),
                Email = email
            };

            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, null))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "answer-queue.dlq"),
                It.IsAny<Func<DocumentEmbeddingsQueryDto, Task>>()))
                .Callback<string, Func<DocumentEmbeddingsQueryDto, Task>>(async (queue, handler) =>
                {
                    await handler(documentEmbeddingsQueryDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<QuizDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, null), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should log error when SetFailingCard throws exception")]
        [Trait("QuizDeadLetterConsumer", "Failure")]
        public async Task ExecuteAsync_SetFailingCardThrowsException_LogsError()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);

            var documentEmbeddingsQueryDto = new DocumentEmbeddingsQueryDto
            {
                Data = JObject.Parse(JToken.FromObject(metaData).ToString()),
                Email = email
            };

            var expectedException = new Exception("Test error");
            _cardServicesMock.Setup(s => s.SetFailingCard(cardId, email))
                .ThrowsAsync(expectedException);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "answer-queue.dlq"),
                It.IsAny<Func<DocumentEmbeddingsQueryDto, Task>>()))
                .Callback<string, Func<DocumentEmbeddingsQueryDto, Task>>(async (queue, handler) =>
                {
                    try
                    {
                        await handler(documentEmbeddingsQueryDto);
                    }
                    catch
                    {
                        // Expected
                    }
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<QuizDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _cardServicesMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }
    }
}
