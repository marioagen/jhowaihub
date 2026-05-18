using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class UsageFixture
    {
        private readonly Faker _faker = new("pt_BR");

        public UsageDaily CreateValidUsageDaily()
        {
            return new UsageDaily(
                _faker.Random.Int(1, 1000),
                _faker.Date.Recent(),
                Guid.NewGuid(),
                _faker.Random.Int(1, 10),
                _faker.Random.Int(1, 100),
                false,
                _faker.Random.Int(0, 5)
            );
        }

        public UsageDailyDto CreateValidUsageDailyDto()
        {
            return new UsageDailyDto(
                _faker.Random.Int(1, 10),
                _faker.Random.Int(1, 100),
                Guid.NewGuid(),
                _faker.Random.Int(0, 5),
                false,
                null
            );
        }

        public UsageType CreateValidUsageType()
        {
            return new UsageType(
                _faker.Random.Int(1, 100),
                _faker.Date.Recent(),
                _faker.Commerce.ProductName()
            );
        }

        public ModelEmbedding CreateValidModelEmbedding()
        {
            return new ModelEmbedding(
                _faker.Random.Int(1, 100),
                _faker.Date.Recent(),
                _faker.Random.Word()
            );
        }

        public static List<UsageDaily> FindValidUsageDailies(int count)
        {
            var faker = new Faker<UsageDaily>("pt_BR")
                .CustomInstantiator(f => new UsageDaily(
                    f.Random.Int(1, 1000),
                    f.Date.Recent(),
                    Guid.NewGuid(),
                    f.Random.Int(1, 10),
                    f.Random.Int(1, 100),
                    false,
                    f.Random.Int(0, 5)
                ));
            return faker.Generate(count);
        }

        public static List<UsageDaily> FindCustomUsageDailies()
        {
            var sharedUserId = Guid.NewGuid();
            var sharedDay = new DateTime(2026, 5, 15);
            const int sharedUsageTypeId = 1;
            const int sharedModelEmbeddingId = 7;
            const int workflowId = 42;

            return new List<UsageDaily>
            {
                new UsageDaily(1, sharedDay, sharedUserId, sharedUsageTypeId, 100, false, sharedModelEmbeddingId, workflowId: null),
                new UsageDaily(2, sharedDay, sharedUserId, sharedUsageTypeId, 50,  false, sharedModelEmbeddingId, workflowId: workflowId)
            };

        }
    }

    [CollectionDefinition(nameof(UsageCollection))]
    public class UsageCollection : ICollectionFixture<UsageFixture>
    {
    }
}
