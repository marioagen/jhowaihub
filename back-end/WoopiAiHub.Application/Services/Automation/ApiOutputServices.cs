using System.Text.Encodings.Web;
using System.Text.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services.Automation
{
    public class ApiOutputServices(IStepToolOutputRepository stepToolOutputRepository,
        IStepToolExecutionRepository stepToolExecutionRepository,
        IExecutionServices executionServices) : IApiOutputServices
    {
        private readonly IStepToolOutputRepository _stepToolOutputRepository = stepToolOutputRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository = stepToolExecutionRepository;
        private readonly IExecutionServices _executionServices = executionServices;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        /// <summary>
        /// Processes the specified API output message, updates related execution and document history records, and
        /// returns an automation services data transfer object.
        /// </summary>
        /// <param name="outputDto">The API output data to process. Must contain a valid execution identifier and associated output information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="AutomationServicesDto"/> with details about the processed automation service.</returns>
        /// <exception cref="AppException">Thrown if the execution specified by <paramref name="outputDto"/> does not exist.</exception>
        public async Task<AutomationServicesDto> ProcessMessage(ApiOutputDto outputDto)
        {
            var execution = await _stepToolExecutionRepository.FindByIdAsync(outputDto.ExecutionId) ?? throw new AppException(ErrorCode.NotFound, "StepToolExecution not found", null);

            var content = JsonSerializer.Serialize(new
            {
                outputDto.TemplateName,
                outputDto.StatusCode,
                outputDto.Content
            }, _jsonOptions);

            var stepToolOutput = new StepToolOutput(
                0,
                DateTime.Now,
                execution.StepToolId,
                execution.CardId,
                content
            );

            await _stepToolOutputRepository.CreateAsync(stepToolOutput);

            await _executionServices.HandleExecutionProgress(execution, outputDto.Email);

            return new AutomationServicesDto
            (
                execution.StepToolId,
                execution.CardId,
                outputDto.Tenant,
                outputDto.Email,
                execution.Card!.Document!.ReferenceFile,
                0
            );
        }
    }
}
