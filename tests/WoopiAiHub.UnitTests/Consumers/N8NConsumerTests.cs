using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Response.Automation;
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
    public class N8NConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IN8NServices> _n8NServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<AutomationOutputDto>> _consumerMock;
        private readonly Mock<ILogger<N8NConsumer>> _loggerMock;
        private readonly Mock<IAutomationServices> _automationServices;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;

        public N8NConsumerTests()
        {
            _mocker = new AutoMocker();

            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                AutomationQueueConsumer = "AutomationQueueConsumer",
                AutomationQueueResponse = "AutomationQueueResponse"
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
            _n8NServices = new Mock<IN8NServices>();
            _automationServices = new Mock<IAutomationServices>();
            _usageDailyServices = new Mock<IUsageDailyServices>();

            _tenantCacheServices = new Mock<ITenantCacheServices>();
            _tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                                .ReturnsAsync(tenant);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IN8NServices)))
                               .Returns(_n8NServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAutomationServices)))
                               .Returns(_automationServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupProperty(x => x.HttpContext, null);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IN8NServices)))
                               .Returns(_n8NServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            _loggerMock = new Mock<ILogger<N8NConsumer>>();

            _consumerMock = new Mock<IMessageConsumer<AutomationOutputDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("N8NConsumer", "N8NConsumer unit tests")]
        public async Task N8NConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var automationOutputDto = MessagingFixture.FindValidAutomationOutputDto();
            _n8NServices
                .Setup(x => x.ProcessMessage(It.IsAny<AutomationOutputDto>()))
                .ReturnsAsync(automationServicesDto);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<AutomationOutputDto, Task>>()))
                         .Callback<string, Func<AutomationOutputDto, Task>>(async (queue, callback) =>
                         {
                             await callback(automationOutputDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<N8NConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _n8NServices.Verify(x => x.ProcessMessage(It.IsAny<AutomationOutputDto>()), Times.Once);
        }
    }
}
