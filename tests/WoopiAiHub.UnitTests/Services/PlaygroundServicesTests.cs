using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class PlaygroundServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly PlaygroundServices _playgroundServices;

        public PlaygroundServicesTests()
        {
            _mocker = new AutoMocker();
            var mockChatSettings = new Mock<IOptions<ChatCompletionSettings>>();
            mockChatSettings.Setup(x => x.Value).Returns(new ChatCompletionSettings
            {
                Model = "model",
                ApiVersion = "v1",
                MaxTokens = 100,
                Temperature = 0.5f
            });
            _mocker.Use(mockChatSettings);
            _playgroundServices = _mocker.CreateInstance<PlaygroundServices>();
        }

        [Fact(DisplayName = "TestPromptWithContextAsync success and logs token usage")]
        [Trait("TestPromptWithContext", "Success")]
        public async Task TestPromptWithContextAsync_Success_LogsTokens()
        {
            var promptText = "Extraia os pontos principais";
            var contextText = "Texto do PDF de exemplo";
            var tenantId = "tenant-id";
            var email = "user@test.com";
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "gateway-key" };
            ChatCompletionDto? capturedDto = null;
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = "Resposta da IA" } }
                },
                Usage = new ChatUsageDto { TotalTokens = 42 }
            };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(tenantId)).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
                .Callback((string _, string _, string _, string _, ChatCompletionDto dto) => capturedDto = dto)
                .ReturnsAsync(chatCompletionResponse);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(u => u.AddByValuesAsync(MetricNames.Token, email, 42, "model", null, It.IsAny<UsageDailyOrigin>()))
                .ReturnsAsync(true);

            var result = await _playgroundServices.TestPromptWithContextAsync(promptText, contextText, tenantId, email);

            Assert.Equal("Resposta da IA", result);
            Assert.NotNull(capturedDto);
            Assert.Single(capturedDto!.Messages);
            Assert.Equal("system", capturedDto.Messages[0].Role);
            Assert.Equal(
                "Baseado no: \"Texto do PDF de exemplo\" e seguindo as orientações a seguir: Extraia os pontos principais",
                capturedDto.Messages[0].Content);

            _mocker.GetMock<IUsageDailyServices>().Verify(
                u => u.AddByValuesAsync(MetricNames.Token, email, 42, "model", null, It.IsAny<UsageDailyOrigin>()),
                Times.Once);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw ArgumentNullException when prompt text is null")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowArgumentNullException_WhenPromptTextNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _playgroundServices.TestPromptWithContextAsync(null!, "ctx", "tenant", "email@test.com"));
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw ArgumentNullException when context text is null")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowArgumentNullException_WhenContextTextNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _playgroundServices.TestPromptWithContextAsync("prompt", null!, "tenant", "email@test.com"));
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw ArgumentException when prompt text is whitespace")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowArgumentException_WhenPromptTextWhitespace()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _playgroundServices.TestPromptWithContextAsync("   ", "ctx", "tenant", "email@test.com"));

            Assert.Equal("Prompt text is required", exception.Message);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw argument exception when tenant gateway is invalid")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowArgumentException_InvalidTenantInfo()
        {
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = null, AiGatewayKey = string.Empty };
            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync("tenant")).ReturnsAsync(tenantInfo);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _playgroundServices.TestPromptWithContextAsync("prompt", "ctx", "tenant", "email@test.com"));

            Assert.Equal("AiGateway ApplicationId not found", exception.Message);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw app exception when AI returns empty content")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowAppException_WhenAiContentEmpty()
        {
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = string.Empty } }
                },
                Usage = new ChatUsageDto { TotalTokens = 10 }
            };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
                .ReturnsAsync(chatCompletionResponse);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(u => u.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<UsageDailyOrigin>()))
                .ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<AppException>(async () =>
                await _playgroundServices.TestPromptWithContextAsync("p", "c", "t", "e@mail.com"));

            Assert.Equal("Empty response from AI Gateway", exception.Message);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync logs zero tokens when usage is missing")]
        [Trait("TestPromptWithContext", "Success")]
        public async Task TestPromptWithContextAsync_LogsZeroTokens_WhenUsageMissing()
        {
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = "Ok" } }
                },
                Usage = null!
            };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync("t")).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
                .ReturnsAsync(chatCompletionResponse);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(u => u.AddByValuesAsync(MetricNames.Token, "e@mail.com", 0, "model", null, It.IsAny<UsageDailyOrigin>()))
                .ReturnsAsync(true);

            var result = await _playgroundServices.TestPromptWithContextAsync("prompt", "", "t", "e@mail.com");

            Assert.Equal("Ok", result);
            _mocker.GetMock<IUsageDailyServices>().Verify(
                u => u.AddByValuesAsync(MetricNames.Token, "e@mail.com", 0, "model", null, It.IsAny<UsageDailyOrigin>()),
                Times.Once);
        }
    }
}
