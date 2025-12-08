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
                _faker.Random.Int(0, 5)
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
    }

    [CollectionDefinition(nameof(UsageCollection))]
    public class UsageCollection : ICollectionFixture<UsageFixture>
    {
    }
}
