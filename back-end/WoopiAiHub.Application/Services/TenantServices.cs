using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
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
        private readonly IKeyGeneratorApi _keyGeneratorApi;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TenantServices> _logger;

        public TenantServices(ITenantRepository tenantRepository,
                              IServiceProvider serviceProvider,
                              ICoreDependencies coreDependencies,
                              IApiDependencies apiDependencies,
                              ILogger<TenantServices> logger
                            )
        {
            _configuration = coreDependencies.Configuration;
            _httpContextAcessor = coreDependencies.HttpContextAccessor;
            _tenantRepository = tenantRepository;
            _marketPlaceApi = apiDependencies.MarketPlaceApi;
            _keyGeneratorApi = apiDependencies.KeyGeneratorApi;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Process Marketplace sub
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ProcessSubscription(TenantSubscriptionDto tenantSubscriptionDto)
        {
            switch(tenantSubscriptionDto.Action)
            {
                case SubscriptionAction.Activate:
                    CreateDatabaseAndSeedData(tenantSubscriptionDto);
                    break;
                case SubscriptionAction.Deactivate:
                    SetActive(tenantSubscriptionDto, false);
                    break;
                case SubscriptionAction.Reactivate:
                    SetActive(tenantSubscriptionDto, true);
                    break;
                case SubscriptionAction.ChangePlan:
                    ChangeSubscriptionPlan(tenantSubscriptionDto);
                    break;
                case SubscriptionAction.Renew:
                     RenewSubscrition(tenantSubscriptionDto);
                    break;
                default:
                    throw new ArgumentException($"Unknown action: {tenantSubscriptionDto.Action}");
            }
        }

        /// <summary>
        /// Update subscription dates
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        private void RenewSubscrition(TenantSubscriptionDto tenantSubscriptionDto)
        {
            SetConnectionString(tenantSubscriptionDto);
            var tenant = _tenantRepository.FindByMarketPlaceId(tenantSubscriptionDto.MarketplaceId);
            if (tenant is not null)
            {
                tenant.SetSubscriptionDates(tenantSubscriptionDto.DateStart,
                                            tenantSubscriptionDto.DateEnd,
                                            tenantSubscriptionDto.DateRenew);
                _tenantRepository.Update(tenant);
            }
        }

        /// <summary>
        /// Update plan
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        private void ChangeSubscriptionPlan(TenantSubscriptionDto tenantSubscriptionDto)
        {
            SetConnectionString(tenantSubscriptionDto);
            var tenant = _tenantRepository.FindByMarketPlaceId(tenantSubscriptionDto.MarketplaceId);
            if (tenant is not null)
            {
                tenant.SetPlanName(tenantSubscriptionDto.PlanName);
                _tenantRepository.Update(tenant);
            }
        }

        /// <summary>
        /// Set IsActive
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        /// <param name="active"></param>
        private void SetActive(TenantSubscriptionDto tenantSubscriptionDto, bool active)
        {
            SetConnectionString(tenantSubscriptionDto);
            var tenant = _tenantRepository.FindByMarketPlaceId(tenantSubscriptionDto.MarketplaceId);
            if (tenant is not null)
            {
                tenant.SetActive(active);
                _tenantRepository.Update(tenant);
            }
        }

        /// <summary>
        /// Creates a new database and seed initial data
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        private void CreateDatabaseAndSeedData(TenantSubscriptionDto tenantSubscriptionDto)
        {
            try
            {
                var connectionString = SetConnectionString(tenantSubscriptionDto);

                var result = _tenantRepository.CreateDatabase();
                if (result)
                {
                    SeedInitialData(tenantSubscriptionDto, connectionString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating tenant {TenantName}", tenantSubscriptionDto.Name);
                throw;
            }
        }

        private string? SetConnectionString(TenantSubscriptionDto tenantSubscriptionDto)
        {
            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenantSubscriptionDto.Name);

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

            if (CreateTenant(tenantSubscriptionDto))
            {
                var userCreateDto = new UserCreateDto
                {
                    Name = tenantSubscriptionDto.Name!,
                    Email = tenantSubscriptionDto.Email,
                    Password = tenantSubscriptionDto.Email,
                    TeamIds = new List<int> { 1 }
                };
                var headerDto = new HeadersDto
                {
                    Tenant = tenantSubscriptionDto.Name!
                };

                var userService = scope.ServiceProvider.GetRequiredService<IUserServices>();
                userService.Create(userCreateDto, headerDto);
            }
        }

        /// <summary>
        /// Create tenant
        /// </summary>
        /// <param name="tenantSubscriptionDto"></param>
        /// <returns></returns>
        private bool CreateTenant(TenantSubscriptionDto tenantSubscriptionDto)
        {
            var tenant = new Tenant(
                    0,
                    DateTime.Now,
                    tenantSubscriptionDto.Name,
                    tenantSubscriptionDto.MarketplaceId!,
                    true,
                    tenantSubscriptionDto.PlanName,
                    tenantSubscriptionDto.DateStart,
                    tenantSubscriptionDto.DateEnd,
                    tenantSubscriptionDto.DateRenew,
                    string.Empty
                );
            return _tenantRepository.CreateUniqueTenant(tenant);
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

            await ApplyMigrations(keyAccess, tenant);

            return result;
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