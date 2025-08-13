using System.Linq;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.Repository;

namespace WoopiAiHub.Application.Utils
{
    public class ValidateStep : IValidateStep
    {
        private readonly ICardRepository _cardRepository;

        public ValidateStep(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        /// <summary>
        /// Validates the creation of steps in a workflow.
        /// </summary>
        /// <param name="stepsCreateDto"></param>
        /// <exception cref="AppException"></exception>
        public void ValidateCreateStep(ICollection<StepCreateDto> stepsCreateDto)
        {
            if (stepsCreateDto == null || stepsCreateDto.Count == 0)
            {
                throw new AppException(ErrorCode.RequiredField, "Workflow must have at least one step", StepLabel.Required);
            }

            var steps = stepsCreateDto.Select(s => new Step(0, DateTime.UtcNow, 0, s.Name, s.Order, s.ProfileId, s.StatusId)).ToList();

            ValidateNames(steps);

            ValidateOrder(steps);
        }

        /// <summary>
        /// Validates the update of steps in a workflow.
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="stepsUpdateDto"></param>
        /// <exception cref="AppException"></exception>
        public void ValidateUpdateStep(Workflow workflow, ICollection<StepUpdateDto> stepsUpdateDto)
        {
            if (stepsUpdateDto == null || stepsUpdateDto.Count == 0)
            {
                throw new AppException(ErrorCode.RequiredField, "Workflow must have at least one step", StepLabel.Required);
            }

            var steps = stepsUpdateDto.Select(s => new Step(s.Id, DateTime.UtcNow, workflow.Id, s.Name, s.Order, s.ProfileId, s.StatusId)).ToList();

            ValidateNames(steps);

            ValidateOrder(steps);
        }

        /// <summary>
        /// Validates if steps can be deleted.
        /// </summary>
        /// <param name="stepIds"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task ValidateDeleteStep(ICollection<int> stepIds)
        {
            var existingStepsInUse = await _cardRepository.ExistsStepsInUse(stepIds);
            if (existingStepsInUse)
            {
                throw new AppException(ErrorCode.Conflict, "Cannot delete steps that are in use by cards", StepLabel.StepsInUse);
            }
        }

        /// <summary>
        /// Validates the order of steps in a workflow.
        /// </summary>
        /// <param name="steps"></param>
        /// <exception cref="AppException"></exception>
        private static void ValidateOrder(List<Step> steps)
        {
            var duplicate = steps
                    .GroupBy(s => s.Order)
                    .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
            {
                throw new AppException(
                    ErrorCode.Conflict,
                    $"Step order '{duplicate.Key}' is already used in this workflow",
                    StepLabel.OrderInvalid
                );
            }
        }

        /// <summary>
        /// Validates the names of steps in a workflow.
        /// </summary>
        /// <param name="steps"></param>
        /// <exception cref="AppException"></exception>
        private static void ValidateNames(List<Step> steps)
        {
            var stepNames = new HashSet<string>();
            foreach (var step in steps)
            {
                if (string.IsNullOrEmpty(step.Name))
                {
                    throw new AppException(ErrorCode.RequiredField, "Step name cannot be empty", StepLabel.NameRequired);
                }
                if (step.Order < 0)
                {
                    throw new AppException(ErrorCode.InvalidValue, "Step order must be a non-negative integer", StepLabel.OrderInvalid);
                }
                if (!stepNames.Add(step.Name))
                {
                    throw new AppException(ErrorCode.Conflict, $"Step name '{step.Name}' is already used in this workflow", StepLabel.NameAlreadyExists);
                }
            }
        }

    }
}
