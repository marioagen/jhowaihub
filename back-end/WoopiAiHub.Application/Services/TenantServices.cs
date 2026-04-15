using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Repository.Util;

namespace WoopiAiHub.Application.Services
{
    public class TenantServices : ITenantServices
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantCacheServices _tenantCacheService;

        public TenantServices(ITenantRepository tenantRepository,
                              IServiceProvider serviceProvider,
                              ICoreDependencies coreDependencies,
                              IApiDependencies apiDependencies,
                              ITenantCacheServices tenantCacheService
                            )
        {
            _configuration = coreDependencies.Configuration;
            _httpContextAcessor = coreDependencies.HttpContextAccessor;
            _tenantRepository = tenantRepository;
            _marketPlaceApi = apiDependencies.MarketPlaceApi;
            _serviceProvider = serviceProvider;
            _tenantCacheService = tenantCacheService;
        }

        /// <summary>
        /// Process Marketplace sub
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ProcessSubscription(TenantSubscriptionDto tenantSubscriptionDto)
        {
            var connectionString = SetConnectionString(tenantSubscriptionDto);

            var result = _tenantRepository.CreateDatabase();
            if (result)
            {
                SeedInitialData(tenantSubscriptionDto, connectionString);
                _marketPlaceApi.SendDatabaseCreatedNotification(_configuration["keyAccess"]!,
                                               tenantSubscriptionDto.Name!);
            }
        }

        /// <summary>
        /// Set connection string and return it 
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        /// <returns></returns>
        private string? SetConnectionString(TenantSubscriptionDto tenantSubscriptionDto)
        {
            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenantSubscriptionDto.DataBaseName);

            _httpContextAcessor!.HttpContext ??= new DefaultHttpContext();
            _httpContextAcessor.HttpContext.Items["TenantConnection"] = connectionString;
            return connectionString;
        }

        /// <summary>
        /// Initialize data
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        /// <param name="connectionString"></param>
        private void SeedInitialData(TenantSubscriptionDto tenantSubscriptionDto, string? connectionString)
        {
            var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.GetDbConnection().ConnectionString = connectionString;

            var password = tenantSubscriptionDto.Password;
            if (string.IsNullOrEmpty(password))
            {
                password = tenantSubscriptionDto.Name;
            }

            var userCreateDto = new UserCreateDto
            {
                Name = tenantSubscriptionDto.Name!,
                Email = tenantSubscriptionDto.Email,
                Password = password,
                TeamIds = new List<int> { 1 }
            };
            var headerDto = new HeadersDto
            {
                Tenant = tenantSubscriptionDto.Name!
            };

            var userService = scope.ServiceProvider.GetRequiredService<IUserServices>();
            userService.Create(userCreateDto, headerDto);
        }

        /// <summary>
        /// Finds the plan associated with the tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<DashboardTenantInfo> FindPlanByName(string tenant)
        {
            var tenantInfo = await _tenantCacheService.FindTenantAsync(tenant);
            return new DashboardTenantInfo
            {
                Plan = tenantInfo?.Plan ?? string.Empty,
                WtcIncluded = tenantInfo?.WtcsIncluded ?? 0
            };
        }

        /// <summary>
        /// Calls the key service to obtain the tenant's key and applies the migrations
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task InitializeTenant(string tenant)
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

            await ApplyMigrations(keyAccess, tenant);
        }

        /// <summary>
        /// Apply ApplicationDbContext Migrations
        /// </summary>
        private async Task ApplyMigrations(string keyAccess, string tenantName)
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                throw new ArgumentException("Tenant name cannot be null or empty.", nameof(tenantName));
            }

            var tenant = await _marketPlaceApi.FindTenantByName(keyAccess,tenantName);

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

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.GetDbConnection().ConnectionString = connectionString;
            InitApplicationDb.RunApplicationMigration(dbContext);
        }
    }
}
