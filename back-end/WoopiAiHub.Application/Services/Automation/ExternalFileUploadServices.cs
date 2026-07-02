using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services.Automation
{
    public class ExternalFileUploadServices : IExternalFileUploadServices
    {
        private readonly IAutomationServices _automationServices;
        private readonly IWorkflowServices _workflowServices;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotifier _hubNotifier;

        public ExternalFileUploadServices(IAutomationServices automationServices,
                                          IWorkflowServices workflowServices,
                                          IDocumentRepository documentRepository,
                                          IUnitOfWork unitOfWork,
                                          IHubNotifier hubNotifier)
        {
            _automationServices = automationServices;
            _workflowServices = workflowServices;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
            _hubNotifier = hubNotifier;
        }

        /// <summary>
        /// Process the external file upload by creating a document and a card, associating them with the corresponding workflow, and starting the automation execution if applicable.
        /// </summary>
        /// <param name="externalFileUploadDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task ProcessExternalFileUpload(ExternalFileUploadDto externalFileUploadDto)
        {
            _unitOfWork.BeginTransaction();
            Workflow? workflow;
            try
            {
                workflow = await _workflowServices.FindModelById(externalFileUploadDto.WorkflowId);
                if ( workflow != null)
                {
                    var document = CreateDocument(externalFileUploadDto, workflow);
                    var card = CreateCard(externalFileUploadDto, workflow);
                    document.Cards = [card];
                    _documentRepository.Create(document);

                    var hasExecutions = await _automationServices.PrepareExecutionAsync([workflow]);
                    if (hasExecutions)
                    {
                        var automationServicesDto = CreateAutomation(externalFileUploadDto);
                        await _automationServices.StartExecutionByWorkflowsAsync(automationServicesDto, [workflow]);
                        await _hubNotifier.WorkflowKanbanRefreshAsync(externalFileUploadDto.Email, externalFileUploadDto.WorkflowId);
                    }
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="AutomationServicesDto"/> based on the provided <see cref="ExternalFileUploadDto"/>.
        /// </summary>
        /// <param name="externalFileUploadDto"></param>
        /// <returns></returns>
        private static AutomationServicesDto CreateAutomation(ExternalFileUploadDto externalFileUploadDto)
        {
            return new AutomationServicesDto
            (
                0,
                0,
                externalFileUploadDto.Tenant,
                externalFileUploadDto.Email,
                externalFileUploadDto.FileReference,
                0
            );
        }

        /// <summary>
        /// Creates an instance of <see cref="Card"/> based on the provided <see cref="ExternalFileUploadDto"/> and associated <see cref="Workflow"/>.
        /// </summary>
        /// <param name="externalFileUploadDto"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private static Card CreateCard(ExternalFileUploadDto externalFileUploadDto, Workflow workflow)
        {
            var firtStep = workflow.Steps.OrderBy(s => s.Order).FirstOrDefault();
            return new Card
            (
                0,
                DateTime.UtcNow,
                firtStep!.Id,
                0,
                externalFileUploadDto.FileName,
                firtStep.StatusId,
                null
            );
        }

        /// <summary>
        /// Creates an instance of <see cref="Document"/> based on the provided <see cref="ExternalFileUploadDto"/> and associated <see cref="Workflow"/>.
        /// </summary>
        /// <param name="externalFileUploadDto"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private static Document CreateDocument(ExternalFileUploadDto externalFileUploadDto, Workflow workflow)
        {
            return new Document
            (
                externalFileUploadDto.FileName,
                string.Empty,
                externalFileUploadDto.FileReference,
                Domain.Enum.DocumentStatus.NotAnalyzed,
                externalFileUploadDto.Email,
                0,
                [workflow],
                DateTime.Now,
                extractionMode: externalFileUploadDto.ExtractionMode
            );
        }
    }
}
