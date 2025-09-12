using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(ToolDataCollection))]
    public class ToolDataServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly IToolDataServices _toolDataServices;
        private readonly Mock<IToolDataRepository> _toolDataRepositoryMock;

        public ToolDataServicesTests()
        {
            _mocker = new AutoMocker();
            _toolDataServices = _mocker.CreateInstance<ToolDataServices>();
            _toolDataRepositoryMock = _mocker.GetMock<IToolDataRepository>();
        }

        [Fact(DisplayName = "FindAllAsync should return all tool datas")]
        [Trait("FindAllAsync", "Success")]
        public async Task FindAllAsync_ShouldReturnAllToolDatas()
        {
            // Arrange
            var toolDatas = ToolDataFixture.FindValidToolDatas();
            _toolDataRepositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(toolDatas);

            // Act
            var result = await _toolDataServices.FindAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(toolDatas, result);
            _toolDataRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
        }
    }
}
