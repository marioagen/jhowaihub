using AutoMapper;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http.HttpResults;
using StackExchange.Redis;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class AutomationFixture
    {

        public static StepTool FindValidStepTool(int? fixedId = null)
        {
            var _faker = new Faker("pt_BR");
            var steptool = new StepTool(
                fixedId.HasValue ? fixedId.Value : _faker.Random.Int(1, 1000),
                DateTime.UtcNow,
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 1000),
                _faker.Random.Decimal(1, 1000),
                _faker.Random.Decimal(1, 1000)
            );
            steptool.DependsOnStepTool = new StepTool(1,DateTime.Now, 1, 1, 1, 1, 1);
            steptool.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1);
            return steptool;
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

        public static AutomationServicesDto FindValidautomationServicesDto()
        {
            var _faker = new Faker("pt_BR");
            return new AutomationServicesDto(
                1,
                1,
                "tenant",
                "email",
                "ref",
                1
            );
        }

        public static ToolDto FindValidToolDto()
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
        public static StepDto FindValidStepDto()
        {
            var _faker = new Faker("pt_BR");
            return new StepDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                WorkflowId = _faker.Random.Int(1, 1000),
                Order = 1,
                Profile = FindValidProfileDto(),
                Status = FindValidStatusDto(),
                Cards = new List<CardDto>() { FindValidCardDto() },
                StepTools = new List<StepToolDto>
                {
                    new StepToolDto{

                        Id = 1,
                        Name = _faker.Name.LastName(),
                        StepId = 1,
                        ToolId = 1,
                        Order = 1,
                        PositionX = 1,
                        PositionY = 1,
                        DependsOnStepToolId = null,
                        DependsOnStepTool = null,
                        Step = new StepDto
                        {
                            Id = 1,
                            Name = _faker.Lorem.Sentence(2),
                            Order = 1,
                            Profile = FindValidProfileDto(),
                            Status = FindValidStatusDto(),
                            Cards = new List<CardDto>() { FindValidCardDto() },
                            StepTools = new List<StepToolDto>()
                        },
                        Tool = null,
                        Parameters = new List<StepToolParameterDto>()
                    }
                }

            };
        }

        public static ProfileDto FindValidProfileDto()
        {
            var _faker = new Faker("pt_BR");
            return new ProfileDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                Created = DateTime.UtcNow,
            };
        }

        public static StatusDto FindValidStatusDto()
        {
            var _faker = new Faker("pt_BR");
            return new StatusDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                Color = _faker.Internet.Color(),
            };
        }

        public static CardDto FindValidCardDto()
        {
            var _faker = new Faker("pt_BR");
            return new CardDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                Description = _faker.Lorem.Sentence(5),
                StepId = _faker.Random.Int(1, 1000),
                Order = _faker.Random.Int(1, 1000),
                Status = FindValidStatusDto(),
                Created = DateTime.UtcNow,
            };
        }

        public static StepToolDto FindValidStepToolDto()
        {
            var _faker = new Faker("pt_BR");
            return new StepToolDto
            {
                Id = 1,
                Name = _faker.Name.LastName(),
                StepId = 1,
                ToolId = 1,
                Order = 1,
                PositionX = 1,
                PositionY = 1,
                DependsOnStepToolId = 1,
                DependsOnStepTool = new StepToolDto
                {
                    Id = 1,
                    Name = _faker.Name.LastName(),
                    StepId = 1,
                    ToolId = 1,
                    Order = 1,
                    PositionX = 1,
                    PositionY = 1,
                    DependsOnStepToolId = null,
                    Step = FindValidStepDto(),
                    Tool = null,
                    Parameters = new List<StepToolParameterDto>()
                },
                Step = FindValidStepDto(),
                Tool = FindValidToolDto(),
            };
        }

    }

    [CollectionDefinition(nameof(AutomationCollection))]
    public class AutomationCollection : ICollectionFixture<AutomationFixture>
    {
    }
}
