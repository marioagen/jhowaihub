using Xunit;
using Bogus;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class ToolFixture
    {   
        public static ToolDto FindValidTool()
        {
            var _faker = new Faker("pt_BR");
            return new ToolDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                InputData = _faker.Name.FullName(),
                OutputData = _faker.Name.FullName(),
                ToolType = _faker.Name.FullName(),
            };
        }

        public static List<ToolDto> FindValidTools()
        {
            return new List<ToolDto>() {
                FindValidTool(),
                FindValidTool(),
            };
        }

        public static ToolCreateDto FindValidToolCreateDto()
        {
            var _faker = new Faker("pt_BR");
            return new ToolCreateDto
            {
                Name = _faker.Name.FullName(),
                InputDataId = _faker.Random.Int(1, 1000),
                OutputDataId = _faker.Random.Int(1, 1000),
                ToolTypeId = _faker.Random.Int(1, 1000),
                ConnectorUrl = _faker.Internet.Url(),
                ConnectorApiKey = Guid.NewGuid().ToString(),
            };
        }

        public static ToolUpdateDto FindValidToolUpdateDto()
        {
            var _faker = new Faker("pt_BR");
            return new ToolUpdateDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                InputDataId = _faker.Random.Int(1, 1000),
                OutputDataId = _faker.Random.Int(1, 1000),
                ToolTypeId = _faker.Random.Int(1, 1000),
                ConnectorUrl = _faker.Internet.Url(),
                ConnectorApiKey = Guid.NewGuid().ToString(),
            };
        }

        public static Tool FindValidToolModel()
        {
            var _faker = new Faker("pt_BR");
            return new Tool
            (
                _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                _faker.Name.FullName(),
                true,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                true,
                _faker.Name.FullName(),
                _faker.Random.Guid().ToString()
            );
        }
        public static Tool FindValidToolModelWithEmptyConnector()
        {
            var _faker = new Faker("pt_BR");
            return new Tool
            (
                _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                _faker.Name.FullName(),
                true,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                true,
                string.Empty,
                string.Empty
            );
        }

        public static ToolPagedDataDto FindValidToolPagedDataDto()
        {
            var _faker = new Faker("pt_BR");
            return new ToolPagedDataDto
            {
                Page = _faker.Random.Int(1, 100),
                PageSize = _faker.Random.Int(1, 100),
                IsAscending = _faker.Random.Bool()
            };
        }

        public static ToolConnectorDto FindValidToolConnectorDto()
        {
            var _faker = new Faker("pt_BR");
            return new ToolConnectorDto
            {
                ConnectorUrl = _faker.Internet.Url(),
                ConnectorApiKey = Guid.NewGuid().ToString(),
            };
        }

        public static ApiTemplateStepToolCreateDto FindValidApiTemplateStepToolCreateDto()
        {
            var _faker = new Faker("pt_BR");
            return new ApiTemplateStepToolCreateDto
            {
                StepToolId = _faker.Random.Int(1, 1000),
                Method = "POST",
                Url = _faker.Internet.Url(),
                QueryTemplate = @"[{""key"":""userId"",""value"":""1234""}]",
                HeaderTemplate = @"[{""key"":""Authorization"",""value"":""Bearer abcdefg""}]",
                BodyTemplate = @"{""text"":""abcdefg""}",
            };
        }
    }

        public static WorkflowUsageDto FindValidWorkflowUsage()
        {
            var f = new Faker("pt_BR");
            return new WorkflowUsageDto
            {
                WorkflowId = f.Random.Int(1, 1000),
                WorkflowName = f.Lorem.Sentence(2),
            };
        }

        public static List<WorkflowUsageDto> FindValidWorkflowUsageList()
        {
            return new List<WorkflowUsageDto> { FindValidWorkflowUsage(), FindValidWorkflowUsage() };
        }

        public static Workflow FindValidWorkflowWithTeamUser(int toolId)
        {
            var f = new Faker("pt_BR");
            var userId = Guid.NewGuid();
            var userEmail = f.Internet.Email();
            var user = new User(userId, f.Person.FirstName, userEmail, true, DateTime.UtcNow);
            var team = new Team("Test Team", 1, DateTime.UtcNow);
            team.Users.Add(user);

            var workflow = new Workflow(f.Random.Int(1, 1000), DateTime.UtcNow, new List<Team> { team }, f.Lorem.Sentence(2));

            var step = new Step(f.Random.Int(1, 1000), DateTime.UtcNow, workflow.Id, "Step 1", 1, 1, 1);
            var stepTool = new StepTool(f.Random.Int(1, 1000), DateTime.UtcNow, step.Id, toolId, 1, 0m, 0m);
            step.StepTools = new List<StepTool> { stepTool };

            workflow.Steps = new List<Step> { step };
            return workflow;
        }
    }

    [CollectionDefinition(nameof(ToolCollection))]
    public class ToolCollection : ICollectionFixture<ToolFixture>
    {
    }
}
