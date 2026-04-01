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
            return new StepToolUpdateDto
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

        public static StepToolUpdateDto FindValidStepToolUpdateDtoWithDependencies()
        {
            return new StepToolUpdateDto
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
                Dependencies = new List<StepToolOutputDependencyDto>
                {
                    new StepToolOutputDependencyDto { StepOrder = 1, StepToolOrder = 1 }
                }
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
                Order = 1,
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
            var step = new StepTool(
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

        public static WorkflowCloneRequestDto CreateWorkflowCloneRequestDto(int sourceWorkflowId = 1, string newName = "Cloned Workflow")
        {
            return new WorkflowCloneRequestDto
            {
                SourceWorkflowId = sourceWorkflowId,
                NewName = newName
            };
        }

        public static Workflow CreateWorkflowForClone(int workflowId = 1, int teamId = 1)
        {
            var f = new Faker("pt_BR");
            var team = new Team("Test Team", teamId, DateTime.UtcNow);

            var workflow = new Workflow(
                workflowId,
                DateTime.UtcNow,
                new List<Team> { team },
                "Source Workflow"
            );

            // Add Step 1 with 2 StepTools
            var step1 = new Step(
                1,
                DateTime.UtcNow,
                workflowId,
                "Step 1",
                1,
                1,
                1
            );

            var stepTool1 = new StepTool(
                1,
                DateTime.UtcNow,
                step1.Id,
                1,
                1,
                100,
                100
            );
            stepTool1.Parameters.Add(new StepToolParameter(
                1,
                DateTime.UtcNow,
                stepTool1.Id,
                false,
                null,
                "param1_value"
            ));

            var stepTool2 = new StepTool(
                2,
                DateTime.UtcNow,
                step1.Id,
                2,
                2,
                200,
                200
            );
            stepTool2.Parameters.Add(new StepToolParameter(
                2,
                DateTime.UtcNow,
                stepTool2.Id,
                true,
                null,
                "param2_value"
            ));
            stepTool2.UpdateDependencyStepTool(stepTool1);

            step1.AddStepTool(stepTool1);
            step1.AddStepTool(stepTool2);

            // Add Step 2 with 1 StepTool
            var step2 = new Step(
                2,
                DateTime.UtcNow,
                workflowId,
                "Step 2",
                2,
                2,
                1
            );

            var stepTool3 = new StepTool(
                3,
                DateTime.UtcNow,
                step2.Id,
                1,
                1,
                300,
                300
            );
            stepTool3.Parameters.Add(new StepToolParameter(
                3,
                DateTime.UtcNow,
                stepTool3.Id,
                false,
                null,
                "param3_value"
            ));
            stepTool3.UpdateDependencyStepTool(stepTool2);

            step2.AddStepTool(stepTool3);

            workflow.AddStep(step1);
            workflow.AddStep(step2);

            return workflow;
        }

        public static Workflow CreateWorkflowWithDependencies(int workflowId = 1)
        {
            var f = new Faker("pt_BR");
            var team = new Team("Test Team", 1, DateTime.UtcNow);

            var workflow = new Workflow(
                workflowId,
                DateTime.UtcNow,
                new List<Team> { team },
                "Workflow with Dependencies"
            );

            var step = new Step(
                1,
                DateTime.UtcNow,
                workflowId,
                "Step with Dependencies",
                1,
                1,
                1
            );

            var stepTool1 = new StepTool(1, DateTime.UtcNow, step.Id, 1, 1, 100, 100);
            var stepTool2 = new StepTool(2, DateTime.UtcNow, step.Id, 2, 2, 200, 200);
            var stepTool3 = new StepTool(3, DateTime.UtcNow, step.Id, 3, 3, 300, 300);

            // Create dependency chain: stepTool1 -> stepTool2 -> stepTool3
            stepTool2.UpdateDependencyStepTool(stepTool1);
            stepTool3.UpdateDependencyStepTool(stepTool2);

            // Add StepToolDependency records
            stepTool2.Dependencies.Add(new StepToolDependency(1, DateTime.UtcNow, stepTool2.Id, stepTool1.Id));
            stepTool3.Dependencies.Add(new StepToolDependency(2, DateTime.UtcNow, stepTool3.Id, stepTool2.Id));

            step.AddStepTool(stepTool1);
            step.AddStepTool(stepTool2);
            step.AddStepTool(stepTool3);

            workflow.AddStep(step);

            return workflow;
        }

        public static Tool CreateToolModel(int id, string name, string toolTypeName)
        {
            var tool = new Tool(
                id,
                DateTime.UtcNow,
                name,
                true,
                1,
                1,
                1,
                false,
                null,
                null
            );

            var toolType = new ToolType(1, DateTime.UtcNow, toolTypeName, string.Empty, true);
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);

            return tool;
        }

        public static ToolDto CreateToolDto(Tool tool)
        {
            return new ToolDto
            {
                Id = tool.Id,
                Name = tool.Name,
                ToolTypeId = tool.ToolType?.Id ?? 1,
                ToolType = tool.ToolType?.Name ?? "OCR",
                InputDataId = tool.InputDataId,
                InputData = "Input",
                OutputDataId = tool.OutputDataId,
                OutputData = "Output",
                IsEditableInput = tool.IsEditableInput,
                ConnectorUrl = tool.ConnectorUrl
            };
        }
    }

    [CollectionDefinition(nameof(WorkflowCollection))]
    public class WorkflowCollection : ICollectionFixture<WorkflowFixture>
    {
    }
}
