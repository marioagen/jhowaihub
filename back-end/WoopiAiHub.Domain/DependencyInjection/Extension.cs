using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Validations.DTOs;
using WoopiAiHub.Domain.Validations.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            //Models validations
            services.AddScoped<IValidator<DocumentHistory>, DocumentHistoryValidator>();
            services.AddScoped<IValidator<DocumentNormalized>, DocumentNormalizedValidator>();
            //Dto validations
            services.AddScoped<IValidator<RequestCreateDocumentDto>, DocumentDtoValidator>();

            return services;
        }
    }
}
