using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(CardCollection))]
    public class FailingCardServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly FailingCardService _failingCardService;

        public FailingCardServiceTests()
        {
            _mocker = new AutoMocker();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();

            _failingCardService = _mocker.CreateInstance<FailingCardService>();
        }

        [Fact(DisplayName = "SetFailingCard should throw AppException when card not found")]
        [Trait("SetFailingCard", "Failure")]
        public async Task SetFailingCard_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _failingCardService.SetFailingCard(cardId, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "SetFailingCard should throw AppException when fail status not found")]
        [Trait("SetFailingCard", "Failure")]
        public async Task SetFailingCard_FailStatusNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync((Status?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _failingCardService.SetFailingCard(cardId, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "SetFailingCard should update card status to fail")]
        [Trait("SetFailingCard", "Success")]
        public async Task SetFailingCard_Success_UpdatesCardStatusToFail()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            card.UpdateStatus(2);
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);
            unitOfWorkMock.Setup(u => u.BeginTransaction()).Callback(() => { });
            unitOfWorkMock.Setup(u => u.Commit()).Callback(() => { });
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>())).Returns(true);

            // Act
            await _failingCardService.SetFailingCard(cardId, email);

            // Assert
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "SetFailingCard should handle null email gracefully")]
        [Trait("SetFailingCard", "Success")]
        public async Task SetFailingCard_NullEmail_SucceedsWithoutNotification()
        {
            // Arrange
            var cardId = 1;
            string? email = null;
            var card = CardFixture.FindValidCard();
            card.UpdateStatus(2);
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);
            unitOfWorkMock.Setup(u => u.BeginTransaction()).Callback(() => { });
            unitOfWorkMock.Setup(u => u.Commit()).Callback(() => { });
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>())).Returns(true);

            // Act
            await _failingCardService.SetFailingCard(cardId, email);

            // Assert
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "SetFailingCard should rollback transaction on error")]
        [Trait("SetFailingCard", "Failure")]
        public async Task SetFailingCard_OnError_RollsBackTransaction()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            card.UpdateStatus(2);
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);
            unitOfWorkMock.Setup(u => u.BeginTransaction()).Callback(() => { });
            unitOfWorkMock.Setup(u => u.Rollback()).Callback(() => { });

            // Simulate error during update
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _failingCardService.SetFailingCard(cardId, email));

            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "SetFailingCard should return early if card is already failing")]
        [Trait("SetFailingCard", "Success")]
        public async Task SetFailingCard_CardAlreadyFailing_ReturnsEarly()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            card.UpdateStatus(failStatus.Id);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);

            // Act
            await _failingCardService.SetFailingCard(cardId, email);

            // Assert
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Never);
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Never);
        }
    }
}
