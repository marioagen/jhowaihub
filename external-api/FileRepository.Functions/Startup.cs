using FileRepository.Application.DependencyInjection;
using FileRepository.Functions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FileRepository.Functions.Startup))]
namespace FileRepository.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var configuration = builder.ConfigureAppSettings();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetConfiguration();

            builder.Services
                .AddApplication(configuration);
        }

    }
}
