using FluentValidation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Validations.Models
{
    public class DocumentNormalizedValidator : AbstractValidator<DocumentNormalized>
    {
        public DocumentNormalizedValidator()
        {
            RuleFor(i => i.Content).NotEmpty();
        }
    }
}