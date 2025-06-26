using FluentValidation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Validations.Models
{
    public class DocumentHistoryValidator : AbstractValidator<DocumentHistory>
    {
        public DocumentHistoryValidator()
        {
            RuleFor(i => i.Input).NotEmpty();
            RuleFor(i => i.Output).NotEmpty();
        }
    }
}
