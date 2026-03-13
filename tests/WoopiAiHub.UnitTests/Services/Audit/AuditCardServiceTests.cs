using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Audit;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models.Audit;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Audit
{
    public class AuditCardServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IAuditCardRepository> _auditCardRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly AuditCardService _service;

        public AuditCardServiceTests()
        {
            _mocker = new AutoMocker();
            _auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            _currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            _service = _mocker.CreateInstance<AuditCardService>();
        }

        [Fact(DisplayName = "CreateAndSaveAsync should throw ArgumentOutOfRangeException when actionType is not defined")]
        [Trait("AuditCardService", "CreateAndSaveAsync")]
        public async Task CreateAndSaveAsync_InvalidActionType_ThrowsArgumentOutOfRangeException()
        {
            var invalidActionType = (AuditCardActionType)(-1);
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _service.CreateAndSaveAsync(1, 1, invalidActionType));

            Assert.Equal("actionType", exception.ParamName);
            _auditCardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditCard>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "CreateAndSaveAsync should throw InvalidOperationException when user is not authenticated")]
        [Trait("AuditCardService", "CreateAndSaveAsync")]
        public async Task CreateAndSaveAsync_UserNotAuthenticated_ThrowsInvalidOperationException()
        {
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateAndSaveAsync(1, 1, AuditCardActionType.Assign));

            Assert.Equal("Current user is required to create an audit log. When running in automation context, provide the user email from the automation DTO.", exception.Message);
            _auditCardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditCard>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "CreateAndSaveAsync should throw InvalidOperationException when user Id is null")]
        [Trait("AuditCardService", "CreateAndSaveAsync")]
        public async Task CreateAndSaveAsync_UserIdNull_ThrowsInvalidOperationException()
        {
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns((Guid?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateAndSaveAsync(1, 1, AuditCardActionType.Assign));

            Assert.Equal("Current user is required to create an audit log. When running in automation context, provide the user email from the automation DTO.", exception.Message);
            _auditCardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditCard>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "CreateAndSaveAsync should call repository AddAsync with correct AuditCard")]
        [Trait("AuditCardService", "CreateAndSaveAsync")]
        public async Task CreateAndSaveAsync_ValidInputs_CallsRepositoryAddAsync()
        {
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns(userId);
            _auditCardRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuditCard>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.CreateAndSaveAsync(cardId: 2, workflowId: 3, AuditCardActionType.Advancement);

            _auditCardRepositoryMock.Verify(r => r.AddAsync(It.Is<AuditCard>(a =>
                a.CardId == 2 &&
                a.WorkflowId == 3 &&
                a.ActionType == AuditCardActionType.Advancement &&
                a.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "CreateAndSaveAsync should pass CancellationToken to repository")]
        [Trait("AuditCardService", "CreateAndSaveAsync")]
        public async Task CreateAndSaveAsync_PassesCancellationToken()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());
            _auditCardRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuditCard>(), token))
                .Returns(Task.CompletedTask);

            await _service.CreateAndSaveAsync(1, 1, AuditCardActionType.Assign, cancellationToken: token);

            _auditCardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditCard>(), token), Times.Once);
        }

        [Fact(DisplayName = "CreateBatchAndSaveAsync should return without calling repository when cardWorkflows is empty")]
        [Trait("AuditCardService", "CreateBatchAndSaveAsync")]
        public async Task CreateBatchAndSaveAsync_EmptyList_DoesNotCallRepository()
        {
            var cardWorkflows = Array.Empty<(int cardId, int workflowId)>().ToList();

            await _service.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Assign);

            _auditCardRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<AuditCard>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "CreateBatchAndSaveAsync should throw ArgumentOutOfRangeException when actionType is not defined")]
        [Trait("AuditCardService", "CreateBatchAndSaveAsync")]
        public async Task CreateBatchAndSaveAsync_InvalidActionType_ThrowsArgumentOutOfRangeException()
        {
            var invalidActionType = (AuditCardActionType)(-1);
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());
            var cardWorkflows = new List<(int, int)> { (1, 1) };

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _service.CreateBatchAndSaveAsync(cardWorkflows, invalidActionType));

            Assert.Equal("actionType", exception.ParamName);
            _auditCardRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<AuditCard>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "CreateBatchAndSaveAsync should throw InvalidOperationException when user is not authenticated")]
        [Trait("AuditCardService", "CreateBatchAndSaveAsync")]
        public async Task CreateBatchAndSaveAsync_UserNotAuthenticated_ThrowsInvalidOperationException()
        {
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
            var cardWorkflows = new List<(int, int)> { (1, 1) };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Unassign));

            Assert.Equal("Current user is required to create an audit log. When running in automation context, provide the user email from the automation DTO.", exception.Message);
            _auditCardRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<AuditCard>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "CreateBatchAndSaveAsync should call repository AddRangeAsync with correct AuditCards")]
        [Trait("AuditCardService", "CreateBatchAndSaveAsync")]
        public async Task CreateBatchAndSaveAsync_ValidInputs_CallsRepositoryAddRangeAsync()
        {
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns(userId);
            _auditCardRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var cardWorkflows = new List<(int cardId, int workflowId)> { (1, 10), (2, 10) };

            await _service.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Assign);

            _auditCardRepositoryMock.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<AuditCard>>(e =>
                e.Count() == 2 &&
                e.All(a => a.ActionType == AuditCardActionType.Assign && a.UserId == userId) &&
                e.Any(a => a.CardId == 1 && a.WorkflowId == 10) &&
                e.Any(a => a.CardId == 2 && a.WorkflowId == 10)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "CreateBatchAndSaveAsync should pass CancellationToken to repository")]
        [Trait("AuditCardService", "CreateBatchAndSaveAsync")]
        public async Task CreateBatchAndSaveAsync_PassesCancellationToken()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());
            _auditCardRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<AuditCard>>(), token))
                .Returns(Task.CompletedTask);
            var cardWorkflows = new List<(int, int)> { (1, 1) };

            await _service.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Advancement, cancellationToken: token);

            _auditCardRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<AuditCard>>(), token), Times.Once);
        }
    }
}
