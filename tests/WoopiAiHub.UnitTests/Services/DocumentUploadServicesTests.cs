using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.AutoMock;
using Refit;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
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

        [Fact(DisplayName = "ProcessChunks")]
        [Trait("ProcessChunks", "Success")]
        public async Task ProcessChunks_Success()
        {
            // Arrange
            var requestCreateDocumentDto = _fixture.FindValidRequestCreateDocumentDto();
            var fileUploadSummaryDto = _fixture.FindValidFileUploadSummaryDto();
            var workflows = WorkflowFixture.FindValidWorkflows();

            _mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
            var validatorMock = _mocker.GetMock<IValidator<WoopiAiHub.Application.Dto.RequestCreateDocumentDto>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<WoopiAiHub.Application.Dto.RequestCreateDocumentDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            fileRepositoryApi.Setup(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>())).ReturnsAsync(fileUploadSummaryDto);
            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(a => a.FindByIdsAsync(requestCreateDocumentDto.Workflows)).ReturnsAsync(workflows);
            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            automationServicesMock.Setup(a => a.PrepareExecutionAsync(It.IsAny<ICollection<Workflow>>())).ReturnsAsync(true);
            automationServicesMock.Setup(a => a.StartExecutionByWorkflowsAsync(It.IsAny<AutomationServicesDto>(), It.IsAny<List<Workflow>>())).Returns(Task.CompletedTask);

            var documentUploadServices = _mocker.CreateInstance<DocumentUploadServices>();

            // Act
            await documentUploadServices.ProcessChunks(requestCreateDocumentDto, "tenant");

            // Assert
            fileRepositoryApi.Verify(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>()), Times.Once);
        }
    }
}