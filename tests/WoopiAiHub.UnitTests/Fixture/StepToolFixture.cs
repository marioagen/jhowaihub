using Bogus;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class StepToolFixture
    {
        public static StepTool FindValidStepTool()
        {
            var _faker = new Faker("pt_BR");
            return new StepTool(
                _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 10),
                _faker.Random.Decimal(0, 100),
                _faker.Random.Decimal(0, 100)
            );
        }

        public static StepTool FindValidStepToolWithParameters()
        {
            var stepTool = FindValidStepTool();
            var parameter = FindValidStepToolParameter(stepTool.Id);
            stepTool.AddParameter(parameter);
            return stepTool;
        }

        public static StepToolParameter FindValidStepToolParameter(int stepToolId)
        {
            var _faker = new Faker("pt_BR");
            return new StepToolParameter(
                _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                stepToolId,
                false,
                null,
                @"{""method"":""POST"",""url"":""https://api.exemplo.com/v1/process""}"
            );
        }
    }
}
