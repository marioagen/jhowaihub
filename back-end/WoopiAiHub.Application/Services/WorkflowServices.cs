using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class WorkflowServices : IWorkflowServices
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateWorkflow _validateWorkflow;
        private readonly IValidateStep _validateStep;
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IStepToolParameterRepository _stepParameterRepository;
        private const string NotFoundMessage = "Workflow not found";

        public WorkflowServices(IWorkflowRepository workflowRepository,
                                IProfileRepository profileRepository,
                                IStatusRepository statusRepository,
                                IStepRepository stepRepository,
                                IUnitOfWork unitOfWork,
                                IValidateStep validateStep,
                                IValidateWorkflow validateWorkflow,
                                IStepToolRepository stepToolRepository,
                                IStepToolOutputRepository stepToolOutputRepository,
                                IStepToolParameterRepository stepToolParameterRepository)
        {
            _workflowRepository = workflowRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _stepRepository = stepRepository;
            _unitOfWork = unitOfWork;
            _validateStep = validateStep;
            _validateWorkflow = validateWorkflow;
            _stepToolRepository = stepToolRepository;
            _stepParameterRepository = stepToolParameterRepository;
        }

        /// <summary>
        /// Creates a new workflow for a specific team.
        /// </summary>
        /// <param name="workflowCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> Create(WorkflowCreateDto workflowCreateDto)
        {
            await _validateWorkflow.ValidateCreateWorkflow(workflowCreateDto);

            _validateStep.ValidateCreateStep(workflowCreateDto.Steps);

            var workflow = new Workflow(0, DateTime.UtcNow, workflowCreateDto.TeamId, workflowCreateDto.Name);

            ICollection<Step> steps = await CreateStepsAndValidate(workflowCreateDto.Steps, workflow.TeamId);

            workflow.AddSteps(steps);

            return await _workflowRepository.Create(workflow);
        }

        /// <summary>
        /// Updates an existing workflow.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> Update(WorkflowUpdateDto workflowUpdateDto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var workflow = await _validateWorkflow.ValidateUpdateWorkflow(workflowUpdateDto);

                _validateStep.ValidateUpdateStep(workflow, workflowUpdateDto.Steps);

                DeleteSteps(workflowUpdateDto, workflow);

               // await UpdateSteps(workflowUpdateDto);

                ICollection<Step> stepsAdd = await CreateStepsAndValidate(workflowUpdateDto.Steps.ToList(), 
                                                                          workflow.TeamId);

                workflow.AddSteps(stepsAdd);

                await _workflowRepository.Update(workflow);

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<WorkflowDto> FindById(int id)
        {
            var workflow = await _workflowRepository.FindById(id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }
            return workflow;
        }

        /// <summary>
        /// Retrieves a workflow associated with a specific team ID.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<WorkflowDto> FindByTeamId(int teamId, WorkflowFilterDto workflowFilterDto)
        {
            var workflow = await _workflowRepository.FindByTeamId(teamId, workflowFilterDto);

            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            int totalCards = workflow.Steps.Sum(step => step.Cards.Count);
            workflow.NumDocuments = totalCards;

            return workflow;
        }

        /// <summary>
        /// Deletes a workflow by its ID, including all associated steps.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> DeleteById(int id)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var workflow = await _workflowRepository.FindByIdReturnModel(id);
                if (workflow == null)
                {
                    throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
                }

                var stepIds = workflow.Steps.Select(s => s.Id).ToList();
                await _validateStep.ValidateDeleteStep(stepIds);

                _stepRepository.DeleteByIds(stepIds);
                await _workflowRepository.DeleteById(id);
                
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Validates that the profile and status associated with a step exist.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<bool> ValidateProfileAndStatus(Step step)
        {
            var profile = await _profileRepository.FindById(step.ProfileId);
            if (profile == null)
            {
                throw new AppException(ErrorCode.NotFound, "Profile not found", ProfileLabel.NotFound);
            }
            var status = await _statusRepository.FindById(step.StatusId);
            if (status == null)
            {
                throw new AppException(ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);
            }
            return true;
        }

        /// <summary>
        /// Deletes steps that are no longer present in the updated workflow DTO.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private void DeleteSteps(WorkflowUpdateDto workflowUpdateDto, Workflow workflow)
        {
            var stepIds = workflow.Steps.Select(s => s.Id).ToList();
            var stepToolIds = workflow.Steps.SelectMany(s => s.StepTools).Select(st => st.Id).ToList();
            var parameterIds = workflow.Steps.SelectMany(s => s.StepTools).SelectMany(st => st.Parameters).Select(p => p.Id).ToList();

            _stepParameterRepository.DeleteByIds(parameterIds);
            _stepToolRepository.DeleteByIds(stepToolIds);
            _stepRepository.DeleteByIds(stepIds);
        }

        /// <summary>
        /// Process StepTools deletion or inclusion 
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stepToolUpdateDtos"></param>
        /// <returns></returns>
        public async Task ProcessStepTools(Step step, ICollection<StepToolUpdateDto> stepToolUpdateDtos)
        {
            var stepToolsInsert = new List<StepTool>();
            foreach (var stepToolUpdate in stepToolUpdateDtos)
            {
                var stepTool = new StepTool(
                                            0,
                                            DateTime.Now,
                                            step.Id,
                                            stepToolUpdate.ToolId,
                                            stepToolUpdate.Order,
                                            stepToolUpdate.PositionX,
                                            stepToolUpdate.PositionY
                                        );
                if (stepToolUpdate.DependsOnStepToolId.HasValue)
                {
                    stepTool.UpdateDependencyStepToolId(stepToolUpdate.DependsOnStepToolId.Value);
                }

                if (!string.IsNullOrEmpty(stepToolUpdate.Input))
                {
                    stepTool.Parameters.Add(new StepToolParameter(0, DateTime.Now, 0, stepToolUpdate.Input));
                }

                stepToolsInsert.Add(stepTool);
            }

            await _stepToolRepository.CreateRangeAsync(stepToolsInsert);
        }

        /// <summary>
        /// Creates a collection of Step entities from the provided DTOs and associates them with the given teamId.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stepsDto"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        private async Task<ICollection<Step>> CreateStepsAndValidate<T>(IEnumerable<T> stepsDto, int teamId) where T : IStepDto
        {
            var steps = new List<Step>();
            StepTool? lastStepTool = null; // último StepTool criado (de qualquer Step)

            foreach (var stepDto in stepsDto)
            {
                var step = new Step(
                    0,
                    DateTime.UtcNow,
                    teamId,
                    stepDto.Name,
                    stepDto.Order,
                    stepDto.ProfileId,
                    stepDto.StatusId);

                StepTool? previousStepToolInSameStep = null;

                foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                {
                    var stepTool = new StepTool(
                        0,
                        DateTime.Now,
                        0,
                        stepToolDto.ToolId,
                        stepToolDto.Order,
                        stepToolDto.PositionX,
                        stepToolDto.PositionY);

                    if (!string.IsNullOrEmpty(stepToolDto.Input))
                    {
                        stepTool.Parameters.Add(
                            new StepToolParameter(0, DateTime.Now, 0, stepToolDto.Input));
                    }

                    // 1) Se for o primeiro do step, e existir um "último do step anterior", ele depende dele
                    if (previousStepToolInSameStep == null && lastStepTool != null)
                    {
                        stepTool.DependsOnStepTool = lastStepTool;
                    }
                    // 2) Se não for o primeiro do step, depende do anterior dentro do mesmo step
                    else if (previousStepToolInSameStep != null)
                    {
                        stepTool.DependsOnStepTool = previousStepToolInSameStep;
                    }

                    step.AddStepTool(stepTool);

                    // Atualiza controles
                    previousStepToolInSameStep = stepTool;
                    lastStepTool = stepTool;
                }

                steps.Add(step);
            }

            foreach (var step in steps)
            {
                await ValidateProfileAndStatus(step);
            }

            return steps;
        }

        /// <summary>
        /// Find all workflows associated with a user, based on their email.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICollection<WorkflowDto> FindAllByUser(string email)
        {
            var workflow = _workflowRepository.FindAllByUser(email);
            return workflow;
        }
    }
}
