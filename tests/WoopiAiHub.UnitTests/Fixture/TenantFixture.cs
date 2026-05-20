using Bogus;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Repository.Context;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class TenantFixture
    {
        public const string ValidUserEmail = "user@woopi.test";

        public static FindPagedRequestDto FindValidFindPagedRequestDto()
        {
            return new FindPagedRequestDto()
            {
                Search = "tenant",
                Page = 1,
            };
        }

        public TenantInfoDto FindValidTenantInfoDto()
        {
            var faker = new Faker<TenantInfoDto>("pt_BR")
              .CustomInstantiator(f => new TenantInfoDto
              {
                  ChunkSize = f.Random.Int(),
                  DatabaseName = f.Random.String(),
                  Email = f.Random.String(),
                  EmbeddingModelName = f.Random.String(),
                  KValue = f.Random.Int(),
                  MaxTokens = f.Random.Int(),
                  Model = f.Random.String(),
                  Name = f.Random.String(),
                  OcrModel = f.Random.String(),
                  RagProvider = f.PickRandom<RagProvider>(),
                  RefineTemplate = f.Random.String(),
                  SearchMode = f.Random.String(),
                  Template = f.Random.String(),
              });

            return faker;
        }

        public static TenantListDto FindValidTenantListDto()
        {
            var faker = new Faker<TenantListDto>("pt_BR")
              .CustomInstantiator(f => new TenantListDto
              {
                  Name = f.Company.CompanyName(),
                  DatabaseName = f.Random.AlphaNumeric(16),
              });
            return faker;
        }

        public static TenantListDto FindValidTenantListDtoForUsageAggregation(
            string databaseName = "TestTenantDb") =>
            new()
            {
                Name = "TestTenant",
                DatabaseName = databaseName,
            };

        public static List<TenantListDto> FindValidTenantListDtosForUsageAggregation(int count)
        {
            var list = new List<TenantListDto>();
            for (var i = 0; i < count; i++)
            {
                list.Add(FindValidTenantListDtoForUsageAggregation($"TestTenantDb{i + 1}"));
            }
            return list;
        }

        public static List<TenantListDto> FindValidTenantListDtos(int count)
        {
            var list = new List<TenantListDto>();
            for (int i = 0; i < count; i++)
            {
                list.Add(FindValidTenantListDto());
            }
            return list;
        }

        public static TenantAccessDto FindValidTenantAccessDto(
            string name = "Tenant1",
            bool isDatabaseCreated = true) =>
            new(name, isDatabaseCreated);

        public static List<TenantAccessDto> FindValidTenantAccessList() =>
        [
            FindValidTenantAccessDto("Tenant1"),
            FindValidTenantAccessDto("Tenant2"),
        ];

        public static ResponseCheckAccessDto FindValidResponseCheckAccessDto(
            bool hasAccess = true,
            ICollection<TenantAccessDto>? tenants = null) =>
            new()
            {
                HasAccess = hasAccess,
                Tenants = tenants ?? FindValidTenantAccessList(),
            };
    }

    [CollectionDefinition(nameof(TenantCollection))]
    public class TenantCollection : ICollectionFixture<TenantFixture>
    {
    }
}
