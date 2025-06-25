using FluentValidation;
using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Validations.Models
{
    public class DocumentNormalizedValidator : AbstractValidator<DocumentNormalized>
    {
        public DocumentNormalizedValidator()
        {
            RuleFor(i => i.Content).NotEmpty();
        }
    }
}