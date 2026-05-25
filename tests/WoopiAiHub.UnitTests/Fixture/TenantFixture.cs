using Bogus;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
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
                  DatabaseName = "database",
                  Email = f.Random.String(),
                  EmbeddingModelName = f.Random.String(),
                  KValue = f.Random.Int(),
                  MaxTokens = f.Random.Int(),
                  Model = f.Random.String(),
                  Name = f.Random.String(),
                  OcrModel = f.Random.String(),
                  RagProvider = f.PickRandom<RagProvider>(),
                  LlmProvider = f.PickRandom<LlmProvider>(),
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
                  Name = f.Random.String(),
                  DatabaseName = f.Random.String(),
              });
            return faker;
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
    }

    [CollectionDefinition(nameof(TenantCollection))]
    public class TenantCollection : ICollectionFixture<TenantFixture>
    {
    }
}
