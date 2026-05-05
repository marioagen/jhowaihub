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

        public static StepToolOutput CreateStepToolOutput(string toolType, string value)
        {
            var output = AutomationFixture.FindValidStepToolOutput(value);
            output.StepTool = new StepTool(1, DateTime.UtcNow, 1, 1, 1, 1, 1)
            {
                Tool = new Tool(1, DateTime.UtcNow, "Tool", true, 1, 1, 1, false, null, null)
                {
                    ToolType = new ToolType(1, DateTime.UtcNow, toolType, string.Empty, true)
                }
            };
            return output;
        }
    }

    [CollectionDefinition(nameof(ToolHandlerCollection))]
    public class ToolHandlerCollection : ICollectionFixture<ToolHandlerFixture>
    {
    }
}
