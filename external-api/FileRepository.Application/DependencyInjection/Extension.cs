using Azure.Storage.Blobs;
using FileRepository.Application.Services;
using FileRepository.Domain.Interfaces;
using FileRepository.Domain.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileRepository.Application.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFileService, FileService>();

            services.AddLogging();

            var azureStorageSection = configuration.GetSection(nameof(AzureStorageOptions));
            var azureStorageOptions = azureStorageSection.Get<AzureStorageOptions>();

            if (string.IsNullOrWhiteSpace(azureStorageOptions.ConnectionString))
                throw new ArgumentNullException($"{nameof(AzureStorageOptions)}_{nameof(azureStorageOptions.ConnectionString)}");
            else if (string.IsNullOrWhiteSpace(azureStorageOptions.ContainerName))
                throw new ArgumentNullException($"{nameof(AzureStorageOptions)}_{nameof(azureStorageOptions.ContainerName)}");

            services.AddScoped(serviceProvider =>
            {
                var client = new BlobContainerClient(azureStorageOptions.ConnectionString, azureStorageOptions.ContainerName);
                return client;
            });

            return services;
        }
    }
}
