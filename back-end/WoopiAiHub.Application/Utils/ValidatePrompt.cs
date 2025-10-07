using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Utils
{
    public class ValidatePrompt : IValidatePrompt
    {
        private readonly IPromptRepository _promptRepository;

        public ValidatePrompt(IPromptRepository promptRepository)
        {
            _promptRepository = promptRepository;
        }

        /// <summary>
        /// Validates if the specified user is the owner of the prompt.
        /// </summary>
        /// <exception cref="PromptNotFoundException">Thrown when the prompt does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not the owner.</exception>
        public void ValidateOwnership(int promptId,
                                      Guid idUser)
        {
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
        public void ValidatePromptFields(Prompt prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt.Name) ||
                string.IsNullOrWhiteSpace(prompt.Description) ||
                string.IsNullOrWhiteSpace(prompt.Text))
            {
                //throw new AppException(ErrorCode.RequiredField, "Required field cannot be null or empty.");
            }
        }
    }
}
