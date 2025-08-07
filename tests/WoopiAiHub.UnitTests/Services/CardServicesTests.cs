using Moq;
using System.Runtime.CompilerServices;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(CardCollection))]
    public class CardServicesTests
    {
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IStepRepository> _stepRepositoryMock;
        private readonly Mock<IStatusRepository> _statusRepositoryMock;
        private readonly CardServices _cardService;
        private readonly CardFixture _cardFixture;

        public CardServicesTests(CardFixture cardFixture)
        {
            _cardFixture = cardFixture;
            _cardRepositoryMock = new Mock<ICardRepository>();
            _stepRepositoryMock = new Mock<IStepRepository>();
            _statusRepositoryMock = new Mock<IStatusRepository>();
            _cardService = new CardServices(_cardRepositoryMock.Object, _stepRepositoryMock.Object, _statusRepositoryMock.Object);
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException when Card not found")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardService.UpdateStepAndStatus(updateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException when step not found")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_StepNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(updateDto.StepId)).ReturnsAsync((Step?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardService.UpdateStepAndStatus(updateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StepLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_StatusNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(updateDto.StepId)).ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindById(updateDto.StatusId)).ReturnsAsync((Status?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardService.UpdateStepAndStatus(updateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StatusLabel.NotFound, exception.LabelError);
        }

        [Fact]
        public async Task UpdateStepAndStatus_ValidInputs_UpdatesCardAndReturnsTrue()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(updateDto.StepId)).ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindById(updateDto.StatusId)).ReturnsAsync(status);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var result = await _cardService.UpdateStepAndStatus(updateDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
        }
    }
}
