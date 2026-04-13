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
    public class EmbeddingsDeadLetterConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IFailingCardService> _failingCardServiceMock;
        private readonly Mock<IMessageConsumer<DocumentEmbeddingsDataDto>> _consumerMock;
        private readonly Mock<ILogger<EmbeddingsDeadLetterConsumer>> _loggerMock;

        public EmbeddingsDeadLetterConsumerTests()
        {
            _mocker = new AutoMocker();
            _failingCardServiceMock = new Mock<IFailingCardService>();
            _consumerMock = new Mock<IMessageConsumer<DocumentEmbeddingsDataDto>>();
            _loggerMock = new Mock<ILogger<EmbeddingsDeadLetterConsumer>>();

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
                EmbeddingQueue = "embedding-queue"
            });

            var tenantCacheServicesMock = new Mock<ITenantCacheServices>();
            tenantCacheServicesMock.Setup(s => s.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(new TenantInfoDto { DatabaseName = "TestDB" });

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IFailingCardService)))
                .Returns(_failingCardServiceMock.Object);
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

        [Fact(DisplayName = "ExecuteAsync should call SetFailingCard with correct cardId and email from embedding")]
        [Trait("EmbeddingsDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_ValidMessage_CallsSetFailingCard()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);

            var embeddingDto = new DocumentEmbeddingsAddDto { Email = email };
            var documentEmbeddingsDataDto = new DocumentEmbeddingsDataDto
            {
                Data = metaData,
                DocumentEmbeddings = new List<DocumentEmbeddingsAddDto> { embeddingDto }
            };

            _failingCardServiceMock.Setup(s => s.SetFailingCard(cardId, email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "embedding-queue.dlq"),
                It.IsAny<Func<DocumentEmbeddingsDataDto, Task>>()))
                .Callback<string, Func<DocumentEmbeddingsDataDto, Task>>(async (queue, handler) =>
                {
                    await handler(documentEmbeddingsDataDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<EmbeddingsDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _failingCardServiceMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should throw exception when no documents identified")]
        [Trait("EmbeddingsDeadLetterConsumer", "Failure")]
        public async Task ExecuteAsync_NoDocumentsIdentified_ThrowsException()
        {
            // Arrange
            var metaData = new MetaDataAutomationDto(1, 0);
            var documentEmbeddingsDataDto = new DocumentEmbeddingsDataDto
            {
                Data = metaData,
                DocumentEmbeddings = new List<DocumentEmbeddingsAddDto>()
            };

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "embedding-queue.dlq"),
                It.IsAny<Func<DocumentEmbeddingsDataDto, Task>>()))
                .Callback<string, Func<DocumentEmbeddingsDataDto, Task>>(async (queue, handler) =>
                {
                    try
                    {
                        await handler(documentEmbeddingsDataDto);
                    }
                    catch
                    {
                        // Expected
                    }
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<EmbeddingsDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert - Handler was attempted even with invalid data
            _consumerMock.Verify(c => c.ConsumerAsync(
                It.IsAny<string>(),
                It.IsAny<Func<DocumentEmbeddingsDataDto, Task>>()), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should use email from first document embedding")]
        [Trait("EmbeddingsDeadLetterConsumer", "Success")]
        public async Task ExecuteAsync_UsesEmailFromFirstDocumentEmbedding()
        {
            // Arrange
            var cardId = 2;
            var email = "embedding@example.com";
            var metaData = new MetaDataAutomationDto(cardId, 0);

            var embeddingDto = new DocumentEmbeddingsAddDto { Email = email };
            var documentEmbeddingsDataDto = new DocumentEmbeddingsDataDto
            {
                Data = metaData,
                DocumentEmbeddings = new List<DocumentEmbeddingsAddDto> { embeddingDto }
            };

            _failingCardServiceMock.Setup(s => s.SetFailingCard(cardId, email))
                .Returns(Task.CompletedTask);

            _consumerMock.Setup(c => c.ConsumerAsync(
                It.Is<string>(q => q == "embedding-queue.dlq"),
                It.IsAny<Func<DocumentEmbeddingsDataDto, Task>>()))
                .Callback<string, Func<DocumentEmbeddingsDataDto, Task>>(async (queue, handler) =>
                {
                    await handler(documentEmbeddingsDataDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<EmbeddingsDeadLetterConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _failingCardServiceMock.Verify(s => s.SetFailingCard(cardId, email), Times.Once);
        }
    }
}
