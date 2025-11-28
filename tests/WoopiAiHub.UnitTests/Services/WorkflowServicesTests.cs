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
using WoopiAiHub.Repository;
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
            _workflowRepositoryMock.Setup(repo => repo.DeleteById(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _workflowServices.DeleteById(1);

            // Assert
            Assert.True(result);
            _workflowRepositoryMock.Verify(repo => repo.FindByIdReturnModel(It.IsAny<int>()), Times.Once);
            _workflowRepositoryMock.Verify(repo => repo.DeleteById(It.IsAny<int>()), Times.Once);
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

        [Fact(DisplayName = "CreatePhase1 should throw AppException when name is empty")]
        [Trait("CreatePhase1", "Fail")]
        public async Task CreatePhase1_EmptyName_ThrowsAppException()
        {
            // Arrange
            var phase1Dto = new WorkflowPhase1Dto
            {
                Name = "",
                Teams = new List<int> { 1 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.CreatePhase1(phase1Dto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(WorkflowLabel.InvalidName, exception.LabelError);
        }

        [Fact(DisplayName = "CreatePhase1 should throw AppException when teams list is empty")]
        [Trait("CreatePhase1", "Fail")]
        public async Task CreatePhase1_EmptyTeams_ThrowsAppException()
        {
            // Arrange
            var phase1Dto = new WorkflowPhase1Dto
            {
                Name = "Test Workflow",
                Teams = new List<int>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.CreatePhase1(phase1Dto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(WorkflowLabel.InvalidTeams, exception.LabelError);
        }

        [Fact(DisplayName = "CreatePhase1 should throw AppException when teams not found")]
        [Trait("CreatePhase1", "Fail")]
        public async Task CreatePhase1_TeamsNotFound_ThrowsAppException()
        {
            // Arrange
            var phase1Dto = new WorkflowPhase1Dto
            {
                Name = "Test Workflow",
                Teams = new List<int> { 1, 2 }
            };

            _teamRepositoryMock.Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns(new List<Team> { new Team("Team 1", 1, DateTime.Now) });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.CreatePhase1(phase1Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(TeamLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "CreatePhase1 should return workflow ID when successful")]
        [Trait("CreatePhase1", "Success")]
        public async Task CreatePhase1_ValidData_ReturnsWorkflowId()
        {
            // Arrange
            var phase1Dto = new WorkflowPhase1Dto
            {
                Name = "Test Workflow",
                Teams = new List<int> { 1 }
            };

            var team = new Team("Team 1", 1, DateTime.Now);
            _teamRepositoryMock.Setup(r => r.FindByIds(It.IsAny<ICollection<int>>()))
                .Returns(new List<Team> { team });

            _workflowRepositoryMock.Setup(r => r.Create(It.IsAny<Workflow>()))
                .ReturnsAsync(true);

            // Act
            var result = await _workflowServices.CreatePhase1(phase1Dto);

            // Assert
            Assert.True(result >= 0);
            _teamRepositoryMock.Verify(r => r.FindByIds(It.IsAny<ICollection<int>>()), Times.Once);
            _workflowRepositoryMock.Verify(r => r.Create(It.IsAny<Workflow>()), Times.Once);
        }

        [Fact(DisplayName = "UpdatePhase2 should throw AppException when workflow not found")]
        [Trait("UpdatePhase2", "Fail")]
        public async Task UpdatePhase2_WorkflowNotFound_ThrowsAppException()
        {
            // Arrange
            var phase2Dto = new WorkflowPhase2Dto
            {
                WorkflowId = 1,
                Steps = new List<StepPhase2Dto>()
            };

            _workflowRepositoryMock.Setup(r => r.FindByIdReturnModel(It.IsAny<int>()))
                .ReturnsAsync((Workflow?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase2(phase2Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "UpdatePhase2 should throw AppException when profile not found")]
        [Trait("UpdatePhase2", "Fail")]
        public async Task UpdatePhase2_ProfileNotFound_ThrowsAppException()
        {
            // Arrange
            var teamFixture = new TeamFixture();
            var team = teamFixture.CreateValidTeam();
            var workflow = new Workflow(1, DateTime.Now, new List<Team> { team }, "Test Workflow");

            var phase2Dto = new WorkflowPhase2Dto
            {
                WorkflowId = 1,
                Steps = new List<StepPhase2Dto>
                {
                    new StepPhase2Dto
                    {
                        Id = 0,
                        Name = "Step 1",
                        Order = 1,
                        ProfileId = 1,
                        StatusId = 1
                    }
                }
            };

            _workflowRepositoryMock.Setup(r => r.FindByIdReturnModel(It.IsAny<int>()))
                .ReturnsAsync(workflow);
            _profileRepositoryMock.Setup(r => r.FindById(It.IsAny<int>()))
                .ReturnsAsync((ProfileDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase2(phase2Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(ProfileLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "UpdatePhase3 should throw AppException when workflow not found")]
        [Trait("UpdatePhase3", "Fail")]
        public async Task UpdatePhase3_WorkflowNotFound_ThrowsAppException()
        {
            // Arrange
            var phase3Dto = new WorkflowPhase3Dto
            {
                WorkflowId = 1,
                Steps = new List<StepPhase3Dto>()
            };

            _workflowRepositoryMock.Setup(r => r.FindByIdReturnModel(It.IsAny<int>()))
                .ReturnsAsync((Workflow?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase3(phase3Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "UpdatePhase1 should throw AppException when workflow not found")]
        [Trait("UpdatePhase1", "Fail")]
        public async Task UpdatePhase1_WorkflowNotFound_ThrowsException()
        {
            // Arrange
            var workflowUpdatePhase1Dto = new WorkflowUpdatePhase1Dto { Id = 1, Name = "Updated Workflow" };
            _workflowRepositoryMock.Setup(x => x.FindByIdReturnModel(1)).ReturnsAsync((Workflow)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase1(workflowUpdatePhase1Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "UpdatePhase1 success")]
        [Trait("UpdatePhase1", "Success")]
        public async Task UpdatePhase1_WorkflowExists_UpdatesSuccessfully()
        {
            // Arrange
            var workflowUpdatePhase1Dto = new WorkflowUpdatePhase1Dto { Id = 1, Name = "Updated Workflow", Teams = new List<int> { 1, 2 } };
            var workflow = WorkflowFixture.FindValidWorkflow();
            _workflowRepositoryMock.Setup(x => x.FindByIdReturnModel(1)).ReturnsAsync(workflow);
            _teamRepositoryMock.Setup(x => x.FindByIds(It.IsAny<List<int>>())).Returns(new List<Team>{ });

            // Act
            var result = await _workflowServices.UpdatePhase1(workflowUpdatePhase1Dto);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact(DisplayName = "FindPhase1ById success")]
        [Trait("FindPhase1ById", "Success")]
        public async Task FindPhase1ById_WorkflowExists_ReturnsPhase1Dto()
        {
            // Arrange
            var teamDto = WorkflowFixture.FindValidTeamDto();
            var phase1Dto = new Phase1Dto { Name = "Workflow 1", Teams = [teamDto] };
            _workflowRepositoryMock.Setup(x => x.FindPhase1ById(1)).ReturnsAsync(phase1Dto);

            // Act
            var result = await _workflowServices.FindPhase1ById(1);

            // Assert
            Assert.Equal(phase1Dto, result);
        }

        [Fact(DisplayName = "FindPhase1ById should throw exception")]
        [Trait("FindPhase1ById", "Fail")]
        public async Task FindPhase1ById_WorkflowNotFound_ThrowsException()
        {
            // Arrange
            _workflowRepositoryMock.Setup(x => x.FindPhase1ById(1)).ReturnsAsync((Phase1Dto)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.FindPhase1ById(1));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "UpdatePhase3 success")]
        [Trait("UpdatePhase3", "Success")]
        public async Task UpdatePhase3_WorkflowExists_UpdatesSuccessfully()
        {
            // Arrange
            var stepDto = WorkflowFixture.FindValidStepDto();
            var workflowPhase3Dto = new WorkflowPhase3Dto
            {
                WorkflowId = 1,
                Steps = { new StepPhase3Dto
                    {
                        Id = stepDto.Id,
                        Order = stepDto.Order,
                        StepTools = []
                    }
                }
            };
            
            var workflow = WorkflowFixture.FindValidWorkflow();

            _workflowRepositoryMock.Setup(x => x.FindByIdReturnModel(workflowPhase3Dto.WorkflowId))
                .ReturnsAsync(workflow);

            var stepToolMap = new Dictionary<int, int>(); 
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _workflowServices.UpdatePhase3(workflowPhase3Dto);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "CreatePhase1 success")]
        [Trait("CreatePhase1", "Success")]
        public async Task CreatePhase1_ValidDto_CreatesWorkflowSuccessfully()
        {
            // Arrange
            var workflowPhase1Dto = new WorkflowPhase1Dto
            {
                Name = "New Workflow",
                Teams = new List<int> { 2 }
            };

            var teamsList = new List<Team>
            {
                new Team("nome",2,DateTime.Now),
            };

            _teamRepositoryMock.Setup(x => x.FindByIds(workflowPhase1Dto.Teams))
                .Returns(teamsList);

            var createdWorkflow = new Workflow(1, DateTime.UtcNow, teamsList, workflowPhase1Dto.Name);
            _workflowRepositoryMock.Setup(x => x.Create(It.IsAny<Workflow>())).ReturnsAsync(true)
                .Callback<Workflow>(wf => createdWorkflow = wf);

            // Act
            var result = await _workflowServices.CreatePhase1(workflowPhase1Dto);

            // Assert
            Assert.Equal(createdWorkflow.Id, result);
            _teamRepositoryMock.Verify(x => x.FindByIds(workflowPhase1Dto.Teams), Times.Once);
            _workflowRepositoryMock.Verify(x => x.Create(It.IsAny<Workflow>()), Times.Once);
        }

        [Fact(DisplayName = "UpdatePhase2 success")]
        [Trait("UpdatePhase2", "Success")]
        public async Task UpdatePhase2_ValidDto_UpdatesWorkflowSuccessfully()
        {
            // Arrange
            var stepDto = WorkflowFixture.FindValidStepDto();
            var step = WorkflowFixture.FindValidStep();
            _profileRepositoryMock.Setup(x => x.FindById(stepDto.Profile.Id)).ReturnsAsync(WorkflowFixture.FindValidProfileDto());
            _statusRepositoryMock.Setup(x => x.FindById(stepDto.Status.Id)).ReturnsAsync(WorkflowFixture.FindValidStatus());
            var workflowPhase2Dto = new WorkflowPhase2Dto
            {
                WorkflowId = 1,
                Steps = { new StepPhase2Dto
                    {
                        Id = 1,
                        Name = "Updated Step 1",
                        Order = 1,
                        ProfileId = stepDto.Profile.Id,
                        StatusId = stepDto.Status.Id
                    }
                }
            };

            var existingSteps = new List<Step>
            {
                step
            };

            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Test Workflow")
            {
                Steps = existingSteps
            };

            _workflowRepositoryMock.Setup(x => x.FindByIdReturnModel(workflowPhase2Dto.WorkflowId))
                .ReturnsAsync(workflow);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _workflowServices.UpdatePhase2(workflowPhase2Dto);

            // Assert
            Assert.True(result);
            Assert.Equal(1, workflow.Steps.Count); 
            Assert.DoesNotContain(workflow.Steps, s => s.Id == 3); 
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "FindPhase2ById success")]
        [Trait("FindPhase2ById", "Success")]
        public async Task FindPhase2ById_WorkflowExists_ReturnsStepDtoList()
        {
            // Arrange
            var workflowId = 1;
            var stepDto = WorkflowFixture.FindValidStepDto();
            var expectedSteps = new List<StepDto> { stepDto };
           

            _workflowRepositoryMock.Setup(x => x.FindPhase2ById(workflowId))
                .ReturnsAsync(expectedSteps);

            // Act
            var result = await _workflowServices.FindPhase2ById(workflowId);

            // Assert
            Assert.Equal(expectedSteps, result);
            _workflowRepositoryMock.Verify(x => x.FindPhase2ById(workflowId), Times.Once);
        }

        [Fact(DisplayName = "FindPhase2ById should throw exception")]
        [Trait("FindPhase2ById", "Fail")]
        public async Task FindPhase2ById_WorkflowNotFound_ThrowsException()
        {
            // Arrange
            var workflowId = 1;
            _workflowRepositoryMock.Setup(x => x.FindPhase2ById(workflowId))
                .ReturnsAsync((List<StepDto>)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.FindPhase2ById(workflowId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindPhase3ById Success")]
        [Trait("FindPhase3ById", "Success")]
        public async Task FindPhase3ById_WorkflowExists_ReturnsStepDtoList()
        {
            // Arrange
            var workflowId = 1;
            var stepDto = WorkflowFixture.FindValidStepDto();
            var expectedSteps = new List<StepDto> { stepDto };
            _workflowRepositoryMock.Setup(x => x.FindPhase3ById(workflowId))
                .ReturnsAsync(expectedSteps);

            // Act
            var result = await _workflowServices.FindPhase3ById(workflowId);

            // Assert
            Assert.Equal(expectedSteps, result);
            _workflowRepositoryMock.Verify(x => x.FindPhase3ById(workflowId), Times.Once);
        }

        [Fact(DisplayName = "FindPhase3ById should throw exception")]
        [Trait("FindPhase3ById", "Fail")]
        public async Task FindPhase3ById_WorkflowNotFound_ThrowsException()
        {
            // Arrange
            var workflowId = 1;
            _workflowRepositoryMock.Setup(x => x.FindPhase3ById(workflowId))
                .ReturnsAsync((List<StepDto>)null); 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.FindPhase3ById(workflowId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindStepById Success")]
        [Trait("FindStepById", "Success")]
        public void FindStepById_StepExists_ReturnsStepDto()
        {
            // Arrange
            var stepId = 1;
            var expectedStep = WorkflowFixture.FindValidStepDto();

            _workflowRepositoryMock.Setup(x => x.FindStepById(stepId))
                .Returns(expectedStep);

            // Act
            var result = _workflowServices.FindStepById(stepId);

            // Assert
            Assert.Equal(expectedStep, result);
            _workflowRepositoryMock.Verify(x => x.FindStepById(stepId), Times.Once);
        }
    }
}
