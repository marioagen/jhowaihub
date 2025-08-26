using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class DocumentEmbeddingsConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly DocumentEmbeddingsResultDto _documentEmbeddingsResultDto;
        private readonly Mock<IDocumentServices> _documentServices;
        private readonly Mock<IMessageConsumer<DocumentEmbeddingsResultDto>> _consumerMock;
        private readonly Mock<ILogger<DocumentEmbeddingsConsumer>> _loggerMock;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;

        public DocumentEmbeddingsConsumerTests()
        {
            _mocker = new AutoMocker();
            _documentEmbeddingsResultDto = MessagingFixture.FindValidDocumentEmbeddingsResultDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                EmbeddingQueueAiHubResponse = "embeddingQueueAiHubResponse"
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
            _documentServices = new Mock<IDocumentServices>();

            _tenantCacheServices = new Mock<ITenantCacheServices>();
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                             .ReturnsAsync(tenant);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDocumentServices)))
                                   .Returns(_documentServices.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDocumentServices)))
                               .Returns(_documentServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            _consumerMock = new Mock<IMessageConsumer<DocumentEmbeddingsResultDto>>();
            _loggerMock = new Mock<ILogger<DocumentEmbeddingsConsumer>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumeAsync", "DocumentEmbeddingsConsumer unit tests")]
        public async Task DocumentEmbeddingsConsumer_ConsumerAsync_ShouldConsumeMessage()
        {
            // Arrange
            var documentId = 1;
            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<DocumentEmbeddingsResultDto, Task>>()))
                         .Callback<string, Func<DocumentEmbeddingsResultDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_documentEmbeddingsResultDto);
                         })
                         .Returns(Task.CompletedTask);

            _documentServices.Setup(x => x.ProcessEmbeddingsResult(_documentEmbeddingsResultDto))
                             .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<DocumentEmbeddingsConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _documentServices.Verify(x => x.ProcessEmbeddingsResult(_documentEmbeddingsResultDto), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumeAsync", "DocumentEmbeddingsConsumer unit tests")]
        public async Task DocumentEmbeddingsConsumer_ConsumerAsync_ShouldCatchException_WhenProcessingResponse()
        {
            // Arrange
            var exceptionExpected = new Exception("Error processing Embeddings response");

            _documentServices
                .Setup(x => x.ProcessEmbeddingsResult(It.IsAny<DocumentEmbeddingsResultDto>()))
                .ThrowsAsync(exceptionExpected);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<DocumentEmbeddingsResultDto, Task>>()))

                         .Callback<string, Func<DocumentEmbeddingsResultDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_documentEmbeddingsResultDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<DocumentEmbeddingsConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _documentServices.Verify(x => x.ProcessEmbeddingsResult(_documentEmbeddingsResultDto), Times.Once);

            _loggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exceptionExpected,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
