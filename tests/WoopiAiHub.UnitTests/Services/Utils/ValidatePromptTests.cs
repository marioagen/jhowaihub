using Moq;
using Moq.AutoMock;
using System;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Utils
{
    [Collection(nameof(PromptCollection))]
    public class ValidatePromptTests
    {
        private readonly AutoMocker _mocker;
        private readonly ValidatePrompt _validatePrompt;
        private readonly PromptFixture _fixture;

        public ValidatePromptTests(PromptFixture fixture)
        {
            _mocker = new AutoMocker();
            _validatePrompt = _mocker.CreateInstance<ValidatePrompt>();
            _fixture = fixture;
        }

        [Fact(DisplayName = "ValidateOwnership should throw KeyNotFoundException when prompt not found")]
        [Trait("ValidateOwnership", "Fail")]
        public void ValidateOwnership_PromptNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            int promptId = 1;
            string userEmail = "user@example.com";
            var guid = Guid.NewGuid();
            _mocker.GetMock<IUserServices>().
                Setup(s => s.FindIdByEmail(It.IsAny<string>()))
                       .Returns(Guid.NewGuid());
            _mocker.GetMock<IPromptRepository>()
                .Setup(repo => repo.FindById(promptId))
                .Returns((PromptDto?)null);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _validatePrompt.ValidateOwnership(promptId, userEmail));
            Assert.Equal($"Prompt with ID {promptId} not found.", exception.Message);
        }

        [Fact(DisplayName = "ValidateOwnership should throw ArgumentException when guid is empty")]
        [Trait("ValidateOwnership", "Fail")]
        public void ValidateOwnership_GuidEmpty_ThrowsArgumentException()
        {
            // Arrange
            int promptId = 1;
            string userEmail = "user@example.com";
            var prompt = PromptFixture.FindValidPromptDto();
            var mockRepo = _mocker.GetMock<IUserRepository>();
            var guid = Guid.NewGuid();
            _mocker.GetMock<IUserServices>().
                Setup(s => s.FindIdByEmail(It.IsAny<string>()))
                       .Returns(Guid.Empty);

            mockRepo.Setup(r => r.FindIdByEmail(userEmail)).Returns(guid);

            _mocker.GetMock<IPromptRepository>()
                .Setup(repo => repo.FindById(promptId))
                .Returns(prompt);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _validatePrompt.ValidateOwnership(promptId, userEmail));
        }

        [Fact(DisplayName = "ValidatePromptFields should throw AppException with RequiredField when required fields are empty")]
        [Trait("ValidatePromptFields", "Fail")]
        public void ValidatePromptFields_RequiredFieldEmpty_Fail()
        {
            //Arrange
            var prompt = PromptFixture.FindInvalidPrompt();

            // Act 
            var empty = _validatePrompt.ValidatePromptFields(prompt);

            //Assert
            Assert.False(empty);
        }

        [Fact(DisplayName = "ValidatePromptFields should throw AppException with RequiredField when required fields are empty")]
        [Trait("ValidatePromptFields", "Fail")]
        public void ValidatePromptFields_PromptValue_Success()
        {
            //Arrange
            var prompt = PromptFixture.FindValidPrompt();

            // Act 
            var result = _validatePrompt.ValidatePromptFields(prompt);

            //Assert
            Assert.True(result);
        }
    }
}
