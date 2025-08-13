using Bogus;
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
                    TeamId = f.Random.Int(1, 1000),
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
                    TeamId = f.Random.Int(1, 1000),
                    Name = f.Lorem.Sentence(3)
                });
            return faker;
        }

        public static WorkflowCreateDto FindValidWorkflowCreateDtoStepWithNoName()
        {
            var faker = new Faker<WorkflowCreateDto>("pt_BR")
                .CustomInstantiator(f => new WorkflowCreateDto
                {
                    TeamId = f.Random.Int(1, 1000),
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
                    }
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
                    Steps = new List<StepUpdateDto>() { FindValidStepUpdateDto() }
                });
            return faker;
        }

        public static ProfileDto FindValidProfileDto()
        {
            var faker = new Faker<ProfileDto>("pt_BR")
                .CustomInstantiator(f => new ProfileDto
                {
                    Id = f.Random.Int(1, 100),
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
                    Order = f.Random.Int(1, 10),
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
                    TeamId = f.Random.Int(1, 1000),
                    Steps = new List<StepDto>() { FindValidStepDto() }
                });
            return faker;
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
                StatusId = f.Random.Int(1, 5)
            };
        }

        public static StepCreateDto FindValidStepCreateDto()
        {
            var faker = new Faker("pt_BR");
            return new StepCreateDto
            {
                Name = faker.Lorem.Sentence(2),
                Order = faker.Random.Int(1, 10),
                ProfileId = faker.Random.Int(1, 100),
                StatusId = faker.Random.Int(1, 5)
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
                f.Random.Int(1, 5),
                f.Person.FirstName)
            {
                Steps = new List<Step> { FindValidStep() }
            };
        }

        public static Step FindValidStep()
        {
            var f = new Faker("pt_BR");
            return new Step(
                f.IndexFaker,
                f.Date.Past(),
                f.Random.Int(1,5),
                f.Person.FirstName,
                f.Random.Int(1, 5),
                f.Random.Int(1, 5),
                f.Random.Int(1, 5));
        }
    }

    [CollectionDefinition(nameof(WorkflowCollection))]
    public class WorkflowCollection : ICollectionFixture<WorkflowFixture>
    {
    }
}
