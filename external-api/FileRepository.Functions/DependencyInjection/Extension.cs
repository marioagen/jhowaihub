using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FileRepository.Functions.DependencyInjection
{
    public static class Extension
    {

        public static IConfiguration GetConfiguration(this IFunctionsHostBuilder builder)
        {
            return builder.GetContext().Configuration;
        }

        public static IConfiguration ConfigureAppSettings(this IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), true, false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), true, false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), true, false)
                .AddEnvironmentVariables();

            return builder.ConfigurationBuilder.Build();
        }
    }
}
