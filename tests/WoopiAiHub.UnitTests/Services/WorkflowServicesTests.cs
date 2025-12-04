using Bogus.DataSets;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly Mock<IWorkflowServices> _workflowServicesMock;
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
            _workflowServicesMock = _mocker.GetMock<IWorkflowServices>();
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

        [Fact(DisplayName = "RemoveTeamWorkflowRelationship should remove workflows and update team when successful")]
        [Trait("RemoveTeamWorkflowRelationship", "Success")]
        public async Task RemoveTeamWorkflowRelationship_ShouldRemoveWorkflows_AndUpdateTeam_WhenSuccessful()
        {
            // Arrange
            var team = new Team("Teen Titans", 1, DateTime.UtcNow);
            var workflowX = new Workflow(10, DateTime.UtcNow, new List<Team>(), "WF X");
            var workflowXIX = new Workflow(20, DateTime.UtcNow, new List<Team>(), "WF XIX");
            var workflows = new List<Workflow>();
            workflows.Add(workflowXIX);

            workflowX.AddTeam(team);
            var dto = new List<TeamsWorkflowsDto>
            {
                new TeamsWorkflowsDto
                {
                    TeamId = 1,
                    Workflows = new List<int> { 10, 20 }
                }
            };

            _teamRepositoryMock
                .Setup(r => r.FindByIdReturnModel(1))
                .Returns(team);

            _workflowRepositoryMock
                .Setup(r => r.FindByIdsAsync(It.IsAny<ICollection<int>>()))
                .ReturnsAsync(workflows);

            // Act
            await _workflowServices.RemoveTeamWorkflowRelationship(dto);

            // Assert
            Assert.Empty(team.Workflows);
            _teamRepositoryMock.Verify(r => r.FindByIdReturnModel(1), Times.Once);
            _workflowRepositoryMock.Verify(r => r.FindByIdsAsync(It.Is<ICollection<int>>(ids => ids.SequenceEqual(dto[0].Workflows))), Times.Once);
            _teamRepositoryMock.Verify(r => r.Update(team), Times.Once);
        }

        [Fact(DisplayName = "UpdateTeamProfileRelationshipToWorkflow should exit early when no workflows to remove")]
        [Trait("UpdateTeamProfileRelationshipToWorkflow", "EarlyExit")]
        public async Task UpdateTeamProfileRelationshipToWorkflow_ShouldStopEarly_WhenNoWorkflowsToRemove()
        {
            // Arrange
            var profile = new Profile("Wayne Corp", 1, DateTime.UtcNow);
            var steps = new List<int> { 1, 2, 3 };

            var team = new Team("Teen Titans", 1, DateTime.UtcNow);
            profile.Teams = new List<Team>();
            profile.AddTeam(team);

            var workflowA = new Workflow(10, DateTime.UtcNow, new List<Team>(), "WF 10");
            var workflowB = new Workflow(20, DateTime.UtcNow, new List<Team>(), "WF 20");
            var workflowsFromSteps = new List<Workflow> { workflowA, workflowB };

            _workflowRepositoryMock
                .Setup(r => r.FindByStep(steps))
                .ReturnsAsync(workflowsFromSteps);

            _teamRepositoryMock
                .Setup(r => r.FindByIdReturnModel(team.Id))
                .Returns(team);

            _workflowServicesMock
                .Setup(s => s.VerifyWorkflowMatchInOtherTeamProfile(
                    profile.Id,
                    team.Id,
                    It.IsAny<List<Workflow>>()
                ))
                .ReturnsAsync(new TeamsWorkflowsDto
                {
                    TeamId = team.Id,
                    Workflows = new List<int>()
                });

            // Act
            await _workflowServices.UpdateTeamProfileRelationshipToWorkflow(steps, profile);

            // Assert
            _workflowRepositoryMock.Verify(r => r.FindByStep(steps), Times.Once);
            _teamRepositoryMock.Verify(r => r.FindByIdReturnModel(team.Id), Times.AtLeastOnce);
        }

        [Fact(DisplayName = "VerifyWorkflowMatchInOtherTeamProfile should return only workflows not matched by other profiles")]
        [Trait("VerifyWorkflowMatchInOtherTeamProfile", "PartialMatch")]
        public async Task VerifyWorkflowMatchInOtherTeamProfile_ShouldReturnOnlyWorkflowsNotFoundInOtherProfiles()
        {
            // Arrange
            int profileId = 100;
            int teamId = 10;

            // Team
            var team = new Team("Justice League", teamId, DateTime.UtcNow)
            {
                Profiles = new List<Profile>()
            };

            var workflowA = new Workflow(1, DateTime.UtcNow, new List<Team>(), "WF 1");
            var workflowB = new Workflow(2, DateTime.UtcNow, new List<Team>(), "WF 2");
            var workflowC = new Workflow(3, DateTime.UtcNow, new List<Team>(), "WF 3");

            var inputWorkflows = new List<Workflow> { workflowA, workflowB, workflowC };

            var anotherProfile = new Profile("Gotham Ops", 200, DateTime.UtcNow)
            {
                StepProfilePermissions = new List<StepProfilePermission>()
            };

            anotherProfile.StepProfilePermissions.Add(new StepProfilePermission(1, 999, 1));
            anotherProfile.StepProfilePermissions.Add(new StepProfilePermission(2, 888, 1));

            team.Profiles.Add(anotherProfile);

            _teamRepositoryMock
                .Setup(r => r.FindByIdReturnModel(teamId))
                .Returns(team);

            var workflowsFromSteps = new List<Workflow> { workflowA, workflowC };

            _workflowRepositoryMock
                .Setup(r => r.FindByStep(It.IsAny<List<int>>()))
                .ReturnsAsync(workflowsFromSteps);

            // Act
            var result = await _workflowServices.VerifyWorkflowMatchInOtherTeamProfile(
                profileId,
                teamId,
                inputWorkflows
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teamId, result.TeamId);
        }
    }
}
