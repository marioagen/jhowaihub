using Bogus;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class AutomationFixture
    {

        public static StepTool FindValidStepTool(int? fixedId = null)
        {
            var _faker = new Faker("pt_BR");
            return new StepTool(
                fixedId.HasValue ? fixedId.Value : _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Decimal(1, 1000),
                _faker.Random.Decimal(1, 1000)
            );
        }

        public static ExecutionMessageDto FindValidExecutionMessageDto()
        {
            var _faker = new Faker("pt_BR");
            return new ExecutionMessageDto
            {
                Message = _faker.Name.FirstName(),
                Queue = _faker.Name.LastName(),
            };
        }

        public static StepToolExecution FindValidStepToolExecution()
        {
            var _faker = new Faker("pt_BR");
            return new StepToolExecution(
                _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                Domain.Enum.StatusExecution.Pending,
                _faker.Random.Int(1, 1000)
            );
        }

    }

    [CollectionDefinition(nameof(AutomationCollection))]
    public class AutomationCollection : ICollectionFixture<AutomationFixture>
    {
    }
}
