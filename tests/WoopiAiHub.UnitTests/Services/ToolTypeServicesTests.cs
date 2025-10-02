using Google.Api;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(ToolTypeCollection))]
    public class ToolTypeServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly ToolTypeServices _toolTypeServices;
        private readonly Mock<IToolTypeRepository> _toolTypeRepositoryMock;

        public ToolTypeServicesTests()
        {
            _mocker = new AutoMocker();
            _toolTypeServices = _mocker.CreateInstance<ToolTypeServices>();
            _toolTypeRepositoryMock = _mocker.GetMock<IToolTypeRepository>();   
        }

        [Fact(DisplayName = "FindAllAsync should return all tool types")]
        [Trait("FindAllAsync", "Success")]
        public async Task FindAllAsync_ShouldReturnAllToolTypes()
        {
            // Arrange
            var toolTypes = ToolTypeFixture.FindValidToolTypes();
            _toolTypeRepositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(toolTypes);

            // Act
            var result = await _toolTypeServices.FindAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(toolTypes, result);
            _toolTypeRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
        }
    }
}
