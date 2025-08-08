using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace WoopiAiHub.Application.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IServiceCollection, ServiceCollection>();
            services.AddScoped<IDocumentServices, DocumentServices>();
            services.AddScoped<IDocumentHistoryServices, DocumentHistoryServices>();
            services.AddScoped<IDocumentNormalizedServices, DocumentNormalizedServices>();
            services.AddScoped<IAccountServices, AccountServices>();
            services.AddScoped<ITenantServices, TenantServices>();
            services.AddScoped<IOcrGoogle, OcrGoogle>();
            services.AddScoped<IOcrAzure, OcrAzure>();
            services.AddScoped<IQuestionnaireServices, QuestionnaireServices>();
            services.AddScoped<IQuestionServices, QuestionServices>();
            services.AddScoped<ITypeDocServices, TypeDocServices>();
            services.AddScoped<ICoreDependencies, CoreDependencies>();
            services.AddScoped<IApiDependencies, ApiDependencies>();
            services.AddScoped<ITeamServices, TeamServices>();
            services.AddScoped<IProfileServices, ProfileServices>();
            services.AddScoped<IPermissionServices, PermissionServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IProfileServices, ProfileServices>();
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
            services.AddScoped<IRefreshTokenServices, RefreshTokenServices>();
            services.AddScoped<IStatusServices, StatusServices>();
            services.AddScoped<IWorkflowServices, WorkflowServices>();
            services.AddScoped<ICardServices, CardServices>();
            services.AddLogging();
            services.AddMemoryCache();

            return services;
        }
    }
}
