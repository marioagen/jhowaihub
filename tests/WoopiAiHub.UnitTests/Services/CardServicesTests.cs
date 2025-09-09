using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
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
        private readonly CardServices _cardServices;

        public CardServicesTests()
        {
            _mocker = new AutoMocker();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            _stepRepositoryMock = _mocker.GetMock<IStepRepository>();
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
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                                                                            updateDto.WorkflowId)).ReturnsAsync((Step?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateStepAndStatus(updateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StepLabel.NotFound, exception.LabelError);
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
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                                                                            updateDto.WorkflowId)).ReturnsAsync(step);

            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
        }

        [Fact(DisplayName = "Tests update AssignedUser when card not found and throws AppException")]
        [Trait("UpdateAssignedUser", "Fail")]
        public async Task UpdateAssignedUser_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto(null);
            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateAssignedUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update AssignedUser to null when UserId is null")]
        [Trait("UpdateAssignedUser", "Sucess")]
        public async Task UpdateAssignedUser_UserIdIsNull_UpdatesAssignedUserToNull()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto(null);

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var result = await _cardServices.UpdateAssignedUser(updateAssignedUserDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
            Assert.Null(card.AssignedUserId);
        }

        [Fact(DisplayName = "Tests update AssignedUser when User not in Team and throws AppException")]
        [Trait("UpdateAssignedUser", "Fail")]
        public async Task UpdateAssignedUser_UserNotInTeam_ThrowsAppException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1,DateTime.Now, 1, "Step", 1, 1, 1);
            card.Step.Workflow = WorkflowFixture.FindValidWorkflow();
            card.Step.Workflow.Team = new Team("Team", 1, DateTime.Now);
            card.Step.Workflow.Team.Users = new List<User>();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto(Guid.NewGuid());

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync(card);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateAssignedUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("User not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update AssignedUser when UserId is valid")]
        [Trait("UpdateAssignedUser", "Sucess")]
        public async Task UpdateAssignedUser_ValidUser_UpdatesAssignedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1);
            card.Step.Workflow = WorkflowFixture.FindValidWorkflow();
            card.Step.Workflow.Team = new Team("Team", 1, DateTime.Now);
            card.Step.Workflow.Team.Users = new List<User>() { new User(userId, "User","user@user.com", true, DateTime.Now) };
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto(userId);

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var result = await _cardServices.UpdateAssignedUser(updateAssignedUserDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
            Assert.Equal(userId, card.AssignedUserId);
        }
    }
}
