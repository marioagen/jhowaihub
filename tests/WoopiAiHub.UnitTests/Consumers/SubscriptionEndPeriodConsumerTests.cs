using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.UnitTests.Fixture;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class SubscriptionEndPeriodConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly SubscriptionPeriodDto _subscriptionPeriodDto;
        private readonly Mock<ISubscriptionPeriodServices> _subscriptionPeriodServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<SubscriptionPeriodDto>> _consumerMock;
        private readonly Mock<ILogger<SubscriptionEndPeriodConsumer>> _loggerMock;

        public SubscriptionEndPeriodConsumerTests()
        {
            _mocker = new AutoMocker();
            _subscriptionPeriodDto = MessagingFixture.FindValidSubscriptionPeriodDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                MarketplaceEndSubscriptionPeriodQueue = "MarketplaceEndSubscriptionPeriodQueue"
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
            
            _subscriptionPeriodServices = new Mock<ISubscriptionPeriodServices>();
            _tenantCacheServices = new Mock<ITenantCacheServices>();
            
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                             .ReturnsAsync(tenant);

            var serviceProviderMock = new Mock<IServiceProvider>();
            
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ISubscriptionPeriodServices)))
                               .Returns(_subscriptionPeriodServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);


            _loggerMock = new Mock<ILogger<SubscriptionEndPeriodConsumer>>();
            _consumerMock = new Mock<IMessageConsumer<SubscriptionPeriodDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumerAsync", "SubscriptionEndPeriodConsumer unit tests")]
        public async Task SubscriptionEndPeriodConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<SubscriptionPeriodDto, Task>>()))
                         .Callback<string, Func<SubscriptionPeriodDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_subscriptionPeriodDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<SubscriptionEndPeriodConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _subscriptionPeriodServices.Verify(x => x.CreateAsync(_subscriptionPeriodDto.PeriodStart, _subscriptionPeriodDto.PeriodEnd, false), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "SubscriptionEndPeriodConsumer unit tests")]
        public async Task SubscriptionEndPeriodConsumer_ConsumeAsync_ShouldCatchException()
        {
            // Arrange
            var exceptionEsperada = new Exception("Error processing message");

            _subscriptionPeriodServices
                .Setup(x => x.CreateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
                .ThrowsAsync(exceptionEsperada);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<SubscriptionPeriodDto, Task>>()))
                         .Callback<string, Func<SubscriptionPeriodDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_subscriptionPeriodDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<SubscriptionEndPeriodConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _subscriptionPeriodServices.Verify(x => x.CreateAsync(_subscriptionPeriodDto.PeriodStart, _subscriptionPeriodDto.PeriodEnd, false), Times.Once);

            _loggerMock.Verify(x =>
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
