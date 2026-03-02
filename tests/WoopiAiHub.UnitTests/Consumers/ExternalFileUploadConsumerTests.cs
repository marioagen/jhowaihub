using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Messaging;
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
    public class ExternalFileUploadConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly ExternalFileUploadDto _externalFileUploadDto;
        private readonly Mock<IExternalFileUploadServices> _externalFileUploadServices;
        private readonly Mock<IMessageConsumer<ExternalFileUploadDto>> _consumerMock;
        private readonly Mock<ILogger<ExternalFileUploadConsumer>> _loggerMock;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;

        public ExternalFileUploadConsumerTests()
        {
            _mocker = new AutoMocker();
            _externalFileUploadDto = MessagingFixture.FindValidExternalFileUploadDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                ExternalFileUploadQueue = "externalFileUploadQueue"
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

            _externalFileUploadServices = new Mock<IExternalFileUploadServices>();
            _tenantCacheServices = new Mock<ITenantCacheServices>();
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                             .ReturnsAsync(tenant);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IExternalFileUploadServices)))
                               .Returns(_externalFileUploadServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            _consumerMock = new Mock<IMessageConsumer<ExternalFileUploadDto>>();
            _loggerMock = new Mock<ILogger<ExternalFileUploadConsumer>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumeAsync", "ExternalFileUploadConsumer unit tests")]
        public async Task ExternalFileUploadConsumer_ConsumerAsync_ShouldConsumeMessage()
        {
            // Arrange
            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ExternalFileUploadDto, Task>>()))
                         .Callback<string, Func<ExternalFileUploadDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_externalFileUploadDto);
                         })
                         .Returns(Task.CompletedTask);

            _externalFileUploadServices.Setup(x => x.ProcessExternalFileUpload(_externalFileUploadDto))
                                       .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ExternalFileUploadConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _externalFileUploadServices.Verify(x => x.ProcessExternalFileUpload(_externalFileUploadDto), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumeAsync", "ExternalFileUploadConsumer unit tests")]
        public async Task ExternalFileUploadConsumer_ConsumerAsync_ShouldCatchException_WhenProcessingResponse()
        {
            // Arrange
            var exceptionExpected = new Exception("Failed to process the answer response.");

            _externalFileUploadServices
                .Setup(x => x.ProcessExternalFileUpload(It.IsAny<ExternalFileUploadDto>()))
                .ThrowsAsync(exceptionExpected);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<ExternalFileUploadDto, Task>>()))
                         .Callback<string, Func<ExternalFileUploadDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_externalFileUploadDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<ExternalFileUploadConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _externalFileUploadServices.Verify(x => x.ProcessExternalFileUpload(_externalFileUploadDto), Times.Once);

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
