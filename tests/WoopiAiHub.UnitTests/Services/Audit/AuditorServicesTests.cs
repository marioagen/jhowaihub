using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Audit;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Rows;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Audit
{
    public class AuditorServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IAuditorRepository> _auditorRepositoryMock;
        private readonly AuditorServices _service;

        public AuditorServicesTests()
        {
            _mocker = new AutoMocker();
            _auditorRepositoryMock = _mocker.GetMock<IAuditorRepository>();
            _mocker.GetMock<ILogger<AuditorServices>>(); // satisfy ctor, no setup needed
            _service = _mocker.CreateInstance<AuditorServices>();
        }

        #region FindCardsAuditSummaryAsync

        [Fact(DisplayName = "FindCardsAuditSummaryAsync should return empty list when no document IDs")]
        [Trait("AuditorServices", "FindCardsAuditSummaryAsync")]
        public async Task FindCardsAuditSummaryAsync_NoDocumentIds_ReturnsEmptyList()
        {
            _auditorRepositoryMock
                .Setup(r => r.FindDocumentIdsForCardsSummaryAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>()))
                .ReturnsAsync(new List<int>());

            var result = await _service.FindCardsAuditSummaryAsync(10, null);

            Assert.Empty(result);
            _auditorRepositoryMock.Verify(r => r.FindAuditRowsForCardsSummaryAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<string?>(), It.IsAny<bool?>()), Times.Never);
        }

        [Fact(DisplayName = "FindCardsAuditSummaryAsync should return card summaries with workflows and IsFinalized when all cards finalized")]
        [Trait("AuditorServices", "FindCardsAuditSummaryAsync")]
        public async Task FindCardsAuditSummaryAsync_ValidData_AllFinalized_ReturnsSummaries()
        {
            var documentIds = new List<int> { 1 };
            var auditRows = new List<CardAuditorSummaryRowDto>
            {
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "Step1", CardId = 1, CardStatusName = StatusNames.Finalize },
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "Step1", CardId = 2, CardStatusName = StatusNames.Finalize }
            };
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForCardsSummaryAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(documentIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForCardsSummaryAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(auditRows);

            var result = await _service.FindCardsAuditSummaryAsync(10, null);

            Assert.Single(result);
            Assert.Equal(1, result.First().DocumentId);
            Assert.Equal("Doc1", result.First().DocumentName);
            Assert.True(result.First().IsFinalized);
            Assert.Equal(2, result.First().ActionsCount);
            Assert.Single(result.First().Workflows);
            Assert.Equal(10, result.First().Workflows.First().Id);
            Assert.Equal("WF1", result.First().Workflows.First().Name);
        }

        [Fact(DisplayName = "FindCardsAuditSummaryAsync should set IsFinalized false when not all cards finalized")]
        [Trait("AuditorServices", "FindCardsAuditSummaryAsync")]
        public async Task FindCardsAuditSummaryAsync_ValidData_NotAllFinalized_IsFinalizedFalse()
        {
            var documentIds = new List<int> { 1 };
            var auditRows = new List<CardAuditorSummaryRowDto>
            {
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "S1", CardId = 1, CardStatusName = StatusNames.Finalize },
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "S1", CardId = 2, CardStatusName = "InProgress" }
            };
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForCardsSummaryAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(documentIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForCardsSummaryAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(auditRows);

            var result = await _service.FindCardsAuditSummaryAsync(10, null);

            Assert.Single(result);
            Assert.False(result.First().IsFinalized);
        }

        [Fact(DisplayName = "FindCardsAuditSummaryAsync should pass take, search and isFinalized to repository")]
        [Trait("AuditorServices", "FindCardsAuditSummaryAsync")]
        public async Task FindCardsAuditSummaryAsync_PassesParametersToRepository()
        {
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForCardsSummaryAsync(5, "test", true)).ReturnsAsync(new List<int>());

            await _service.FindCardsAuditSummaryAsync(5, "test", true);

            _auditorRepositoryMock.Verify(r => r.FindDocumentIdsForCardsSummaryAsync(5, "test", true), Times.Once);
        }

        [Fact(DisplayName = "FindCardsAuditSummaryAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindCardsAuditSummaryAsync")]
        public async Task FindCardsAuditSummaryAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForCardsSummaryAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindCardsAuditSummaryAsync(10, null));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindCardAuditDetailsAsync

        [Fact(DisplayName = "FindCardAuditDetailsAsync should return null when no audit rows")]
        [Trait("AuditorServices", "FindCardAuditDetailsAsync")]
        public async Task FindCardAuditDetailsAsync_NoRows_ReturnsNull()
        {
            _auditorRepositoryMock
                .Setup(r => r.FindAuditRowsForCardDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<CardAuditorDetailRowDto>());

            var result = await _service.FindCardAuditDetailsAsync(1, 10, 20);

            Assert.Null(result);
        }

        [Fact(DisplayName = "FindCardAuditDetailsAsync should return detail DTO with document history")]
        [Trait("AuditorServices", "FindCardAuditDetailsAsync")]
        public async Task FindCardAuditDetailsAsync_ValidData_ReturnsDetailDto()
        {
            var userId = Guid.NewGuid();
            var rows = new List<CardAuditorDetailRowDto>
            {
                new() { DocumentName = "Doc1", WorkflowName = "WF1", UserId = userId, UserName = "User1", ActionName = "Advancement", StepId = 1, StepName = "Step1", Created = DateTime.UtcNow.AddHours(-1) },
                new() { DocumentName = "Doc1", WorkflowName = "WF1", UserId = userId, UserName = "User1", ActionName = "Assign", StepId = 2, StepName = "Step2", Created = DateTime.UtcNow }
            };
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForCardDetailAsync(1, 10, 20, null, null, null, null, true)).ReturnsAsync(rows);

            var result = await _service.FindCardAuditDetailsAsync(1, 10, 20);

            Assert.NotNull(result);
            Assert.Equal(1, result.DocumentId);
            Assert.Equal("Doc1", result.DocumentName);
            Assert.Equal(10, result.WorkflowId);
            Assert.Equal("WF1", result.WorkflowName);
            Assert.Equal(2, result.DocumentHistory.Count);
            Assert.Equal(userId, result.DocumentHistory[0].UserId);
            Assert.Equal("User1", result.DocumentHistory[0].UserName);
            Assert.Equal("Advancement", result.DocumentHistory[0].ActionName);
        }

        [Fact(DisplayName = "FindCardAuditDetailsAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindCardAuditDetailsAsync")]
        public async Task FindCardAuditDetailsAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForCardDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindCardAuditDetailsAsync(1, 10, 20));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindWorkflowAuditSummaryAsync

        [Fact(DisplayName = "FindWorkflowAuditSummaryAsync should return empty list when no workflow IDs")]
        [Trait("AuditorServices", "FindWorkflowAuditSummaryAsync")]
        public async Task FindWorkflowAuditSummaryAsync_NoWorkflowIds_ReturnsEmptyList()
        {
            _auditorRepositoryMock.Setup(r => r.FindWorkflowIdsForWorkflowSummaryAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(new List<int>());

            var result = await _service.FindWorkflowAuditSummaryAsync(10);

            Assert.Empty(result);
            _auditorRepositoryMock.Verify(r => r.FindAuditRowsForWorkflowSummaryAsync(It.IsAny<IReadOnlyList<int>>()), Times.Never);
        }

        [Fact(DisplayName = "FindWorkflowAuditSummaryAsync should return workflow summaries with document and log counts")]
        [Trait("AuditorServices", "FindWorkflowAuditSummaryAsync")]
        public async Task FindWorkflowAuditSummaryAsync_ValidData_ReturnsSummaries()
        {
            var workflowIds = new List<int> { 10 };
            var auditRows = new List<WorkflowAuditorSummaryRowDto>
            {
                new() { WorkflowId = 10, DocumentId = 1, WorkflowName = "WF1", TeamId = 5, TeamName = "Team1", ProfileId = 2, ProfileName = "Profile1" },
                new() { WorkflowId = 10, DocumentId = 2, WorkflowName = "WF1", TeamId = 5, TeamName = "Team1", ProfileId = 2, ProfileName = "Profile1" },
                new() { WorkflowId = 10, DocumentId = 1, WorkflowName = "WF1", TeamId = 5, TeamName = "Team1", ProfileId = 2, ProfileName = "Profile1" }
            };
            _auditorRepositoryMock.Setup(r => r.FindWorkflowIdsForWorkflowSummaryAsync(10, null)).ReturnsAsync(workflowIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForWorkflowSummaryAsync(It.IsAny<IReadOnlyList<int>>())).ReturnsAsync(auditRows);

            var result = await _service.FindWorkflowAuditSummaryAsync(10);

            Assert.Single(result);
            Assert.Equal(10, result.First().WorkflowId);
            Assert.Equal("WF1", result.First().WorkflowName);
            Assert.Equal(2, result.First().DocumentCount);
            Assert.Equal(3, result.First().LogsCount);
            Assert.Equal(5, result.First().TeamId);
            Assert.Equal("Team1", result.First().TeamName);
            Assert.Equal(2, result.First().ProfileId);
            Assert.Equal("Profile1", result.First().ProfileName);
        }

        [Fact(DisplayName = "FindWorkflowAuditSummaryAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindWorkflowAuditSummaryAsync")]
        public async Task FindWorkflowAuditSummaryAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindWorkflowIdsForWorkflowSummaryAsync(It.IsAny<int>(), It.IsAny<string?>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindWorkflowAuditSummaryAsync(10));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindWorkflowAuditDetailsAsync

        [Fact(DisplayName = "FindWorkflowAuditDetailsAsync should return null when no audit rows")]
        [Trait("AuditorServices", "FindWorkflowAuditDetailsAsync")]
        public async Task FindWorkflowAuditDetailsAsync_NoRows_ReturnsNull()
        {
            _auditorRepositoryMock
                .Setup(r => r.FindAuditRowsForWorkflowDetailsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<WorkflowAuditorDetailsRowDto>());

            var result = await _service.FindWorkflowAuditDetailsAsync(10);

            Assert.Null(result);
        }

        [Fact(DisplayName = "FindWorkflowAuditDetailsAsync should return details with steps, document status and cards")]
        [Trait("AuditorServices", "FindWorkflowAuditDetailsAsync")]
        public async Task FindWorkflowAuditDetailsAsync_ValidData_ReturnsDetailsDto()
        {
            var rows = new List<WorkflowAuditorDetailsRowDto>
            {
                new() { Id = 1, CardId = 1, DocumentId = 1, WorkflowId = 10, WorkflowName = "WF1", Created = DateTime.UtcNow, UserId = Guid.NewGuid(), UserName = "U1", ActionType = AuditCardActionType.Advancement, CardName = "C1", CardStatus = StatusNames.Finalize, StepId = 1, StepName = "S1" },
                new() { Id = 2, CardId = 1, DocumentId = 1, WorkflowId = 10, WorkflowName = "WF1", Created = DateTime.UtcNow.AddHours(-1), UserId = Guid.NewGuid(), UserName = "U2", ActionType = AuditCardActionType.Assign, CardName = "C1", CardStatus = "InProgress", StepId = 2, StepName = "S2" }
            };
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForWorkflowDetailsAsync(10, null, null, null, true)).ReturnsAsync(rows);

            var result = await _service.FindWorkflowAuditDetailsAsync(10);

            Assert.NotNull(result);
            Assert.Equal(10, result.WorkflowId);
            Assert.Equal("WF1", result.WorkflowName);
            Assert.Equal(2, result.LogCount);
            Assert.Equal(2, result.StepsCount.Count);
            Assert.Equal(1, result.DocumentStatusCount.TotalDocuments);
            Assert.Equal(1, result.DocumentStatusCount.Finalized); // latest status per card is Finalize for doc 1
            Assert.Equal(2, result.Cards.Count);
        }

        [Fact(DisplayName = "FindWorkflowAuditDetailsAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindWorkflowAuditDetailsAsync")]
        public async Task FindWorkflowAuditDetailsAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForWorkflowDetailsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindWorkflowAuditDetailsAsync(10));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindUserAuditSummaryAsync

        [Fact(DisplayName = "FindUserAuditSummaryAsync should return empty list when no user IDs")]
        [Trait("AuditorServices", "FindUserAuditSummaryAsync")]
        public async Task FindUserAuditSummaryAsync_NoUserIds_ReturnsEmptyList()
        {
            _auditorRepositoryMock.Setup(r => r.FindUserIdsForUserSummaryAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>())).ReturnsAsync(new List<Guid>());

            var result = await _service.FindUserAuditSummaryAsync(10);

            Assert.Empty(result);
            _auditorRepositoryMock.Verify(r => r.FindAuditRowsForUserSummaryAsync(It.IsAny<IReadOnlyList<Guid>>()), Times.Never);
        }

        [Fact(DisplayName = "FindUserAuditSummaryAsync should return user summaries with teams and profiles")]
        [Trait("AuditorServices", "FindUserAuditSummaryAsync")]
        public async Task FindUserAuditSummaryAsync_ValidData_ReturnsSummaries()
        {
            var userId = Guid.NewGuid();
            var userIds = new List<Guid> { userId };
            var teams = new List<UsersAuditorTeamsDto> { new() { TeamId = 5, TeamName = "Team1" } };
            var auditRows = new List<UserAuditorSummaryRowDto>
            {
                new() { UserId = userId, UserName = "User1", WorkflowId = 10, Teams = teams, ProfileId = 2, ProfileName = "Profile1" },
                new() { UserId = userId, UserName = "User1", WorkflowId = 20, Teams = teams, ProfileId = 2, ProfileName = "Profile1" }
            };
            _auditorRepositoryMock.Setup(r => r.FindUserIdsForUserSummaryAsync(10, null, null)).ReturnsAsync(userIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForUserSummaryAsync(It.IsAny<IReadOnlyList<Guid>>())).ReturnsAsync(auditRows);

            var result = await _service.FindUserAuditSummaryAsync(10);

            Assert.Single(result);
            Assert.Equal(userId, result.First().UserId);
            Assert.Equal("User1", result.First().UserName);
            Assert.Single(result.First().Teams);
            Assert.Equal(5, result.First().Teams!.First().TeamId);
            Assert.Equal("Team1", result.First().Teams!.First().TeamName);
            Assert.Single(result.First().Profiles);
            Assert.Equal(2, result.First().Profiles!.First().ProfileId);
            Assert.Equal(2, result.First().WorkflowCount);
            Assert.Equal(2, result.First().LogCount);
        }

        [Fact(DisplayName = "FindUserAuditSummaryAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindUserAuditSummaryAsync")]
        public async Task FindUserAuditSummaryAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindUserIdsForUserSummaryAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindUserAuditSummaryAsync(10));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindUserAuditDetailsAsync

        [Fact(DisplayName = "FindUserAuditDetailsAsync should return null when no audit rows")]
        [Trait("AuditorServices", "FindUserAuditDetailsAsync")]
        public async Task FindUserAuditDetailsAsync_NoRows_ReturnsNull()
        {
            var userId = Guid.NewGuid();
            _auditorRepositoryMock
                .Setup(r => r.FindAuditRowsForUserDetailsAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<UserAuditorDetailsRowDto>());

            var result = await _service.FindUserAuditDetailsAsync(userId);

            Assert.Null(result);
        }

        [Fact(DisplayName = "FindUserAuditDetailsAsync should return details with teams, profiles and action counts")]
        [Trait("AuditorServices", "FindUserAuditDetailsAsync")]
        public async Task FindUserAuditDetailsAsync_ValidData_ReturnsDetailsDto()
        {
            var userId = Guid.NewGuid();
            var teams = new List<UsersAuditorTeamsDto> { new() { TeamId = 5, TeamName = "Team1" } };
            var rows = new List<UserAuditorDetailsRowDto>
            {
                new() { UserId = userId, UserName = "User1", WorkflowId = 10, WorkflowName = "WF1", Teams = teams, ProfileId = 2, ProfileName = "P1", ActionType = AuditCardActionType.Assign, CardId = 1, CardName = "C1", Created = DateTime.UtcNow },
                new() { UserId = userId, UserName = "User1", WorkflowId = 10, WorkflowName = "WF1", Teams = teams, ProfileId = 2, ProfileName = "P1", ActionType = AuditCardActionType.Advancement, CardId = 1, CardName = "C1", Created = DateTime.UtcNow.AddHours(-1) }
            };
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForUserDetailsAsync(userId, null, null, true)).ReturnsAsync(rows);

            var result = await _service.FindUserAuditDetailsAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("User1", result.UserName);
            Assert.Single(result.Teams);
            Assert.Equal(5, result.Teams.First().TeamId);
            Assert.Single(result.Profiles);
            Assert.Equal(2, result.LogCountTotal);
            Assert.Equal(2, result.LogCountByActionType.Count);
            Assert.Equal(2, result.Actions.Count);
        }

        [Fact(DisplayName = "FindUserAuditDetailsAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindUserAuditDetailsAsync")]
        public async Task FindUserAuditDetailsAsync_RepositoryThrows_WrapsInAppException()
        {
            var userId = Guid.NewGuid();
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForUserDetailsAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindUserAuditDetailsAsync(userId));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion
    }
}
