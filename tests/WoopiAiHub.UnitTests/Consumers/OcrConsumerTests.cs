using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
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
        private readonly DocumentEmbeddingsDataDto _documentEmbeddingsDataDto;
        private readonly Mock<IDocumentServices> _documentServices;
        private readonly Mock<IDocumentPipelineServices> _documentPipelineServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IAutomationServices> _automationServices;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;
        private readonly Mock<IFailingCardService> _failingCardServiceMock;
        private readonly Mock<IMessagePublisher<DocumentEmbeddingsDataDto>> _publisherMock;
        private readonly Mock<IMessageConsumer<ProcessOcrResultDto>> _consumerMock;
        private readonly Mock<ILogger<OcrConsumer>> _loggerMock;

        public OcrConsumerTests()
        {
            _mocker = new AutoMocker();

            _documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
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
            _documentPipelineServices = new Mock<IDocumentPipelineServices>();
            _automationServices = new Mock<IAutomationServices>();
            _usageDailyServices = new Mock<IUsageDailyServices>();

            _tenantCacheServices = new Mock<ITenantCacheServices>();
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                             .ReturnsAsync(tenant);

            _failingCardServiceMock = new Mock<IFailingCardService>();
            _failingCardServiceMock.Setup(f => f.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDocumentServices)))
                               .Returns(_documentServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDocumentPipelineServices)))
                               .Returns(_documentPipelineServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAutomationServices)))
                               .Returns(_automationServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IFailingCardService)))
                               .Returns(_failingCardServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            _publisherMock = new Mock<IMessagePublisher<DocumentEmbeddingsDataDto>>();
            _publisherMock.Setup(p => p.PublishAsync("embeddingQueue", _documentEmbeddingsDataDto))
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
        public async Task OcrConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            _documentPipelineServices
                .Setup(x => x.ProcessOcrResult(It.IsAny<ProcessOcrResultDto>()))
                .ReturnsAsync(new MetaDataAutomationDto());

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
            _documentPipelineServices.Verify(x => x.ProcessOcrResult(_processOcrResultDto), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "OcrConsumer unit tests")]
        public async Task OcrConsumer_ConsumeAsync_ShouldCatchException_WhenExtractingOcr()
        {
            // Arrange
            var exceptionEsperada = new Exception("Error processing OCR message for Embeddings");

            _documentPipelineServices
                .SetupSequence(x => x.ProcessOcrResult(It.IsAny<ProcessOcrResultDto>()))
                .ThrowsAsync(exceptionEsperada)
                .ReturnsAsync(new MetaDataAutomationDto { CardId = 10, StepToolId = 1 });

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
            _documentPipelineServices.Verify(x => x.ProcessOcrResult(_processOcrResultDto), Times.Exactly(2));
            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<DocumentEmbeddingsDataDto>()), Times.Never);
            _failingCardServiceMock.Verify(x => x.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()), Times.Once);

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
