using Microsoft.Extensions.Options;
using Moq.AutoMock;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.UnitTests.Fixture
{
    public static class PromptHandlerFixture
    {
        public static (PromptHandler Handler, AutoMocker Mocker) CreatePromptHandlerWithMcpSettings(McpSettings mcpSettings)
        {
            var mocker = new AutoMocker();
            var messageQueues = new MessageQueues
            {
                OpenAiResponseQueueAiHubResponse = "test-queue",
                OpenAiResponseQueue = "test-queue2",
            };
            mocker.Use<IOptions<MessageQueues>>(Options.Create(messageQueues));
            mocker.Use<IOptions<OpenAiSettings>>(Options.Create(new OpenAiSettings
            {
                Temperature = 0,
                Model = "gpt-4",
                ApiVersion = "",
            }));
            mocker.Use<IOptions<McpSettings>>(Options.Create(mcpSettings));
            return (mocker.CreateInstance<PromptHandler>(), mocker);
        }
    }
}
