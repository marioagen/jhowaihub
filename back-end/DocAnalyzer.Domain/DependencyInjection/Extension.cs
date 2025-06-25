using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using DocAnalyzer.Application.Dto;
using DocAnalyzer.Domain.Models;
using DocAnalyzer.Domain.Validations.DTOs;
using DocAnalyzer.Domain.Validations.Models;
using DocAnalyzer.Domain.Utils;
using DocAnalyzer.Domain.DTOs;

namespace DocAnalyzer.Domain.DependencyInjection
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
