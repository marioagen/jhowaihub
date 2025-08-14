using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
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
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IStepRepository> _stepRepositoryMock;
        private readonly Mock<IStatusRepository> _statusRepositoryMock;
        private readonly CardServices _cardServices;

        public CardServicesTests()
        {
            _mocker = new AutoMocker();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            _stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            _statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            _cardServices = _mocker.CreateInstance<CardServices>();
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException when Card not found")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateStepAndStatus(updateDto));
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
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateStepAndStatus(updateDto));
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
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateStepAndStatus(updateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StatusLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Tests update Step and Status and returns true")]
        [Trait("UpdateStepAndStatus", "Success")]
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
            var result = await _cardServices.UpdateStepAndStatus(updateDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
        }
    }
}
