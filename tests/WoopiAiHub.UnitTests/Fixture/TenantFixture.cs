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

        public const string ValidUserEmail = "user@test.com";

        public static TenantAccessDto FindValidTenantAccessDto(
            string name = "Tenant1",
            bool isDatabaseCreated = true) =>
            new(name, isDatabaseCreated);

        public static List<TenantAccessDto> FindValidTenantAccessList() =>
        [
            FindValidTenantAccessDto(),
            FindValidTenantAccessDto("Tenant2", true)
        ];

        public static ResponseCheckAccessDto FindValidResponseCheckAccessDto(
            bool hasAccess = true,
            IReadOnlyList<TenantAccessDto>? tenants = null) =>
            new()
            {
                HasAccess = hasAccess,
                Tenants = tenants?.ToList() ?? [FindValidTenantAccessDto()]
            };
    }

    [CollectionDefinition(nameof(TenantCollection))]
    public class TenantCollection : ICollectionFixture<TenantFixture>
    {
    }
}
