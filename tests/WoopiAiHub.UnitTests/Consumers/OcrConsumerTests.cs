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
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class OcrConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly ProcessOcrResultDto _processOcrResultDto;
        private readonly IEnumerable<DocumentEmbeddingsAddDto> _documentEmbeddingsAddDto;
        private readonly Mock<IDocumentServices> _documentServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessagePublisher<IEnumerable<DocumentEmbeddingsAddDto>>> _publisherMock;
        private readonly Mock<IMessageConsumer<ProcessOcrResultDto>> _consumerMock;
        private readonly Mock<ILogger<OcrConsumer>> _loggerMock;

        public OcrConsumerTests()
        {
            _mocker = new AutoMocker();

            _documentEmbeddingsAddDto = MessagingFixture.FindValidDocumentEmbeddingsAddDto();
            _processOcrResultDto = MessagingFixture.FindValidProcessOcrResultDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                OcrQueueAiHubResponse = "ocrQueueesponse",
                EmbeddingQueue = "embeddingQueue"
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

            _publisherMock = new Mock<IMessagePublisher<IEnumerable<DocumentEmbeddingsAddDto>>>();
            _publisherMock.Setup(p => p.PublishAsync("embeddingQueue", _documentEmbeddingsAddDto))
                          .Returns(Task.CompletedTask);
            _loggerMock = new Mock<ILogger<OcrConsumer>>();
            _mocker.Use(_publisherMock.Object);
            _consumerMock = new Mock<IMessageConsumer<ProcessOcrResultDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumerAsync", "OcrConsumer unit tests")]
        public async Task OcrResponseConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ProcessOcrResultDto, Task>>()))
                         .Callback<string, Func<ProcessOcrResultDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_processOcrResultDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<OcrConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _documentServices.Verify(x => x.ProcessOcrResult(_processOcrResultDto), Times.Once);
            _publisherMock.Verify(x => x.PublishAsync("embeddingQueue", It.IsAny<IEnumerable<DocumentEmbeddingsAddDto>>()), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "OcrConsumer unit tests")]
        public async Task OcrResponseConsumer_ConsumeAsync_ShouldCatchException_WhenExtractingOcr()
        {
            // Arrange
            var exceptionEsperada = new Exception("Error processing OCR message for Embeddings");

            _documentServices
                .Setup(x => x.ProcessOcrResult(It.IsAny<ProcessOcrResultDto>()))
                .ThrowsAsync(exceptionEsperada);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ProcessOcrResultDto, Task>>()))

                         .Callback<string, Func<ProcessOcrResultDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_processOcrResultDto);
                         })
                         .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<OcrConsumer>>();
            _mocker.Use(loggerMock);

            var consumer = _mocker.CreateInstance<OcrConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _documentServices.Verify(x => x.ProcessOcrResult(_processOcrResultDto), Times.Once);
            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IEnumerable<DocumentEmbeddingsAddDto>>()), Times.Never);

            loggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exceptionEsperada,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
