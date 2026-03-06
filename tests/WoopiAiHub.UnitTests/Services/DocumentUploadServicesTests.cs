using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.AutoMock;
using Refit;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentUploadServicesTests
    {
        private readonly DocumentFixture _fixture;
        private readonly AutoMocker _mocker;

        public DocumentUploadServicesTests(DocumentFixture documentFixture)
        {
            _fixture = documentFixture;
            _mocker = new AutoMocker();
        }

        [Fact(DisplayName = "ProcessChunks - Should process document chunks and upload successfully")]
        [Trait("ProcessChunks", "Success")]
        public async Task ProcessChunks_Success()
        {
            // Arrange
            var requestCreateDocumentDto = DocumentFixture.FindValidRequestCreateDocumentDto();
            var fileUploadSummaryDto = DocumentFixture.FindValidFileUploadSummaryDto();
            var workflows = WorkflowFixture.FindValidWorkflows();

            _mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
            var validatorMock = _mocker.GetMock<IValidator<Application.Dto.RequestCreateDocumentDto>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<Application.Dto.RequestCreateDocumentDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            fileRepositoryApi.Setup(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>())).ReturnsAsync(fileUploadSummaryDto);
            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(a => a.FindByIdsAsync(requestCreateDocumentDto.Workflows)).ReturnsAsync(workflows);
            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            automationServicesMock.Setup(a => a.PrepareExecutionAsync(It.IsAny<ICollection<Workflow>>())).ReturnsAsync(true);
            automationServicesMock.Setup(a => a.StartExecutionByWorkflowsAsync(It.IsAny<AutomationServicesDto>(), It.IsAny<List<Workflow>>())).Returns(Task.CompletedTask);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var documentUploadServices = _mocker.CreateInstance<DocumentUploadServices>();

            // Act
            await documentUploadServices.ProcessChunks(requestCreateDocumentDto, "tenant");

            // Assert
            fileRepositoryApi.Verify(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "ProcessChunks with DocumentBatch - Should create DocumentBatch on first file")]
        [Trait("ProcessChunks", "DocumentBatch")]
        public async Task ProcessChunks_WithDocumentBatch_FirstFile()
        {
            // Arrange
            var requestCreateDocumentDto = DocumentFixture.FindValidRequestCreateDocumentDtoForBatch("file1", false);

            var fileUploadSummaryDto = DocumentFixture.FindValidFileUploadSummaryDto();
            var tenant = DocumentFixture.FindValidTenantInfoDto();
            var workflows = WorkflowFixture.FindValidWorkflows();
            var documentBatch = new DocumentBatch(1, DateTime.UtcNow);

            _mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
            var validatorMock = _mocker.GetMock<IValidator<Application.Dto.RequestCreateDocumentDto>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<Application.Dto.RequestCreateDocumentDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            fileRepositoryApi.Setup(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>())).ReturnsAsync(fileUploadSummaryDto);

            var tenantCache = _mocker.GetMock<ITenantCacheServices>();
            tenantCache.Setup(a => a.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenant);

            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(a => a.FindByIdsAsync(requestCreateDocumentDto.Workflows)).ReturnsAsync(workflows);

            var documentBatchRepositoryMock = _mocker.GetMock<IDocumentBatchRepository>();
            documentBatchRepositoryMock.Setup(a => a.CreateAsync(It.IsAny<DocumentBatch>())).ReturnsAsync(documentBatch);

            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            automationServicesMock.Setup(a => a.PrepareExecutionAsync(It.IsAny<List<Workflow>>())).ReturnsAsync(false);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns((Guid?)Guid.NewGuid());
            _mocker.Use<ICurrentUserService>(currentUserServiceMock.Object);

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var documentUploadServices = _mocker.CreateInstance<DocumentUploadServices>();

            // Act
            await documentUploadServices.ProcessChunks(requestCreateDocumentDto, "tenant");

            // Assert
            fileRepositoryApi.Verify(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>()), Times.Once);
            documentBatchRepositoryMock.Verify(a => a.CreateAsync(It.IsAny<DocumentBatch>()), Times.Once);
        }

        [Fact(DisplayName = "ProcessChunks with DocumentBatch - Should use existing batch on subsequent files")]
        [Trait("ProcessChunks", "DocumentBatch")]
        public async Task ProcessChunks_WithDocumentBatch_SubsequentFiles()
        {
            // Arrange
            var firstFile = DocumentFixture.FindValidRequestCreateDocumentDtoForBatch("file1", false);
            var secondFile = DocumentFixture.FindValidRequestCreateDocumentDtoForBatch("file2", false);

            var fileUploadSummaryDto = DocumentFixture.FindValidFileUploadSummaryDto();
            var tenant = DocumentFixture.FindValidTenantInfoDto();
            var workflows = WorkflowFixture.FindValidWorkflows();
            var documentBatch = new DocumentBatch(1, DateTime.UtcNow);

            _mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
            var validatorMock = _mocker.GetMock<IValidator<Application.Dto.RequestCreateDocumentDto>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<Application.Dto.RequestCreateDocumentDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            fileRepositoryApi.Setup(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>())).ReturnsAsync(fileUploadSummaryDto);

            var tenantCache = _mocker.GetMock<ITenantCacheServices>();
            tenantCache.Setup(a => a.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenant);

            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(a => a.FindByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(workflows);

            var documentBatchRepositoryMock = _mocker.GetMock<IDocumentBatchRepository>();
            documentBatchRepositoryMock.Setup(a => a.CreateAsync(It.IsAny<DocumentBatch>())).ReturnsAsync(documentBatch);

            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            automationServicesMock.Setup(a => a.PrepareExecutionAsync(It.IsAny<List<Workflow>>())).ReturnsAsync(false);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns((Guid?)Guid.NewGuid());
            _mocker.Use<ICurrentUserService>(currentUserServiceMock.Object);

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var documentUploadServices = _mocker.CreateInstance<DocumentUploadServices>();

            // Act
            await documentUploadServices.ProcessChunks(firstFile, "tenant");
            await documentUploadServices.ProcessChunks(secondFile, "tenant");

            // Assert
            fileRepositoryApi.Verify(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>()), Times.Exactly(2));
            documentBatchRepositoryMock.Verify(a => a.CreateAsync(It.IsAny<DocumentBatch>()), Times.Once);
        }
    }
}
