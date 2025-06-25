using FluentValidation;
using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Validations.Models
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
