using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DocAnalyzer.Domain.Interfaces.Repository;
using DocAnalyzer.Repository.Context;
using DocAnalyzer.Domain.Interfaces.Utils;
using DocAnalyzer.Domain.Interfaces.Repository.Cache;
using DocAnalyzer.Repository.Cache;

namespace DocAnalyzer.Repository.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentHistoryRepository, DocumentHistoryRepository>();
            services.AddScoped<IDocumentNormalizedRepository, DocumentNormalizedRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IQuestionQuestionnaireRepository, QuestionQuestionnaireRepository>();
            services.AddScoped<ITypeDocRepository, TypeDocRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IServiceProvider, ServiceProvider>();
            services.AddScoped<ITenantCacheServices, TenantCacheService>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                string? connectionString = configuration.GetConnectionString("TemplateConnection");
                options.UseSqlServer(connectionString);
            });

            return services;
        }
    }
}