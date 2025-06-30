using AutoMapper;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Repository.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WoopiAiHub.Application.Services
{
    public class TenantServices : ITenantServices
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IKeyGeneratorApi _keyGeneratorApi;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;

        public TenantServices(ITenantRepository tenantRepository,
                              IServiceProvider serviceProvider,
                              ICoreDependencies coreDependencies,
                              IApiDependencies apiDependencies
                            )
        {
            _configuration = coreDependencies.Configuration;
            _httpContextAcessor = coreDependencies.HttpContextAccessor;
            _tenantRepository = tenantRepository;
            _marketPlaceApi = apiDependencies.MarketPlaceApi;
            _keyGeneratorApi = apiDependencies.KeyGeneratorApi;
            _serviceProvider = serviceProvider;
            _mapper = coreDependencies.Mapper;
        }

        /// <summary>
        /// Makes the request to obtain the tenants that the user has enabled in the Marketplace
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> FindAllByUserEmail(string email)
        {
            var tenants = await _marketPlaceApi.FindTenantsByUserEmail(_configuration["keyAccess"]!,
                                                                                        email);

            return tenants;
        }

        /// <summary>
        /// Calls the key service to obtain the tenant's key and applies the migrations
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> InitializeTenant(string tenant)
        {
            if (string.IsNullOrEmpty(tenant))
            {
                throw new ArgumentException("Tenant name cannot be null or empty.", nameof(tenant));
            }

            var keyAccess = _configuration.GetSection("KeyAccess").Get<string>();

            if (string.IsNullOrEmpty(keyAccess))
            {
                throw new InvalidOperationException("KeyAccess is not configured in the application settings.");
            }

            string result = await _keyGeneratorApi.GetKey(keyAccess, tenant);

            await ApplyMigrations(keyAccess,
                                  tenant,
                                  ColTypeModule.WoopiAiHub);

            return result;
        }

        /// <summary>
        /// Apply ApplicationDbContext Migrations
        /// </summary>
        private async Task ApplyMigrations(string keyAccess,
                                           string tenantName,
                                           ColTypeModule module)
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                throw new ArgumentException("Tenant name cannot be null or empty.", nameof(tenantName));
            }

            var tenant = await _marketPlaceApi.FindTenantByNameAndModule(keyAccess,
                                                                         tenantName,
                                                                         module);

            if (tenant == null)
            {
                throw new InvalidOperationException($"Tenant '{tenantName}' not found.");
            }

            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenant.DatabaseName);
            if (_httpContextAcessor.HttpContext != null)
            {
                _httpContextAcessor.HttpContext.Items["TenantConnection"] = connectionString;
            }

            var result = _tenantRepository.CreateDatabase();
            if (result)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.GetDbConnection().ConnectionString = connectionString;
                InitApplicationDb.RunApplicationMigration(dbContext);
            }
            else
            {
                throw new InvalidOperationException($"Tenant '{tenantName}' not found.");
            }
        }
    }
}