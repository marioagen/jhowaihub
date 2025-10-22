using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Utils
{
    public class ValidatePrompt : IValidatePrompt
    {
        private readonly IPromptRepository _promptRepository;
        private readonly IUserServices _userServices;

        public ValidatePrompt(IPromptRepository promptRepository,
                              IUserServices userServices)
        {
            _promptRepository = promptRepository;
            _userServices = userServices;
        }

        /// <summary>
        /// Validates if the specified user is the owner of the prompt.
        /// </summary>
        /// <exception cref="PromptNotFoundException">Thrown when the prompt does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not the owner.</exception>
        public void ValidateOwnership(int promptId,
                                      string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);

            if (idUser == Guid.Empty)
                throw new ArgumentException("User id cannot be empty.", nameof(idUser));

            var prompt = _promptRepository.FindById(promptId)
                ?? throw new KeyNotFoundException($"Prompt with ID {promptId} not found.");
        }

        /// <summary>
        /// Validate the prompt: required fields cannot be null or empty, and the variables must be valid
        /// </summary>
        /// <param name="prompt"></param>
        /// <exception cref="AppException"></exception>e
        public bool ValidatePromptFields(Prompt prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt.Name) ||
                string.IsNullOrWhiteSpace(prompt.Description) ||
                string.IsNullOrWhiteSpace(prompt.Text))
            {
                return false;
            }
            return true;
        }
    }
}
