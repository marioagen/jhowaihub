using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IValidatePrompt
    {
        void ValidateOwnership(int promptId,
                              string emailCreator);
        bool ValidateRequiredPromptFields(Prompt prompt);
    }
}
