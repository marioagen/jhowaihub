using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoopiAiHub.Application.DependencyInjection;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Repository.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddRepository(configuration);

        services.AddExternalApi(configuration);

        services.AddScoped<IUsageAggregationService, UsageAggregationService>();
        services.AddScoped<IUsageArchiveService, UsageArchiveService>();
    })
    .Build();

host.Run();
