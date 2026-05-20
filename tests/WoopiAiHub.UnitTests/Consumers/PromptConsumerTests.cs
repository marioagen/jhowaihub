using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.UnitTests.Fixture;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
using WoopiAiHub.Application.Utils;

namespace WoopiAiHub.UnitTests.Consumers
{
    [Collection(nameof(MessagingCollection))]
    public class PromptConsumerTests
    {
        private readonly AutoMocker _mocker;
        private readonly ChatCompletionResponseDto _chatCompletionResponseDto;
        private readonly OpenAiResponseConsumerResponseDto _openAiResponseConsumerResponseDto;
        private readonly Mock<IDocumentServices> _documentServices;
        private readonly Mock<IPromptServices> _promptServices;
        private readonly Mock<ITenantCacheServices> _tenantCacheServices;
        private readonly Mock<IMessageConsumer<OpenAiResponseConsumerResponseDto>> _consumerMock;
        private readonly Mock<ILogger<PromptConsumer>> _loggerMock;
        private readonly Mock<IUsageDailyServices> _usageDailyServices;
        private readonly Mock<IAutomationServices> _automationServices;

        public PromptConsumerTests()
        {
            _mocker = new AutoMocker();

            _chatCompletionResponseDto = MessagingFixture.FindValidChatCompletionResponseDto();
            _openAiResponseConsumerResponseDto = MessagingFixture.FindValidOpenAiResponseConsumerResponseDto();
            var tenant = MessagingFixture.FindValidTenantInfoDto();

            var messageQueues = Options.Create(new MessageQueues
            {
                ChatCompletionQueueAiHubResponse = "chatCompletionQueueAiHubResponse",
            });

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
            var chatCompletionSettings = Options.Create(new ChatCompletionSettings { Model = "gpt-test-model", });
            _mocker.Use<IConfiguration>(configuration);
            _mocker.Use<IOptions<MessageQueues>>(messageQueues);
            _mocker.Use<IOptions<ChatCompletionSettings>>(chatCompletionSettings);
            _documentServices = new Mock<IDocumentServices>();
            _promptServices = new Mock<IPromptServices>();
            _usageDailyServices = new Mock<IUsageDailyServices>();
            _automationServices = new Mock<IAutomationServices>();

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
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPromptServices)))
                .Returns(_promptServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantCacheServices)))
                .Returns(_tenantCacheServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
                .Returns(httpContextAccessorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUsageDailyServices)))
                .Returns(_usageDailyServices.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAutomationServices)))
                .Returns(_automationServices.Object);

            _loggerMock = new Mock<ILogger<PromptConsumer>>();
            _consumerMock = new Mock<IMessageConsumer<OpenAiResponseConsumerResponseDto>>();
            _mocker.Use(_consumerMock.Object);
            _mocker.Use(_loggerMock.Object);
            _mocker.Use(httpContextAccessorMock.Object);
        }

        [Fact(DisplayName = "It must consume the response and process it successfully")]
        [Trait("ConsumerAsync", "PromptConsumer unit tests")]
        public async Task PromptConsumer_ConsumeAsync_ShouldConsumeMessage()
        {
            // Arrange
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();

            _promptServices
                .Setup(x => x.ProcessOpenAiResponseResult(It.IsAny<OpenAiResponseConsumerResponseDto>()))
                .ReturnsAsync(stepToolExecution);

            _usageDailyServices
                .Setup(x => x.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), null, It.IsAny<UsageDailyOrigin>()))
                .ReturnsAsync(true);

            _consumerMock.Setup(x =>
                    x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<OpenAiResponseConsumerResponseDto, Task>>()))
                .Callback<string, Func<OpenAiResponseConsumerResponseDto, Task>>(async (queue, callback) =>
                {
                    await callback(_openAiResponseConsumerResponseDto);
                })
                .Returns(Task.CompletedTask);

            var consumer = _mocker.CreateInstance<PromptConsumer>();

            // Act
            await consumer.StartAsync(CancellationToken.None);

            // Assert
            _promptServices.Verify(x => x.ProcessOpenAiResponseResult(_openAiResponseConsumerResponseDto), Times.Once);
        }

        [Fact(DisplayName = "Must catch exception when processing response")]
        [Trait("ConsumerAsync", "PromptConsumer unit tests")]
        public async Task PromptConsumer_ConsumeAsync_ShouldCatchException_WhenExtractingOcr()
        {
            // Arrange
            var exceptionEsperada = new ArgumentException("StepToolExecution not found");

            _promptServices
                .Setup(x => x.ProcessOpenAiResponseResult(It.IsAny<OpenAiResponseConsumerResponseDto>()))
                .ThrowsAsync(exceptionEsperada);

            _consumerMock.Setup(x =>
                    x.ConsumerAsync(It.IsAny<string>(), It.IsAny<Func<OpenAiResponseConsumerResponseDto, Task>>()))
                .Callback<string, Func<OpenAiResponseConsumerResponseDto, Task>>(async (queue, callback) =>
                {
                    await callback(_openAiResponseConsumerResponseDto);
                })
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<PromptConsumer>>();
            _mocker.Use(loggerMock);

            var consumer = _mocker.CreateInstance<PromptConsumer>();

            // Act
            var exception = await Record.ExceptionAsync(() => consumer.StartAsync(CancellationToken.None));

            // Assert
            Assert.Null(exception);
            _promptServices.Verify(x => x.ProcessOpenAiResponseResult(_openAiResponseConsumerResponseDto), Times.Once);

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
