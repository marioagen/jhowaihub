using Bogus;
using Microsoft.VisualBasic;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class WorkflowFixture
    {
        public static WorkflowCreateDto FindValidWorkflowCreateDto()
        {
            var faker = new Faker<WorkflowCreateDto>("pt_BR")
                .CustomInstantiator(f => new WorkflowCreateDto
                {
                    Teams = new List<int>(),
                    Name = f.Lorem.Sentence(3),
                    Steps = new List<StepCreateDto>() { FindValidStepCreateDto() }
                });
            return faker;
        }

        public static WorkflowCreateDto FindValidWorkflowCreateDtoNoSteps()
        {
            var faker = new Faker<WorkflowCreateDto>("pt_BR")
                .CustomInstantiator(f => new WorkflowCreateDto
                {
                    Teams = new List<int>(),
                    Name = f.Lorem.Sentence(3)
                });
            return faker;
        }

        public static WorkflowCreateDto FindValidWorkflowCreateDtoStepWithNoName()
        {
            var faker = new Faker<WorkflowCreateDto>("pt_BR")
                .CustomInstantiator(f => new WorkflowCreateDto
                {
                    Name = f.Lorem.Sentence(3),
                    Steps = new List<StepCreateDto>()
                    {
                        new StepCreateDto
                        {
                            Name = string.Empty,
                            Order = f.Random.Int(1, 10),
                            ProfileId = f.Random.Int(1, 100),
                            StatusId = f.Random.Int(1, 5)
                        }
                    },
                    Teams = new List<int> { 2 }
                });
            return faker;
        }

        public static WorkflowUpdateDto FindValidWorkflowUpdateDto()
        {
            var faker = new Faker<WorkflowUpdateDto>("pt_BR")
                .CustomInstantiator(f => new WorkflowUpdateDto
                {
                    Id = f.Random.Int(1, 1000),
                    Name = f.Lorem.Sentence(3),
                    Steps = new List<StepUpdateDto>() { FindValidStepUpdateDto() },
                    Teams = new List<int> { f.Random.Int(1, 100) },
                });
            return faker;
        }

        public static ProfileDto FindValidProfileDto()
        {
            var faker = new Faker<ProfileDto>("pt_BR")
                .CustomInstantiator(f => new ProfileDto
                {
                    Id = 1,
                    Name = f.Lorem.Sentence(2)
                });
            return faker;
        }

        public static StatusDto FindValidStatusDto()
        {
            var f = new Faker("pt_BR");
            return new StatusDto
            {
                Id = f.Random.Int(1, 5),
                Name = f.Lorem.Sentence(1)
            };
        }

        public static Status FindValidStatus()
        {
            var f = new Faker("pt_BR");
            return new Status
            (
                f.Lorem.Sentence(2),
                f.Lorem.Sentence(1),
                f.Random.Int(1, 5),
                f.Date.Past()
            );
        }

        public static StepDto FindValidStepDto()
        {
            var faker = new Faker<StepDto>("pt_BR")
                .CustomInstantiator(f => new StepDto
                {
                    Id = f.Random.Int(1, 1000),
                    Name = f.Lorem.Sentence(2),
                    Order = 1,
                    Profile = FindValidProfileDto(),
                    Status = FindValidStatusDto()
                });
            return faker;
        }

        public static WorkflowDto FindValidWorkflowDto()
        {
            var faker = new Faker<WorkflowDto>("pt_BR")
                .CustomInstantiator(f => new WorkflowDto
                {
                    Id = f.Random.Int(1, 1000),
                    Name = f.Lorem.Sentence(3),
                    Teams = new List<TeamDto>() { FindValidTeamDto() },
                    Steps = new List<StepDto>() { FindValidStepDto() }
                });
            return faker;
        }

        public static StepToolUpdateDto FindValidStepToolUpdateDto()
        {
            return  new StepToolUpdateDto
            {
                Id = 0,
                ToolId = 1,
                Order = 1,
                PositionX = 2,
                PositionY = 2,
                Parameters = new List<StepToolParameterUpdateDto>
                {
                    new StepToolParameterUpdateDto
                    {
                        Value = "value1"
                    }
                },
            };
        }

        public static StepUpdateDto FindValidStepUpdateDto()
        {
            var f = new Faker("pt_BR");
            return new StepUpdateDto
            {
                Id = f.Random.Int(0, 2),
                Name = f.Lorem.Sentence(2),
                Order = f.Random.Int(1, 10),
                ProfileId = f.Random.Int(1, 100),
                StatusId = f.Random.Int(1, 5),
                StepTools = new List<StepToolUpdateDto>() { FindValidStepToolUpdateDto() }
            };
        }

        public static StepCreateDto FindValidStepCreateDto()
        {
            var stepToolUpdateDto = new StepToolUpdateDto
            {
                Id = 0,
                ToolId = 1,
                Order = 1,
                PositionX = 2,
                PositionY = 2
            };
            var faker = new Faker("pt_BR");
            return new StepCreateDto
            {
                Name = faker.Lorem.Sentence(2),
                Order = faker.Random.Int(1, 10),
                ProfileId = faker.Random.Int(1, 100),
                StatusId = faker.Random.Int(1, 5),
                StepTools = new List<StepToolUpdateDto>() { stepToolUpdateDto }
            };
        }

        public static TeamDto FindValidTeamDto()
        {
            var faker = new Faker<TeamDto>("pt_BR")
            .CustomInstantiator(f => new TeamDto
            {
                Id = f.Random.Int(1, 1000),
                Name = f.Company.CompanyName(),
                Created = f.Date.Past(),
                Users = new List<UserDto>
                {
                        new UserDto
                        {
                            Id = f.Random.Guid(),
                            Name = f.Person.FullName,
                            Email = f.Internet.Email(),
                            IsActive = true
                        }
                }
            });
            return faker;
        }

        public static Workflow FindValidWorkflow()
        {
            var f = new Faker("pt_BR");
            return new Workflow(
                f.IndexFaker,
                f.Date.Past(),
                new List<Team>() { DocumentFixture.FindValidTeam() },
                f.Person.FirstName)
            {
                Steps = new List<Step> { FindValidStep() }
            };
        }

        public static List<Workflow> FindValidWorkflows()
        {
            var f = new Faker("pt_BR");
            List<Workflow> workflow = new Faker<Workflow>("pt_BR")
           .CustomInstantiator(f => new Workflow
           (
               f.IndexFaker,
               f.Date.Past(),
               new List<Team>() { DocumentFixture.FindValidTeam() },
               f.Person.FirstName

           )
           {
               Steps = new List<Step> { FindValidStep() }
           }
           ).Generate(2);

            return workflow;
        }

        public static Step FindValidStep(int? workflowId = null)
        {
            var f = new Faker("pt_BR");
            return new Step(
                f.IndexFaker,
                f.Date.Past(),
                workflowId ?? f.Random.Int(1, 5),
                f.Person.FirstName,
                1,
                1,
                f.Random.Int(1, 5))
            {
                Cards = new List<Card> { FindValidCard() },
                StepTools = new List<StepTool> { FindValidStepTool() }
            };
        }

        public static Card FindValidCard()
        {
            var _faker = new Faker("pt_BR");
            return new Card(
                10,
                DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Name.FullName(),
                _faker.Random.Int(1, 1000),
                true,
                Guid.NewGuid()
             );
        }

        public static StepTool FindValidStepTool()
        {
            var _faker = new Faker("pt_BR");
            return new StepTool(
                _faker.Random.Int(1, 1000),
                 DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Decimal(1, 1000),
                _faker.Random.Decimal(1, 1000)
             );
        }

        public static StepTool FindValidStepToolWithDependencies()
        {
            var _faker = new Faker("pt_BR");
            var step =  new StepTool(
                _faker.Random.Int(1, 1000),
                 DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Decimal(1, 1000),
                _faker.Random.Decimal(1, 1000)
             );
            step.Dependencies.Add(new StepToolDependency(
                _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                step.Id,
                _faker.Random.Int(1, 1000)
             ));
            return step;
        }
    }

    [CollectionDefinition(nameof(WorkflowCollection))]
    public class WorkflowCollection : ICollectionFixture<WorkflowFixture>
    {
    }
}
