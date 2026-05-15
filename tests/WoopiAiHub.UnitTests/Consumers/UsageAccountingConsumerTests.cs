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
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class UsageAccountingConsumerTests
    {
        private const string QueueName = "usageAccountingQueue";

        private readonly AutoMocker _mocker;
        private readonly UsageAccountingDto _usageAccountingDto;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<UsageAccountingDto>> _consumerMock;
        private readonly Mock<ILogger<UsageAccountingConsumer>> _loggerMock;

        public UsageAccountingConsumerTests()
        {
            _mocker = new AutoMocker();

            _usageAccountingDto = MessagingFixture.FindValidUsageAccountingDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                UsageAccountingQueue = QueueName,
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

            _usageDailyServices = new Mock<IUsageDailyServices>();

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

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            _loggerMock = new Mock<ILogger<UsageAccountingConsumer>>();
            _consumerMock = new Mock<IMessageConsumer<UsageAccountingDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the usage accounting message and persist via UsageDailyServices")]
        [Trait("ConsumerAsync", "UsageAccountingConsumer unit tests")]
        public async Task UsageAccountingConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<UsageDailyOrigin>()))
                .ReturnsAsync(true);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<UsageAccountingDto, Task>>()))
                         .Callback<string, Func<UsageAccountingDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_usageAccountingDto);
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<UsageAccountingConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _usageDailyServices.Verify(x => x.AddByValuesAsync(
                _usageAccountingDto.UsageTypeName,
                _usageAccountingDto.Email,
                _usageAccountingDto.Count,
                _usageAccountingDto.ModelEmbeddingName!,
                _usageAccountingDto.WorkflowId,
                _usageAccountingDto.Origin), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when AddByValuesAsync throws and log it")]
        [Trait("ConsumerAsync", "UsageAccountingConsumer unit tests")]
        public async Task UsageAccountingConsumer_ConsumeAsync_ShouldCatchException_WhenAddByValuesThrows()
        {
            // Arrange
            var expectedException = new InvalidOperationException("UsageDaily failed");

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<UsageDailyOrigin>()))
                .ThrowsAsync(expectedException);

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<UsageAccountingDto, Task>>()))
                         .Callback<string, Func<UsageAccountingDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_usageAccountingDto);
                         })
                         .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<UsageAccountingConsumer>>();
            _mocker.Use(loggerMock);

            var consumer = _mocker.CreateInstance<UsageAccountingConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _usageDailyServices.Verify(x => x.AddByValuesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<UsageDailyOrigin>()), Times.Once);

            loggerMock.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Must set HttpContext TenantConnection when processing message")]
        [Trait("ConsumerAsync", "UsageAccountingConsumer unit tests")]
        public async Task UsageAccountingConsumer_ConsumeAsync_ShouldSetTenantConnection()
        {
            // Arrange
            string? capturedConnectionString = null;

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContext);

            var tenantInfo = new TenantInfoDto
            {
                DatabaseName = "test-tenant-db",
                Email = "test@example.com",
                Name = "test-tenant"
            };

            var tenantCacheServicesMock = new Mock<ITenantCacheServices>();
            tenantCacheServicesMock.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                                   .ReturnsAsync(tenantInfo);

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<UsageDailyOrigin>()))
                .ReturnsAsync(true)
                .Callback(() =>
                {
                    capturedConnectionString = httpContext.Items["TenantConnection"] as string;
                });

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<UsageAccountingDto, Task>>()))
                         .Callback<string, Func<UsageAccountingDto, Task>>(async (queue, callback) =>
                         {
                             await callback(_usageAccountingDto);
                         })
                         .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                               .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                               .Returns(tenantCacheServicesMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                               .Returns(httpContextAccessorMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _mocker.Use(scopeFactoryMock.Object);

            var consumer = _mocker.CreateInstance<UsageAccountingConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(capturedConnectionString);
            Assert.Contains("test-tenant-db", capturedConnectionString);
        }

        [Fact(DisplayName = "Must subscribe using the queue name from MessageQueues options")]
        [Trait("ConsumerAsync", "UsageAccountingConsumer unit tests")]
        public async Task UsageAccountingConsumer_ConsumeAsync_ShouldUseQueueFromOptions()
        {
            // Arrange
            string? capturedQueue = null;

            _consumerMock.Setup(x => x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<UsageAccountingDto, Task>>()))
                         .Callback<string, Func<UsageAccountingDto, Task>>((queue, _) =>
                         {
                             capturedQueue = queue;
                         })
                         .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<UsageAccountingConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            Assert.Equal(QueueName, capturedQueue);
            _consumerMock.Verify(x => x.ConsumerAsync(QueueName, It.IsAny<Func<UsageAccountingDto, Task>>()), Times.Once);
        }
    }
}
