using FluentValidation;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Validations.DTOs
{
    public class DocumentDtoValidator : AbstractValidator<RequestCreateDocumentDto>
    {
        public DocumentDtoValidator()
        {
            RuleFor(i => i.Chunk).NotEmpty();
            RuleFor(i => i.Name).NotEmpty();
            RuleFor(i => i.Description).NotEmpty();
            RuleFor(i => i.EmailCreator).NotEmpty();
            RuleFor(i => i.IsLast).NotEmpty();
            RuleFor(i => i.Filename).NotEmpty();
            RuleFor(i => i.ExtractionMode)
                .Must(mode => string.IsNullOrWhiteSpace(mode) || DocumentExtractionModes.IsValid(mode))
                .WithMessage("Invalid extraction mode.");
        }
    }
}