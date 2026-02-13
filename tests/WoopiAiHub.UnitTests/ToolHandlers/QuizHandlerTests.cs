using Moq;
using Moq.AutoMock;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.UnitTests.Handlers
{
    public class QuizHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly QuizHandler _handler;

        public QuizHandlerTests()
        {
            _mocker = new AutoMocker();
            
            var messageQueues = new MessageQueues { AnswerQueue = "test-answer-queue" };
            _mocker.Use<IOptions<MessageQueues>>(Options.Create(messageQueues));

            var configMock = _mocker.GetMock<IConfiguration>();
            configMock.Setup(c => c["IndexerApiKey"]).Returns("test-api-key");

            _handler = _mocker.CreateInstance<QuizHandler>();
        }

        [Fact(DisplayName = "BuildPayload should return valid ExecutionMessageDto")]
        public async Task BuildPayload_ValidInput_ReturnsExecutionMessageDto()
        {
            // Arrange
            var automationDto = new AutomationServicesDto(1, 1, "tenant-test", "user@test.com", "ref-file", 1);
            var input = new StepToolParameter(1, System.DateTime.Now, 1, false, null, "123"); // 123 is the quizId
            var outputs = new List<StepToolOutput>();

            var tenantInfo = new TenantInfoDto {
                Name = "tenant-test",
                AiGatewayApplicationId = Guid.NewGuid(),
                AiGatewayKey = "key",
                Model = "gpt-4",
                KValue = 5,
                Template = "template {language}",
                RefineTemplate = "refine",
                MaxTokens = 1000,
                SearchMode = "hybrid" };
            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync("tenant-test")).ReturnsAsync(tenantInfo);

            var questionnaireDto = new QuestionnaireDto 
            { 
                Id = 123, 
                Questions = new[] { new Question("Q1", "user", 1, System.DateTime.Now) } 
            };
            _mocker.GetMock<IQuestionnaireServices>().Setup(s => s.FindById(123)).Returns(questionnaireDto);

            // Act
            var result = await _handler.BuildPayload(automationDto, input, outputs);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-answer-queue", result.Queue);
            var message = Assert.IsType<DocumentEmbeddingsQueryDto>(result.Message);
            Assert.Equal("ref-file", message.ReferenceFile);
            Assert.Equal("tenant-test", message.Tenant);
            Assert.Equal("test-api-key", message.KeyMongoAccess);
            Assert.Single(message.Questions);
            Assert.Equal(1, message.Questions.First().Id);
            Assert.Equal("Q1", message.Questions.First().Question);
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when AiGateway info is missing")]
        public async Task BuildPayload_MissingAiGatewayInfo_ThrowsArgumentException()
        {
            // Arrange
            var automationDto = new AutomationServicesDto(1, 1, "tenant-test", "user@test.com", "ref-file", 1);
            var input = new StepToolParameter(1, System.DateTime.Now, 1, false, null, "123");
            
            var tenantInfo = new TenantInfoDto { Name = "tenant-test", AiGatewayApplicationId = null }; // Missing ID
            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync("tenant-test")).ReturnsAsync(tenantInfo);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.ArgumentException>(() => _handler.BuildPayload(automationDto, input, null!));
            Assert.Equal("AiGateway ApplicationId not found", exception.Message);
        }
    }
}
