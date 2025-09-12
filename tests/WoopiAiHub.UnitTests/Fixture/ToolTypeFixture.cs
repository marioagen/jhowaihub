using Bogus;
using WoopiAiHub.Domain.DTOs.Response;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class ToolTypeFixture
    {
        public static ToolTypeDto FindValidToolTypeDto()
        {
            var _faker = new Faker("pt_BR");
            return new ToolTypeDto
            {
                Name = _faker.Name.FullName(),
                Id = _faker.Random.Int(1, 1000)
            };
        }

        public static IEnumerable<ToolTypeDto> FindValidToolTypes()
        {
            return new List<ToolTypeDto>() {
                FindValidToolTypeDto(),
                FindValidToolTypeDto(),
            };
        }
    }

    [CollectionDefinition(nameof(ToolTypeCollection))]
    public class ToolTypeCollection : ICollectionFixture<ToolTypeFixture>
    {
    }
}
