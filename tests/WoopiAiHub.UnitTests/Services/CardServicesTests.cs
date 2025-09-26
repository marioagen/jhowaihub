using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
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
        private readonly Mock<IStepToolRepository> _stepToolRepositoryMock;
        private readonly Mock<IAutomationServices> _automationServices;
        private readonly CardServices _cardServices;

        public CardServicesTests()
        {
            _mocker = new AutoMocker();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            _stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            _stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            _automationServices = _mocker.GetMock<IAutomationServices>();

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
            _automationServices.Setup(s => s.StartExecutionByCardAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                                                                            updateDto.WorkflowId)).ReturnsAsync(step);

            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);
            


            _stepToolRepositoryMock.Setup(repo=> repo.FindByStepIdAndOrderAsync(1,1)).ReturnsAsync(It.IsAny<StepTool>());

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
        }

        [Fact(DisplayName = "Tests update UnassignUser when card not found and throws AppException")]
        [Trait("UnassignUser", "Fail")]
        public async Task UnassignUser_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            _cardRepositoryMock.Setup(repo => repo.FindById(cardId))
                               .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UnassignUser(cardId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update UnassignUser to null successfully")]
        [Trait("UnassignUser", "Success")]
        public async Task UnassignUser_Success()
        {
            //Arrange
            var cardId = 1;
            var card = CardFixture.FindValidCard();

            _cardRepositoryMock.Setup(repo => repo.FindById(cardId))
                               .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            //Act
            var result = await _cardServices.UnassignUser(cardId);

            //Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
            Assert.Null(card.AssignedUserId);
        }

        [Fact(DisplayName = "Tests update AssignedUser when card not found and throws AppException")]
        [Trait("AssignUser", "Fail")]
        public async Task AssignUser_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update AssignedUser throws ArgumentNullException when userId is empty")]
        [Trait("AssignUser", "Fail")]
        public async Task UpdateAssignedUser_UserIdIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = Guid.Empty;

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.AssignUser(updateAssignedUserDto));

            // Assert
            _cardRepositoryMock.Verify(repo => repo.FindById(updateAssignedUserDto.CardId), Times.Once);
        }

        [Fact(DisplayName = "Tests update AssignedUser when User not in Team and throws AppException")]
        [Trait("AssignUser", "Fail")]
        public async Task AssignUser_UserNotInTeam_ThrowsAppException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1,DateTime.Now, 1, "Step", 1, 1, 1);
            card.Step.Workflow = WorkflowFixture.FindValidWorkflow();
            card.Step.Workflow.Team = new Team("Team", 1, DateTime.Now);
            card.Step.Workflow.Team.Users = new List<User>();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync(card);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("User not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update AssignedUser when UserId is valid")]
        [Trait("AssignUser", "Sucess")]
        public async Task AssignUser_ValidUser_UpdatesAssignedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var card = CardFixture.FindValidCard();
            card.UpdateAssignedUser(userId);
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1);
            card.Step.Workflow = WorkflowFixture.FindValidWorkflow();
            card.Step.Workflow.Team = new Team("Team", 1, DateTime.Now);
            card.Step.Workflow.Team.Users = new List<User>() { new User(userId, "User","user@user.com", true, DateTime.Now) };
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = userId;

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                               .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var result = await _cardServices.AssignUser(updateAssignedUserDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
        }
    }
}
