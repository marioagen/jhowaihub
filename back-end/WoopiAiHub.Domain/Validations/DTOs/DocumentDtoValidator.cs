using FluentValidation;
using WoopiAiHub.Application.Dto;

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
        }
    }
}