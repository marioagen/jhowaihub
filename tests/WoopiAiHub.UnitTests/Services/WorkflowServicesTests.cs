using Moq;
using Moq.AutoMock;
using System;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
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

        [Fact(DisplayName = "FindByProfileStep should return workflows when successful")]
        [Trait("FindByProfileStep", "Success")]
        public async Task FindByProfileStep_ShouldReturnWorkflows_WhenSuccessful()
        {
            // Arrange
            var profile = new Profile("Test Profile I", 1, DateTime.UtcNow);
            var profiles = new List<Profile>();

            var step = new Step(1, DateTime.UtcNow, 123, "Step Test I", 1, 1, 1);
            var permission = new Permission("Desc", "Permission Name I", "Group", 1, DateTime.UtcNow);

            var stepPermissions = new List<StepProfilePermission>
            {
                new StepProfilePermission(1, 1, 1),
                new StepProfilePermission(1, 2, 1),
            };

            profile.StepProfilePermissions = stepPermissions;
            profiles.Add(profile);
            var workflows = new List<Workflow>
            {
                new Workflow(123, DateTime.UtcNow, new List<Team>(), "Workflow Test I"),
                new Workflow(125, DateTime.UtcNow, new List<Team>(), "Workflow Test II"),
            };

            _workflowRepositoryMock
                .Setup(r => r.FindByStep(It.IsAny<List<int>>()))
                .ReturnsAsync(workflows);

            // Act
            var result = await _workflowServices.FindByProfileStep(profiles);

            // Assert
            Assert.NotNull(result);
            var returnedWorkflow = result.First();
            Assert.Equal(123, returnedWorkflow.Id);
            Assert.Equal("Workflow Test I", returnedWorkflow.Name);
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
            var step = new Step(1, DateTime.Now, workflow.Id, "Step 1", 1, 1, 1);
            var steps = new List<Step> { step };
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
            _stepRepositoryMock.Setup(r => r.FindByIdsWithCards(It.IsAny<IEnumerable<int>>()))
                .Returns(steps);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase2(phase2Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(ProfileLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "UpdatePhase2 should throw AppException when status not found")]
        [Trait("UpdatePhase2", "Fail")]
        public async Task UpdatePhase2_StatusNotFound_ThrowsAppException()
        {
            // Arrange
            var teamFixture = new TeamFixture();
            var team = teamFixture.CreateValidTeam();
            var workflow = new Workflow(1, DateTime.Now, new List<Team> { team }, "Test Workflow");
            var step = new Step(1, DateTime.Now, workflow.Id, "Step 1", 1, 1, 1);
            var steps = new List<Step> { step };
            var profileDto = WorkflowFixture.FindValidProfileDto();

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
                .ReturnsAsync(profileDto);
            _statusRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync((Status?)null);
            _stepRepositoryMock.Setup(r => r.FindByIdsWithCards(It.IsAny<IEnumerable<int>>()))
                .Returns(steps);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase2(phase2Dto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StatusLabel.NotFound, exception.LabelError);
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
            _teamRepositoryMock.Setup(x => x.FindByIds(It.IsAny<List<int>>())).Returns(new List<Team> { });

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
            var stepTools = WorkflowFixture.FindValidStepToolWithDependencies();
            var stepToolUpdateDto = WorkflowFixture.FindValidStepToolUpdateDto();
            var _stepToolRepositoryMock = _mocker.GetMock<IStepToolDependencyRepository>();
            var stepToolDependencyDto = new StepToolOutputDependencyDto{ StepOrder = 1, StepToolOrder = 1};
            stepToolUpdateDto.Dependencies = new List<StepToolOutputDependencyDto> { stepToolDependencyDto };
            var stepToolsList = new List<StepToolUpdateDto> { stepToolUpdateDto };
            var workflowPhase3Dto = new WorkflowPhase3Dto
            {
                WorkflowId = 1,
                Steps = { new StepPhase3Dto
                    {
                        Id = stepDto.Id,
                        Order = stepDto.Order,
                        StepTools = stepToolsList
                }
                }
            };

            var workflow = WorkflowFixture.FindValidWorkflow();

            _workflowRepositoryMock.Setup(x => x.FindByIdReturnModel(workflowPhase3Dto.WorkflowId))
                .ReturnsAsync(workflow);

            _stepToolRepositoryMock.Setup(x => x.DeleteByStepToolIdAsync(It.IsAny<IEnumerable<int>>()))
                .Returns(Task.CompletedTask);

            _stepToolRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<StepToolDependency>()))
                .Returns(Task.CompletedTask);

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
            var stepWithCards = new Step(1, DateTime.Now, 1, "Step 1", 1, 1, 1);
            var steps = new List<Step> { stepWithCards };
            _profileRepositoryMock.Setup(x => x.FindById(stepDto.Profile.Id)).ReturnsAsync(WorkflowFixture.FindValidProfileDto());
            _statusRepositoryMock.Setup(x => x.FindById(stepDto.Status.Id)).ReturnsAsync(WorkflowFixture.FindValidStatus());

            _stepRepositoryMock.Setup(r => r.FindByIdsWithCards(It.IsAny<IEnumerable<int>>()))
            .Returns(steps);
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
            Assert.Equal(2, workflow.Steps.Count);
            Assert.DoesNotContain(workflow.Steps, s => s.Id == 3);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "UpdatePhase2 should throw exception when steps to remove have cards")]
        [Trait("UpdatePhase2", "Fail")]
        public async Task UpdatePhase2_ShouldThrowException_WhenStepsToRemoveHaveCards()
        {
            // Arrange
            var workflowId = 1;
            var step = new Step(1, DateTime.Now, 1, "Step 1", 1, 1, 1);
            var card = new Card(1, DateTime.Now, 1, 1, "Name", 1, true, null);
            step.AddCard(card);
            var stepDto = WorkflowFixture.FindValidStepDto();
            var steps = new List<Step> { step };
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
            var existingWorkflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Test Workflow")
            {
                Steps = new List<Step> { step }
            };

            _workflowRepositoryMock.Setup(x => x.FindByIdReturnModel(workflowId))
                .ReturnsAsync(existingWorkflow);

            _stepRepositoryMock.Setup(x => x.FindByIdsWithCards(It.IsAny<IEnumerable<int>>()))
                .Returns(steps);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowServices.UpdatePhase2(workflowPhase2Dto));
            Assert.Equal(ErrorCode.DefaultError, exception.ErrorCode);
            Assert.Equal("Can't delete with cards related", exception.Message);
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

        [Fact(DisplayName = "FindByTeamId success")]
        [Trait("FindByTeamId", "Success")]
        public async Task FindByTeamId_ShouldReturnWorkflowDto_WhenWorkflowExists()
        {
            // Arrange
            int teamId = 1;
            var workflowFilterDto = new WorkflowFilterDto();
            var workflow = new WorkflowDto
            {
                Id = 1,
                Steps = new List<StepDto>
            {
                new StepDto { Id = 1, Name = "Step 1", Cards = new List<CardDto> { new CardDto(), new CardDto() } },
                new StepDto { Id = 2, Name = "Step 2", Cards = new List<CardDto> { new CardDto() } }
            }
            };

            _workflowRepositoryMock.Setup(x => x.FindByTeamId(teamId, workflowFilterDto))
                .ReturnsAsync(workflow);

            // Act
            var result = await _workflowServices.FindByTeamId(teamId, workflowFilterDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workflow.Id, result.Id);
            Assert.Equal(3, result.NumDocuments);
            _workflowRepositoryMock.Verify(x => x.FindByTeamId(teamId, workflowFilterDto), Times.Once);
        }
        [Fact(DisplayName = "UpdateStepToolOutput success")]
        [Trait("UpdateStepToolOutput", "Success")]
        public async Task UpdateStepToolOutput_ShouldReturnTrue_WhenUpdateIsSuccessful()
        {
            // Arrange
            var outputUpdateDto = new OutputUpdateDto { Id = 1, Value = "New Value" };
            var stepToolOutput = new StepToolOutput(1, DateTime.Now, 1, 1, "test");

            _workflowRepositoryMock.Setup(x => x.FindByStepToolOutputById(outputUpdateDto.Id))
                .Returns(stepToolOutput);

            _workflowRepositoryMock.Setup(x => x.UpdateStepToolOutput(stepToolOutput))
                .ReturnsAsync(true);

            // Act
            var result = await _workflowServices.UpdateStepToolOutput(outputUpdateDto);

            // Assert
            Assert.True(result);
            Assert.Equal("New Value", stepToolOutput.Value); // Verifica se o valor foi alterado
        }

        [Fact(DisplayName = "FindByStepToolOutputById success")]
        [Trait("FindByStepToolOutputById", "Success")]
        public void FindByStepToolOutputById_ShouldReturnStepToolOutput_WhenFound()
        {
            // Arrange
            int stepId = 1;
            var stepToolOutput = new StepToolOutput(1, DateTime.Now, 1, 1, "test");

            _workflowRepositoryMock.Setup(x => x.FindByStepToolOutputById(stepId))
                .Returns(stepToolOutput);

            // Act
            var result = _workflowServices.FindByStepToolOutputById(stepId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(stepId, result.Id);
        }

        [Fact(DisplayName = "FindByStepToolOutputById should return null")]
        [Trait("FindByStepToolOutputById", "Fail")]
        public void FindByStepToolOutputById_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            int stepId = 1;

            _workflowRepositoryMock.Setup(x => x.FindByStepToolOutputById(stepId))
                .Returns((StepToolOutput)null);

            // Act
            var result = _workflowServices.FindByStepToolOutputById(stepId);

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "CreateWorkflowRelationship should execute successfully")]
        [Trait("CreateWorkflowRelationship", "Success")]
        public async Task CreateWorkflowRelationship_ShouldExecuteSuccessfully()
        {
            // Arrange
            var profile = new Profile("name", 1, DateTime.UtcNow);
            var team = new Team("Team 1", 1, DateTime.UtcNow);
            profile.Teams = new List<Team> { team };

            var workflow = WorkflowFixture.FindValidWorkflow();

            var stepsIds = new List<int> { 1, 2, 3 };

            var workflows = new List<Workflow>
            {
                workflow
            };
            _teamRepositoryMock.Setup(x => x.FindByIdReturnModel(It.IsAny<int>()))
                .Returns(team);
            _workflowRepositoryMock.Setup(x => x.FindByStep(stepsIds))
                .ReturnsAsync(workflows);

            // Act
            await _workflowServices.CreateWorkflowRelationship(profile, stepsIds);

            Assert.True(true);
        }
        [Fact(DisplayName = "UpdateTeamWorkflowRelationship should execute successfully")]
        [Trait("UpdateTeamWorkflowRelationship", "Success")]
        public async Task UpdateTeamWorkflowRelationship_ShouldExecuteSuccessfully()
        {
            // Arrange
            var teamfixture = new TeamFixture();
            var team = teamfixture.CreateValidTeam();
            var workflow = WorkflowFixture.FindValidWorkflow();
            var workflows = new List<Workflow>
            {
                workflow
            };
            var profile = new Profile("name", 1, DateTime.UtcNow);
            var profiles = new List<Profile>
            {
                profile
            };
            _teamRepositoryMock.Setup(x => x.FindByIdReturnModel(It.IsAny<int>()))
                .Returns(team);
            // Act
            await _workflowServices.UpdateTeamWorkflowRelationship(team, workflows, profiles);

            // Assert
            Assert.True(true);
        }

        [Fact(DisplayName = "UpdateTeamProfileRelationshipToWorkflow should execute successfully")]
        [Trait("UpdateTeamProfileRelationshipToWorkflow","Success")]
        public async Task UpdateTeamProfileRelationshipToWorkflow_ShouldExecuteSuccessfully()
        {
            // Arrange
            var teamfixture = new TeamFixture();
            var steps = new List<int> { 1, 2, 3 };
            var profile = new Profile("name", 1, DateTime.UtcNow);
            var profile2 = new Profile("name", 2, DateTime.UtcNow);
            var team = teamfixture.CreateValidTeam();
            team.Profiles = new List<Profile> { profile2 };
            profile.Teams = new List<Team> { team };
            var workflow = WorkflowFixture.FindValidWorkflow();
            var workflows = new List<Workflow>
            {
                workflow
            };
            _teamRepositoryMock.Setup(x => x.FindByIdReturnModel(It.IsAny<int>()))
                .Returns(team);
            _workflowRepositoryMock.Setup(x => x.FindByStep(steps)).ReturnsAsync(workflows);

            // Act
            await _workflowServices.UpdateTeamProfileRelationshipToWorkflow(steps, profile);

            // Assert
            Assert.True(true); 
        }
    }
}
