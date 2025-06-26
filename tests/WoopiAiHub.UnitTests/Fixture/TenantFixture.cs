using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
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
                  RefineTemplate = f.Random.String(),
                  SearchMode = f.Random.String(),
                  Template = f.Random.String(),
              });

            return faker;
        }
    }

    [CollectionDefinition(nameof(TenantCollection))]
    public class TenantCollection : ICollectionFixture<TenantFixture>
    {
    }
}
