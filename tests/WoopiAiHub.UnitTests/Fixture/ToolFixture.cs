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
                _faker.Random.Int(1, 1000)
            );
        }

        public static PagedDataDto FindValidPagedDataDto()
        {
            var _faker = new Faker("pt_BR");
            return new PagedDataDto
            {
                Page = _faker.Random.Int(1, 100),
                PageSize = _faker.Random.Int(1, 100),
                IsAscending = _faker.Random.Bool()
            };
        }
    }

    [CollectionDefinition(nameof(ToolCollection))]
    public class ToolCollection : ICollectionFixture<ToolFixture>
    {
    }
}
