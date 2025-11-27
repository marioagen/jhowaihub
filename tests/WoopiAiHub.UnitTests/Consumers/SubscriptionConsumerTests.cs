using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Repository.Cache;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class SubscriptionConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly TenantSubscriptionDto _tenantSubscriptionDto;
        private readonly Mock<ITenantServices> _tenantServices;
        private readonly Mock<IMessageConsumer<TenantSubscriptionDto>> _consumerMock;
        private readonly Mock<ILogger<SubscriptionConsumer>> _loggerMock;

        public SubscriptionConsumerTests()
        {
            _mocker = new AutoMocker();
            _tenantSubscriptionDto = MessagingFixture.FindValidTenantSubscriptionDto();
            var messageQueues = Options.Create(new MessageQueues
            {
                MarketplaceSubscriptionQueue = "MarketplaceSubscriptionQueue"
            });
            _mocker.Use<IOptions<MessageQueues>>(messageQueues);

            _tenantServices = new Mock<ITenantServices>();
            _tenantServices.Setup(x => x.ProcessSubscription(It.IsAny<TenantSubscriptionDto>()));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantServices)))
                               .Returns(_tenantServices.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantServices)))
                               .Returns(_tenantServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);


            _loggerMock = new Mock<ILogger<SubscriptionConsumer>>();
            _mocker.Use(_loggerMock.Object);
            _consumerMock = new Mock<IMessageConsumer<TenantSubscriptionDto>>();
            _mocker.Use(_consumerMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumerAsync", "SubscriptionConsumer unit tests")]
        public async Task OcrConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<TenantSubscriptionDto, Task>>()))
                         .Callback<string, Func<TenantSubscriptionDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_tenantSubscriptionDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<SubscriptionConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _tenantServices.Verify(x => x.ProcessSubscription(_tenantSubscriptionDto), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "SubscriptionConsumer unit tests")]
        public async Task OcrConsumer_ConsumeAsync_ShouldCatchException_WhenExtractingOcr()
        {
            // Arrange
            var exceptionEsperada = new Exception($"Error processing marketplace activation message for Tenant: {_tenantSubscriptionDto.Name}");

            _tenantServices
                .Setup(x => x.ProcessSubscription(It.IsAny<TenantSubscriptionDto>()))
                .Throws(exceptionEsperada);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<TenantSubscriptionDto, Task>>()))

                         .Callback<string, Func<TenantSubscriptionDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_tenantSubscriptionDto);
                         })
                         .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SubscriptionConsumer>>();
            _mocker.Use(loggerMock);

            var consumer = _mocker.CreateInstance<SubscriptionConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
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
