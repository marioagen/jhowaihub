using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Domain.Models;
using Xunit;
using WoopiAiHub.UnitTests.Fixture;

namespace WoopiAiHub.UnitTests.Messaging
{
    public class QuizConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly DocumentEmbeddingsQueryResponseDto _documentEmbeddingsQueryResponseDto;
        private readonly Mock<IDocumentServices> _documentServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<DocumentEmbeddingsQueryResponseDto>> _consumerMock;
        private readonly Mock<ILogger<QuizConsumer>> _loggerMock;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;

        public QuizConsumerTests()
        {
            _mocker = new AutoMocker();

            _documentEmbeddingsQueryResponseDto = MessagingFixture.FindValidDocumentEmbeddingsQueryResponseDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                ChatCompletionQueueAiHubResponse = "chatCompletionQueueAiHubResponse",
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
            _usageDailyServices = new Mock<IUsageDailyServices>();

            _tenantCacheServices = new Mock<ITenantCacheServices>();
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
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
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);

            _loggerMock = new Mock<ILogger<QuizConsumer>>();
            _consumerMock = new Mock<IMessageConsumer<DocumentEmbeddingsQueryResponseDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumerAsync", "QuizConsumer unit tests")]
        public async Task QuizConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            var document = new Document(
                "Doc",
                "Ref",
                "Link",
                Domain.Enum.DocumentStatus.ReadyForAnalysis,
                "email",
                1,
                new List<Workflow>(),
                DateTime.Now
               );
            _ = _documentServices
                .Setup(x => x.InputToolQuestionnaire(It.IsAny<DocumentEmbeddingsQueryResponseDto>()))
                .Returns(Task.FromResult<Document?>(document));

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<DocumentEmbeddingsQueryResponseDto, Task>>()))
                         .Callback<string, Func<DocumentEmbeddingsQueryResponseDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_documentEmbeddingsQueryResponseDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<QuizConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _documentServices.Verify(x => x.InputToolQuestionnaire(_documentEmbeddingsQueryResponseDto), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "QuizConsumer unit tests")]
        public async Task QuizConsumer_ConsumeAsync_ShouldCatchException_WhenProcessingResponse()
        {
            // Arrange
            var exceptionEsperada = new ArgumentException("StepToolExecution not found");

            _documentServices
               .Setup(x => x.InputToolQuestionnaire(It.IsAny<DocumentEmbeddingsQueryResponseDto>()))
               .ThrowsAsync(exceptionEsperada);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<DocumentEmbeddingsQueryResponseDto, Task>>()))
                         .Callback<string, Func<DocumentEmbeddingsQueryResponseDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_documentEmbeddingsQueryResponseDto);
                         })
                         .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<QuizConsumer>>();
            _mocker.Use(loggerMock);

            var consumer = _mocker.CreateInstance<QuizConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _documentServices.Verify(x => x.InputToolQuestionnaire(_documentEmbeddingsQueryResponseDto), Times.Once);

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
