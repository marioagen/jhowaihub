using Moq;
using Moq.AutoMock;
using System;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
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
    public class WorkflowServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IWorkflowRepository> _workflowRepositoryMock;
        private readonly Mock<IStepRepository> _stepRepositoryMock;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly Mock<IStatusRepository> _statusRepositoryMock;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly IValidateWorkflow _validateWorkflow;
        private readonly IValidateStep _validateStep;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly WorkflowServices _workflowServices;

        public WorkflowServicesTests()
        {
            _mocker = new AutoMocker();

            _workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            _stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            _profileRepositoryMock = _mocker.GetMock<IProfileRepository>();
            _statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            _teamRepositoryMock = _mocker.GetMock<ITeamRepository>();
            _unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _validateWorkflow = new ValidateWorkflow(_workflowRepositoryMock.Object, _teamRepositoryMock.Object);
            _validateStep = new ValidateStep(_cardRepositoryMock.Object);

            _mocker.Use<IValidateWorkflow>(_validateWorkflow);
            _mocker.Use<IValidateStep>(_validateStep);

            _workflowServices = _mocker.CreateInstance<WorkflowServices>();
        }

        [Fact(DisplayName = "Test FindById and returns a workflow")]
        [Trait("FindById", "Success")]
        public async Task FindById_WorkflowExists_ReturnsWorkflow()
        {
            // Arrange
            var workflowId = 1;
            var expectedWorkflow = new WorkflowDto { Id = workflowId };
            _workflowRepositoryMock.Setup(repo => repo.FindById(workflowId, null))
                .ReturnsAsync(expectedWorkflow);

            // Act
            var result = await _workflowServices.FindById(workflowId, null);

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindById(workflowId, null), Times.Once);
            Assert.Equal(expectedWorkflow, result);
        }

        [Fact(DisplayName = "Test FindById and throws an exception")]
        [Trait("FindById", "Fail")]
        public async Task FindById_WorkflowDoesNotExist_ThrowsAppException()
        {
            // Arrange
            var workflowId = 1;
            _workflowRepositoryMock.Setup(repo => repo.FindById(workflowId, null))
                .ReturnsAsync((WorkflowDto?)null);

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.FindById(workflowId, null));

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindById(workflowId, null), Times.Once);
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Workflow not found", exception.Message);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Test FindByTeamId and throws an exception")]
        [Trait("FindByTeamId", "Fail")]
        public async Task FindByTeamId_WorkflowDoesNotExist_ThrowsAppException()
        {
            // Arrange
            var teamId = 1;
            _workflowRepositoryMock.Setup(repo => repo.FindByTeamId(teamId, null))
                .ReturnsAsync((WorkflowDto?)null);

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.FindByTeamId(teamId, null));

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindByTeamId(teamId, null), Times.Once);
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Workflow not found", exception.Message);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Create should throw AppException when workflow has no step")]
        [Trait("Create", "Fail")]
        public async Task Create_ShouldThrowAppException_WhenWorkflowHasNoStep()
        {        
            // Arrange
            var workflowCreateDto = WorkflowFixture.FindValidWorkflowCreateDtoNoSteps();
            var teamFixtrue = new TeamFixture();
            var team = teamFixtrue.CreateValidTeam();
            workflowCreateDto.Teams = new List<int> { team.Id };

            _workflowRepositoryMock.Setup(r => r.FindByTeamId(It.IsAny<int>(), null)).ReturnsAsync((WorkflowDto?)null);
            _teamRepositoryMock
                .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns((ICollection<int> ids) =>
                {
                    return new List<Team> { team };
                });

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _workflowServices.Create(workflowCreateDto));

            // Assert 
            Assert.Equal(ErrorCode.RequiredField, ex.ErrorCode);
            Assert.Equal("Workflow must have at least one step", ex.Message);
            Assert.Equal(StepLabel.Required, ex.LabelError);
            _workflowRepositoryMock.Verify(r => r.Create(It.IsAny<Workflow>()), Times.Never);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Exactly(1));
        }

        [Fact(DisplayName = "Create should throw AppException when workflow step has empty name")]
        [Trait("Create", "Fail")]
        public async Task Create_ShouldThrowAppException_WhenStepHasEmptyName()
        {
            // Arrange
            var workflowCreateDto = WorkflowFixture.FindValidWorkflowCreateDtoStepWithNoName();
            var teamFixtrue = new TeamFixture();
            var team = teamFixtrue.CreateValidTeam();
            workflowCreateDto.Teams = new List<int> { team.Id };

            _workflowRepositoryMock.Setup(r => r.FindByTeamId(It.IsAny<int>(), null)).ReturnsAsync((WorkflowDto?)null);
            _teamRepositoryMock
                .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns((ICollection<int> ids) =>
                {
                    return new List<Team> { team };
                });

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _workflowServices.Create(workflowCreateDto));

            // Assert 
            Assert.Equal(ErrorCode.RequiredField, ex.ErrorCode);
            Assert.Equal("Step name cannot be empty", ex.Message);
            Assert.Equal(StepLabel.NameRequired, ex.LabelError);
            _workflowRepositoryMock.Verify(r => r.Create(It.IsAny<Workflow>()), Times.Never);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Exactly(1));
        }

        [Fact(DisplayName = "Create should throw AppException when team not found")]
        [Trait("Create", "Fail")]
        public async Task Create_ShouldThrowAppException_WhenTeamNotFound()
        {
            // Arrange
            var workflowCreateDto = WorkflowFixture.FindValidWorkflowCreateDto();
            var profileDto = WorkflowFixture.FindValidProfileDto();
            var status = WorkflowFixture.FindValidStatus();
            var teamFixtrue = new TeamFixture();
            var team = teamFixtrue.CreateValidTeam();
            workflowCreateDto.Teams = new List<int> { 5 };

            _profileRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(profileDto);
            _statusRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(status);
            _workflowRepositoryMock.Setup(r => r.FindByTeamId(It.IsAny<int>(), null)).ReturnsAsync((WorkflowDto?)null);
            _teamRepositoryMock
                .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns((ICollection<int> ids) =>
                {
                    return new List<Team> { };
                });

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _workflowServices.Create(workflowCreateDto));

            // Assert
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal("Team not found", ex.Message);
            Assert.Equal(TeamLabel.NotFound, ex.LabelError);
            _workflowRepositoryMock.Verify(r => r.Create(It.IsAny<Workflow>()), Times.Never);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Exactly(1));
            _profileRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Never);
        }

        [Fact(DisplayName = "Create should throw AppException when Step has invalid ProfileId")]
        [Trait("Create", "Fail")]
        public async Task Create_ShouldThrowAppException_WhenStepHasInvalidProfileId()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStepCreateDto();
            var workflowCreateDto = WorkflowFixture.FindValidWorkflowCreateDto();
            var teamFixtrue = new TeamFixture();
            var team = teamFixtrue.CreateValidTeam();
            workflowCreateDto.Teams = new List<int> { team.Id };

            _workflowRepositoryMock.Setup(r => r.FindByTeamId(It.IsAny<int>(), null)).ReturnsAsync((WorkflowDto?)null);
            _profileRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync((ProfileDto?)null);
            _teamRepositoryMock
               .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
               .Returns((ICollection<int> ids) =>
               {
                   return new List<Team> { team };
               });

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _workflowServices.Create(workflowCreateDto));

            // Assert
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal("Profile not found", ex.Message);
            Assert.Equal(ProfileLabel.NotFound, ex.LabelError);
            _profileRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Once);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Exactly(2));
        }

        [Fact(DisplayName = "Create should throw AppException when Step has invalid StatusId")]
        [Trait("Create", "Fail")]
        public async Task Create_ShouldThrowAppException_WhenStepHasInvalidStatusId()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStepCreateDto();
            var workflowCreateDto = WorkflowFixture.FindValidWorkflowCreateDto();
            var teamDto = WorkflowFixture.FindValidTeamDto();
            var profileDto = WorkflowFixture.FindValidProfileDto();
            var teamFixtrue = new TeamFixture();
            var team = teamFixtrue.CreateValidTeam();
            workflowCreateDto.Teams = new List<int> { team.Id };

            _workflowRepositoryMock.Setup(r => r.FindByTeamId(It.IsAny<int>(), null)).ReturnsAsync((WorkflowDto?)null);
            _teamRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(teamDto);
            _profileRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(profileDto);
            _statusRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync((Status?)null);
            _teamRepositoryMock
               .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
               .Returns((ICollection<int> ids) =>
               {
                   return new List<Team> { team };
               });

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _workflowServices.Create(workflowCreateDto));

            // Assert
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal("Status not found", ex.Message);
            Assert.Equal(StatusLabel.NotFound, ex.LabelError);
            _profileRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Once);
            _statusRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Once);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Exactly(2));
        }

        [Fact(DisplayName = "Create should return true when success")]
        [Trait("Create", "Success")]
        public async Task Create_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStepCreateDto();
            var workflowCreateDto = WorkflowFixture.FindValidWorkflowCreateDto();
            var profileDto = WorkflowFixture.FindValidProfileDto();
            var status = WorkflowFixture.FindValidStatus();
            var teamFixtrue = new TeamFixture();
            var team = teamFixtrue.CreateValidTeam();
            workflowCreateDto.Teams = new List<int> { team.Id };

            _workflowRepositoryMock.Setup(r => r.FindByTeamId(It.IsAny<int>(), null)).ReturnsAsync((WorkflowDto?)null);
            _profileRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(profileDto);
            _statusRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(status);
            _workflowRepositoryMock.Setup(r => r.Create(It.IsAny<Workflow>())).ReturnsAsync(true);
            _teamRepositoryMock
                .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns((ICollection<int> ids) =>
                {
                    return new List<Team> { team };
                });

            // Act
            var result = await _workflowServices.Create(workflowCreateDto);

            // Assert
            Assert.True(result);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Exactly(2));
            _profileRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Once);
            _statusRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Update should throw AppException when workflow is not found")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldThrowAppException_WhenWorkflowNotFound()
        {
            // Arrange
            var updateDto = WorkflowFixture.FindValidWorkflowUpdateDto();
            _workflowRepositoryMock.Setup(r => r.FindByIdReturnModel(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _workflowServices.Update(updateDto));

            // Assert
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal("Workflow not found", ex.Message);
            Assert.Equal(WorkflowLabel.NotFound, ex.LabelError);
            _workflowRepositoryMock.Verify(r => r.FindByIdReturnModel(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Update should return true when update is successful")]
        [Trait("Update", "Success")]
        public async Task Update_ShouldReturnTrue_WhenUpdateIsSuccessful()
        {
            var teamFixtrue = new TeamFixture();
            // Arrange
            var updateDto = WorkflowFixture.FindValidWorkflowUpdateDto();

            var workflow = WorkflowFixture.FindValidWorkflow();
            var step = WorkflowFixture.FindValidStep();

            var team = teamFixtrue.CreateValidTeam();
            updateDto.Steps.Clear();

            var stepUpdateDto = WorkflowFixture.FindValidStepUpdateDto();
            var stepUpdateDto2 = WorkflowFixture.FindValidStepUpdateDto();

            stepUpdateDto.Id = 0;
            stepUpdateDto.Order = 1;
            updateDto.Steps.Add(stepUpdateDto);

            stepUpdateDto2.Id = 10;
            stepUpdateDto2.Order = 2;
            updateDto.Steps.Add(stepUpdateDto2);

            var teams = new List<int> { team.Id };
            updateDto.Teams = teams;

            workflow.Steps.Clear();
            foreach (var stepDto in updateDto.Steps)
            {
                workflow.Steps.Add(new Step
                (
                    stepDto.Id,
                    DateTime.Now,
                    workflow.Id,
                    stepDto.Name,
                    stepDto.Order,
                    stepDto.ProfileId,
                    stepDto.StatusId
                ));
            }
            _workflowRepositoryMock.Setup(r => r.FindByIdReturnModel(updateDto.Id)).ReturnsAsync(workflow);
            _cardRepositoryMock.Setup(r => r.ExistsStepsInUse(It.IsAny<ICollection<int>>())).ReturnsAsync(false);
            int callCount = 0;
            _stepRepositoryMock
                .Setup(r => r.FindById(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    if (callCount == 0)
                    {
                        callCount++;
                        return step; // Retorna Step na primeira chamada
                    }
                    return null; // Retorna null nas demais
                });
            _stepRepositoryMock.Setup(r => r.Update(It.IsAny<Step>())).ReturnsAsync(true);
            _stepRepositoryMock.Setup(r => r.DeleteByIds(It.IsAny<ICollection<int>>())).Returns(true);
            _profileRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(WorkflowFixture.FindValidProfileDto());
            _statusRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(WorkflowFixture.FindValidStatus());
            _teamRepositoryMock
                .Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns((ICollection<int> ids) =>
                {
                    return new List<Team> { team };
                });

            // Act
            var result = await _workflowServices.Update(updateDto);

            // Assert
            Assert.True(result);
            _workflowRepositoryMock.Verify(r => r.FindByIdReturnModel(updateDto.Id), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "DeleteById should delete workflow and steps and return true")]
        [Trait("DeleteById", "Success")]
        public async Task DeleteById_ShouldDeleteWorkflowAndSteps()
        {
            // Arrange
            var workflow = WorkflowFixture.FindValidWorkflow();

            _workflowRepositoryMock.Setup(repo => repo.FindByIdReturnModel(It.IsAny<int>())).ReturnsAsync(workflow);
            _cardRepositoryMock.Setup(repo => repo.ExistsStepsInUse(It.IsAny<List<int>>())).ReturnsAsync(false);
            _stepRepositoryMock.Setup(repo => repo.DeleteByIds(It.IsAny<List<int>>())).Returns(true);

            // Act
            var result = await _workflowServices.DeleteById(1);

            // Assert
            Assert.True(result);
            _stepRepositoryMock.Verify(repo => repo.DeleteByIds(It.IsAny<List<int>>()), Times.Once);
            _workflowRepositoryMock.Verify(repo => repo.DeleteById(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        }

        [Fact(DisplayName = "DeleteById should throw exception when workflow not found")]
        [Trait("DeleteById", "Success")]
        public async Task DeleteById_ShouldThrowException_WhenWorkflowNotFound()
        {
            // Arrange
            _workflowRepositoryMock.Setup(repo => repo.FindByIdReturnModel(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.DeleteById(1));

            _workflowRepositoryMock.Verify(repo => repo.FindByIdReturnModel(It.IsAny<int>()), Times.Once);
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Workflow not found", exception.Message);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Test FindAllByUser returns workflows for valid user")]
        [Trait("FindAllByUser", "Success")]
        public void FindAllByUser_ValidUser_ReturnsWorkflows()
        {
            // Arrange
            var email = "user@email.com";
            var expectedWorkflows = new List<WorkflowDto>
            {
                new WorkflowDto { Id = 1, Name = "Workflow 1" },
                new WorkflowDto { Id = 2, Name = "Workflow 2" }
            };
            _workflowRepositoryMock.Setup(repo => repo.FindAllByUser(email))
                .Returns(expectedWorkflows);

            // Act
            var result = _workflowServices.FindAllByUser(email);

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindAllByUser(email), Times.Once);
            Assert.Equal(expectedWorkflows, result);
        }

        [Fact(DisplayName = "Test FindAllByUser returns empty when user has no workflows")]
        [Trait("FindAllByUser", "Fail")]
        public void FindAllByUser_UserHasNoWorkflows_ReturnsEmptyList()
        {
            // Arrange
            var email = "empty@email.com";
            var expectedWorkflows = new List<WorkflowDto>();
            _workflowRepositoryMock.Setup(repo => repo.FindAllByUser(email))
                .Returns(expectedWorkflows);

            // Act
            var result = _workflowServices.FindAllByUser(email);

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindAllByUser(email), Times.Once);
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Test FindAllPaged page greater than zero returns PaginatedList")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_PageGreaterThanZero_ReturnsPaginatedList()
        {
            // Arrange
            var workflowPagedDto = new WorkflowPagedDto { Page = 1 };
            var workflowList = new List<WorkflowDto> { new WorkflowDto() };

            _workflowRepositoryMock.Setup(repo => repo.FindAllWithFilter(workflowPagedDto)).Returns(workflowList.AsQueryable());

            // Act
            var result = _workflowServices.FindAllPaged(workflowPagedDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Test FindAllPaged page less than or equal to zero throws ArgumentException")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_PageLessThanOrEqualToZero_ThrowsArgumentException()
        {
            // Arrange
            var workflowPagedDto = new WorkflowPagedDto { Page = 0 };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _workflowServices.FindAllPaged(workflowPagedDto));
            Assert.Equal("Invalid Page", exception.Message);
        }

        [Fact(DisplayName = "Test FindAll returns all Workflows")]
        [Trait("FindAll", "Success")]
        public void FindAll_ReturnsAllWorkflows()
        {
            // Arrange
            var workflowList = new List<WorkflowDto> { new WorkflowDto(), new WorkflowDto() };
            _workflowRepositoryMock.Setup(repo => repo.FindAll()).Returns(workflowList);

            // Act
            var result = _workflowServices.FindAll();

            // Assert
            Assert.Equal(workflowList, result);
        }
    }
}
