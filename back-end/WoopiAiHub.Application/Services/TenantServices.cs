using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Repository.Util;
using static System.Formats.Asn1.AsnWriter;

namespace WoopiAiHub.Application.Services
{
    public class TenantServices : ITenantServices
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IKeyGeneratorApi _keyGeneratorApi;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TenantServices> _logger;
        private readonly IMapper _mapper;

        public TenantServices(ITenantRepository tenantRepository,
                              IServiceProvider serviceProvider,
                              ICoreDependencies coreDependencies,
                              IApiDependencies apiDependencies,
                              IUserRepository userRepository,
                              ILogger<TenantServices> logger
                            )
        {
            _configuration = coreDependencies.Configuration;
            _httpContextAcessor = coreDependencies.HttpContextAccessor;
            _tenantRepository = tenantRepository;
            _marketPlaceApi = apiDependencies.MarketPlaceApi;
            _keyGeneratorApi = apiDependencies.KeyGeneratorApi;
            _serviceProvider = serviceProvider;
            _mapper = coreDependencies.Mapper;
            _userRepository = userRepository;
            _logger = logger;
        }

        public void ProcessSubscription(TenantSubscriptionDto tenantActivationDto)
        {
            switch(tenantActivationDto.Action)
            {
                case SubscriptionAction.Activate:
                    CreateTenant(tenantActivationDto);
                    break;
                case SubscriptionAction.Deactivate:
                    // DeactivateTenant(tenantActivationDto);
                    break;
                case SubscriptionAction.Reactivate:
                    // ReactivateTenant(tenantActivationDto);
                    break;
                case SubscriptionAction.ChangePlan:
                    // ChangePlanTenant(tenantActivationDto);
                    break;
                case SubscriptionAction.Renew:
                    // RenewSubscrition(tenantActivationDto);
                    break;
                default:
                    throw new ArgumentException($"Unknown action: {tenantActivationDto.Action}");
            }
        }

        private void CreateTenant(TenantSubscriptionDto tenantActivationDto)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();


                //var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                //httpAccessor.HttpContext ??= new DefaultHttpContext();

                //if (httpAccessor.HttpContext != null)
                //{
                var template = _configuration.GetConnectionString("TemplateConnection");
                var connectionString = template?.Replace("___NEWDB___", tenantActivationDto.Name);
                if (_httpContextAcessor.HttpContext != null)
                {
                    _httpContextAcessor.HttpContext.Items["TenantConnection"] = connectionString;
                }

                var result = _tenantRepository.CreateDatabase();
                    if (result)
                    {
                        
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        dbContext.Database.GetDbConnection().ConnectionString = connectionString;
                        var tenant = new Tenant(
                                0,
                                DateTime.Now,
                                tenantActivationDto.Name,
                                tenantActivationDto.MarketplaceId!,
                                true,
                                tenantActivationDto.PlanName,
                                tenantActivationDto.DateStart,
                                tenantActivationDto.DateEnd,
                                tenantActivationDto.DateRenew,
                                string.Empty
                            );
                        var resultTenant = _tenantRepository.CreateUniqueTenant(tenant);
                        if (resultTenant)
                        {
                            var user = new User(
                                    Guid.NewGuid(),
                                    tenant.Name!,
                                    tenantActivationDto.Email,
                                    true,
                                    DateTime.Now
                                );
                            var resultUser = _userRepository.CreateAsync(user);
                        }
                    }
             //   }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating tenant {TenantName}", tenantActivationDto.Name);
                throw;
            }
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