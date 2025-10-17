using Bogus;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class ToolHandlerFixture
    {

        public static StepToolParameter FindValidStepToolParameter()
        {
            var _faker = new Faker("pt_BR");
            return new StepToolParameter
            (
                _faker.IndexFaker,
                _faker.Date.Past(),
                _faker.Random.Int(1,10),
                _faker.Random.Bool(2),
                _faker.Random.Guid(),
                _faker.Name.FullName()
            );
        }
    }

    [CollectionDefinition(nameof(ToolHandlerCollection))]
    public class ToolHandlerCollection : ICollectionFixture<ToolHandlerFixture>
    {
    }
}
