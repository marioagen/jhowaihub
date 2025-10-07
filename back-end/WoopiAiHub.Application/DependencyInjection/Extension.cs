using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.Application.Messaging;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;

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
            services.AddScoped<IValidateStep, ValidateStep>();
            services.AddScoped<IValidateWorkflow, ValidateWorkflow>();
            services.AddScoped<ICardServices, CardServices>();
            services.AddScoped<IToolServices, ToolServices>();
            services.AddScoped<IToolTypeServices, ToolTypeServices>();
            services.AddScoped<IToolDataServices, ToolDataServices>();
            services.AddScoped<IToolFactoryHandler, ToolFactoryHandler>();
            services.AddScoped<IToolOutputServices, ToolOutputServices>();
            services.AddScoped<IAutomationServices, AutomationServices>();
            services.AddScoped<IPromptServices, PromptServices>();
            services.AddScoped<IValidatePrompt, ValidatePrompt>();

            services.AddHostedService<OcrConsumer>();
            services.AddHostedService<DocumentEmbeddingsConsumer>();

            services.AddLogging();
            services.AddMemoryCache();

            return services;
        }
    }
}
