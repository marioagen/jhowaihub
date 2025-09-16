using Bogus;
using WoopiAiHub.Domain.DTOs.Response;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class ToolDataFixture
    {
        public static ToolDataDto FindValidToolDataDto()
        {
            var _faker = new Faker("pt_BR");
            return new ToolDataDto
            {
                Name = _faker.Name.FullName(),
                Id = _faker.Random.Int(1, 1000)
            };
        }

        public static IEnumerable<ToolDataDto> FindValidToolDatas()
        {
            return new List<ToolDataDto>() {
                FindValidToolDataDto(),
                FindValidToolDataDto(),
            };
        }
    }

    [CollectionDefinition(nameof(ToolDataCollection))]
    public class ToolDataCollection : ICollectionFixture<ToolDataFixture>
    {
    }
}
