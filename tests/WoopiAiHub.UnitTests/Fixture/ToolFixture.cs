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
                _faker.Random.Guid().ToString()
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
    }

    [CollectionDefinition(nameof(ToolCollection))]
    public class ToolCollection : ICollectionFixture<ToolFixture>
    {
    }
}
