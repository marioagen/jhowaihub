using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Audit;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Documents;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Users;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Workflows;
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

        #region FindDocumentsAuditSummaryAsync

        [Fact(DisplayName = "FindDocumentsAuditSummaryAsync should return empty list when no document IDs")]
        [Trait("AuditorServices", "FindDocumentsAuditSummaryAsync")]
        public async Task FindDocumentsAuditSummaryAsync_NoDocumentIds_ReturnsEmptyList()
        {
            _auditorRepositoryMock
                .Setup(r => r.FindDocumentIdsForDocumentsSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>()))
                .ReturnsAsync(new List<int>());

            var result = await _service.FindDocumentsAuditSummaryAsync(10, 0, null);

            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
            _auditorRepositoryMock.Verify(r => r.FindAuditRowsForDocumentsSummaryAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<string?>(), It.IsAny<bool?>()), Times.Never);
        }

        [Fact(DisplayName = "FindDocumentsAuditSummaryAsync should return document summaries with workflows and IsFinalized when all finalized")]
        [Trait("AuditorServices", "FindDocumentsAuditSummaryAsync")]
        public async Task FindDocumentsAuditSummaryAsync_ValidData_AllFinalized_ReturnsSummaries()
        {
            var documentIds = new List<int> { 1 };
            var auditRows = new List<DocumentAuditorSummaryRowDto>
            {
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "Step1", CardId = 1, CardStatusName = StatusNames.Finalize },
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "Step1", CardId = 2, CardStatusName = StatusNames.Finalize }
            };
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForDocumentsSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(documentIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForDocumentsSummaryAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(auditRows);

            var result = await _service.FindDocumentsAuditSummaryAsync(10, 0, null);

            Assert.NotNull(result.Items);
            var list = result.Items.ToList();
            Assert.Single(list);
            var item = list[0];
            Assert.Equal(1, item.DocumentId);
            Assert.Equal("Doc1", item.DocumentName);
            Assert.True(item.IsFinalized);
            Assert.Equal(2, item.ActionsCount);
            Assert.Single(item.Workflows);
            Assert.Equal(10, item.Workflows[0].Id);
            Assert.Equal("WF1", item.Workflows[0].Name);
        }

        [Fact(DisplayName = "FindDocumentsAuditSummaryAsync should set IsFinalized false when not all finalized")]
        [Trait("AuditorServices", "FindDocumentsAuditSummaryAsync")]
        public async Task FindDocumentsAuditSummaryAsync_ValidData_NotAllFinalized_IsFinalizedFalse()
        {
            var documentIds = new List<int> { 1 };
            var auditRows = new List<DocumentAuditorSummaryRowDto>
            {
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "S1", CardId = 1, CardStatusName = StatusNames.Finalize },
                new() { DocumentId = 1, DocumentName = "Doc1", WorkflowId = 10, WorkflowName = "WF1", StepId = 1, StepName = "S1", CardId = 2, CardStatusName = "InProgress" }
            };
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForDocumentsSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(documentIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForDocumentsSummaryAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<string?>(), It.IsAny<bool?>())).ReturnsAsync(auditRows);

            var result = await _service.FindDocumentsAuditSummaryAsync(10, 0, null);

            Assert.NotNull(result.Items);
            var list = result.Items.ToList();
            Assert.Single(list);
            Assert.False(list[0].IsFinalized);
        }

        [Fact(DisplayName = "FindDocumentsAuditSummaryAsync should pass take+1, skip, search and isFinalized to repository")]
        [Trait("AuditorServices", "FindDocumentsAuditSummaryAsync")]
        public async Task FindDocumentsAuditSummaryAsync_PassesParametersToRepository()
        {
            // Service requests take+1 from repository to detect HasMore
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForDocumentsSummaryAsync(6, 0, "test", true)).ReturnsAsync(new List<int>());

            await _service.FindDocumentsAuditSummaryAsync(5, 0, "test", true);

            _auditorRepositoryMock.Verify(r => r.FindDocumentIdsForDocumentsSummaryAsync(6, 0, "test", true), Times.Once);
        }

        [Fact(DisplayName = "FindDocumentsAuditSummaryAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindDocumentsAuditSummaryAsync")]
        public async Task FindDocumentsAuditSummaryAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindDocumentIdsForDocumentsSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindDocumentsAuditSummaryAsync(10, 0, null));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindDocumentAuditDetailsAsync

        [Fact(DisplayName = "FindDocumentAuditDetailsAsync should return null when no audit rows")]
        [Trait("AuditorServices", "FindDocumentAuditDetailsAsync")]
        public async Task FindDocumentAuditDetailsAsync_NoRows_ReturnsNull()
        {
            _auditorRepositoryMock
                .Setup(r => r.FindAuditRowsForDocumentDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<DocumentAuditorDetailRowDto>());

            var result = await _service.FindDocumentAuditDetailsAsync(1, 10, 20);

            Assert.Null(result);
        }

        [Fact(DisplayName = "FindDocumentAuditDetailsAsync should return detail DTO with document history")]
        [Trait("AuditorServices", "FindDocumentAuditDetailsAsync")]
        public async Task FindDocumentAuditDetailsAsync_ValidData_ReturnsDetailDto()
        {
            var userId = Guid.NewGuid();
            var rows = new List<DocumentAuditorDetailRowDto>
            {
                new() { DocumentName = "Doc1", WorkflowName = "WF1", UserId = userId, UserName = "User1", ActionName = "Advancement", StepId = 1, StepName = "Step1", Created = DateTime.UtcNow.AddHours(-1) },
                new() { DocumentName = "Doc1", WorkflowName = "WF1", UserId = userId, UserName = "User1", ActionName = "Assign", StepId = 2, StepName = "Step2", Created = DateTime.UtcNow }
            };
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForDocumentDetailAsync(1, 10, 20, null, null, null, null, true)).ReturnsAsync(rows);

            var result = await _service.FindDocumentAuditDetailsAsync(1, 10, 20);

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

        [Fact(DisplayName = "FindDocumentAuditDetailsAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindDocumentAuditDetailsAsync")]
        public async Task FindDocumentAuditDetailsAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForDocumentDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindDocumentAuditDetailsAsync(1, 10, 20));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindWorkflowAuditSummaryAsync

        [Fact(DisplayName = "FindWorkflowAuditSummaryAsync should return empty list when no workflow IDs")]
        [Trait("AuditorServices", "FindWorkflowAuditSummaryAsync")]
        public async Task FindWorkflowAuditSummaryAsync_NoWorkflowIds_ReturnsEmptyList()
        {
            _auditorRepositoryMock.Setup(r => r.FindWorkflowIdsForWorkflowSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(new List<int>());

            var result = await _service.FindWorkflowAuditSummaryAsync(10);

            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
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
            // Service requests take+1 from repository to detect HasMore
            _auditorRepositoryMock.Setup(r => r.FindWorkflowIdsForWorkflowSummaryAsync(11, 0, null)).ReturnsAsync(workflowIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForWorkflowSummaryAsync(It.IsAny<IReadOnlyList<int>>())).ReturnsAsync(auditRows);

            var result = await _service.FindWorkflowAuditSummaryAsync(10);

            Assert.NotNull(result.Items);
            var list = result.Items.ToList();
            Assert.Single(list);
            var item = list[0];
            Assert.Equal(10, item.WorkflowId);
            Assert.Equal("WF1", item.WorkflowName);
            Assert.Equal(2, item.DocumentCount);
            Assert.Equal(3, item.LogsCount);
            Assert.Equal(5, item.TeamId);
            Assert.Equal("Team1", item.TeamName);
            Assert.Equal(2, item.ProfileId);
            Assert.Equal("Profile1", item.ProfileName);
        }

        [Fact(DisplayName = "FindWorkflowAuditSummaryAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindWorkflowAuditSummaryAsync")]
        public async Task FindWorkflowAuditSummaryAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindWorkflowIdsForWorkflowSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
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

            var result = await _service.FindWorkflowAuditDetailsAsync(10, 10);

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

            var result = await _service.FindWorkflowAuditDetailsAsync(10, 10);

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

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindWorkflowAuditDetailsAsync(10, 10));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion

        #region FindUserAuditSummaryAsync

        [Fact(DisplayName = "FindUserAuditSummaryAsync should return empty list when no user IDs")]
        [Trait("AuditorServices", "FindUserAuditSummaryAsync")]
        public async Task FindUserAuditSummaryAsync_NoUserIds_ReturnsEmptyList()
        {
            _auditorRepositoryMock.Setup(r => r.FindUserIdsForUserSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>())).ReturnsAsync(new List<Guid>());

            var result = await _service.FindUserAuditSummaryAsync(10);

            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
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
            // Service requests take+1 from repository to detect HasMore
            _auditorRepositoryMock.Setup(r => r.FindUserIdsForUserSummaryAsync(11, 0, null, null)).ReturnsAsync(userIds);
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForUserSummaryAsync(It.IsAny<IReadOnlyList<Guid>>())).ReturnsAsync(auditRows);

            var result = await _service.FindUserAuditSummaryAsync(10);

            Assert.NotNull(result.Items);
            var list = result.Items.ToList();
            Assert.Single(list);
            var item = list[0];
            Assert.Equal(userId, item.UserId);
            Assert.Equal("User1", item.UserName);
            Assert.Single(item.Teams!);
            var teamsList = item.Teams!.ToList();
            Assert.Equal(5, teamsList[0].TeamId);
            Assert.Equal("Team1", teamsList[0].TeamName);
            Assert.Single(item.Profiles!);
            var profilesList = item.Profiles!.ToList();
            Assert.Equal(2, profilesList[0].ProfileId);
            Assert.Equal(2, item.WorkflowCount);
            Assert.Equal(2, item.LogCount);
        }

        [Fact(DisplayName = "FindUserAuditSummaryAsync should throw AppException when repository throws")]
        [Trait("AuditorServices", "FindUserAuditSummaryAsync")]
        public async Task FindUserAuditSummaryAsync_RepositoryThrows_WrapsInAppException()
        {
            _auditorRepositoryMock.Setup(r => r.FindUserIdsForUserSummaryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>()))
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
                .Setup(r => r.FindAuditRowsForUserDetailsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<UserAuditorDetailsRowDto>());

            var result = await _service.FindUserAuditDetailsAsync(userId, 10);

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
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForUserDetailsAsync(userId, 10, null, null, true)).ReturnsAsync(rows);

            var result = await _service.FindUserAuditDetailsAsync(userId, 10);

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
            _auditorRepositoryMock.Setup(r => r.FindAuditRowsForUserDetailsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Db error"));

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.FindUserAuditDetailsAsync(userId, 10));

            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
            Assert.Equal("Db error", ex.Message);
        }

        #endregion
    }
}
