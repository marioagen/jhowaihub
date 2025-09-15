using Moq;
using Moq.AutoMock;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(ToolCollection))]
    public class ToolServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly ToolServices _toolServices;
        private readonly Mock<IToolRepository> _toolRepositoryMock;

        public ToolServicesTests()
        {
            _mocker = new AutoMocker();
            _toolServices = _mocker.CreateInstance<ToolServices>();
            _toolRepositoryMock = _mocker.GetMock<IToolRepository>();
        }

        [Fact(DisplayName = "FindAllAsync should return all tools")]
        [Trait("FindAllAsync", "Success")]
        public async Task FindAllAsync_ShouldReturnAllTools()
        {
            // Arrange
            var tools = ToolFixture.FindValidTools();
            _toolRepositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(tools);

            // Act
            var result = await _toolServices.FindAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tools, result);
            _toolRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
        }

        [Fact(DisplayName = "DeleteAsync should delete all tools in list")]
        [Trait("DeleteAsync", "Success")]
        public async Task DeleteAsync_ShouldDeleteAllToolsInList()
        {
            // Arrange
            var ids =  new List<int>() { 1, 2};
            _toolRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<List<int>>())).ReturnsAsync(true);

            // Act
            var result = await _toolServices.DeleteAsync(ids);

            // Assert
            Assert.True(result);        
            _toolRepositoryMock.Verify(x => x.DeleteAsync(ids), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAsync should return a Tool")]
        [Trait("FindByIdAsync", "Success")]
        public async Task FindByIdAsync_ShouldReturnATool()
        {
            // Arrange
            var id = 1;
            var tool = ToolFixture.FindValidTool();
            _toolRepositoryMock.Setup(x => x.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(tool);

            // Act
            var result = await _toolServices.FindByIdAsync(id);

            // Assert
            Assert.Equal(tool, result);
            _toolRepositoryMock.Verify(x => x.FindByIdAsync(id), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAsync should return null when not found")]
        [Trait("FindByIdAsync", "Success")]
        public async Task FindByIdAsync_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var id = 1;
            _toolRepositoryMock.Setup(x => x.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((ToolDto?)null);

            // Act
            var result = await _toolServices.FindByIdAsync(id);

            // Assert
            Assert.Null(result);
            _toolRepositoryMock.Verify(x => x.FindByIdAsync(id), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should throw AppException when Tool is duplicated")]
        [Trait("CreateAsync", "Fail")]
        public async Task CreateAsync_ShouldThrowAppException_WhenToolIsDuplicated()
        {
            // Arrange
            var toolCreateDto = ToolFixture.FindValidToolCreateDto();

            _toolRepositoryMock.Setup(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.CreateAsync(toolCreateDto));
            Assert.Equal(ErrorCode.Duplicated, exception.ErrorCode);
            Assert.Equal("Duplicated Tool", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should return true when Tool is created successfully")]
        [Trait("CreateAsync", "Success")]
        public async Task CreateAsync_ShouldReturnTrue_WhenToolIsCreatedSuccessfully()
        {
            // Arrange
            var toolCreateDto = ToolFixture.FindValidToolCreateDto();

            _toolRepositoryMock.Setup(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()))
                .ReturnsAsync(true);

            // Act
            var result = await _toolServices.CreateAsync(toolCreateDto);

            // Assert
            Assert.True(result);
            _toolRepositoryMock.Verify(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()), Times.Once);
        }

        [Fact(DisplayName = "UpdateAsync should throw AppException when Tool not found")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_ToolNotFound_ThrowsAppException()
        {
            // Arrange
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(toolUpdateDto.Id))
                               .ReturnsAsync((Tool?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.UpdateAsync(toolUpdateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Tool not found", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(toolUpdateDto.Id), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Tool>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should throw AppException when  when Tool is duplicated")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_UpdateFails_ThrowsAppException()
        {
            // Arrange
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            var tool = ToolFixture.FindValidToolModel();
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(toolUpdateDto.Id))
                               .ReturnsAsync(tool);
            _toolRepositoryMock.Setup(repo => repo.UpdateAsync(tool))
                               .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.UpdateAsync(toolUpdateDto));
            Assert.Equal(ErrorCode.Duplicated, exception.ErrorCode);
            Assert.Equal("Duplicated Tool", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(toolUpdateDto.Id), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(tool), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should return true when Tool is updated successfully")]
        [Trait("CreateAsync", "Success")]
        public async Task UpdateAsync_SuccessfulUpdate_ReturnsTrue()
        {
            // Arrange
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            var tool = ToolFixture.FindValidToolModel();
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(toolUpdateDto.Id))
                               .ReturnsAsync(tool);
            _toolRepositoryMock.Setup(repo => repo.UpdateAsync(tool))
                               .ReturnsAsync(true);

            // Act
            var result = await _toolServices.UpdateAsync(toolUpdateDto);

            // Assert
            Assert.True(result);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(toolUpdateDto.Id), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(tool), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged should throws ArgumentException when page Less than or equal to zero")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_PageLessThanOrEqualToZero_ThrowsArgumentException()
        {
            // Arrange
            var pagedDataDto = new ToolPagedDataDto { Page = 0 };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _toolServices.FindAllPaged(pagedDataDto));
            Assert.Equal("The number of pages must be greater than 0", exception.Message);
        }

        [Fact(DisplayName = "FindAllPaged should return paged list")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_ReturnsPagedResponse()
        {
            // Arrange
            var tools = ToolFixture.FindValidTools().AsQueryable();
            var pagedDataDto = ToolFixture.FindValidToolPagedDataDto();
            pagedDataDto.PageSize = 0;
            pagedDataDto.Search = tools.First().Id.ToString();

            _toolRepositoryMock.Setup(repo => repo.FindAllPaged()).Returns(tools);
           
            // Act
            var result = _toolServices.FindAllPaged(pagedDataDto);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Items!);
            Assert.Equal(tools.First().Name, result!.Items!.First().Name);
            _mocker.GetMock<IToolRepository>().Verify(repo => repo.FindAllPaged(), Times.Once);
        }

        [Theory(DisplayName = "FindAllPaged should return list in order")]
        [Trait("FindAllPaged", "Success")]
        [InlineData(true)]
        [InlineData(false)]
        public void FindAllPaged_Order_ReturnsOrderedPagedResponse(bool order)
        {
            // Arrange
            var tools = ToolFixture.FindValidTools().AsQueryable();
            var pagedDataDto = ToolFixture.FindValidToolPagedDataDto();
            pagedDataDto.IsAscending = order;

            _toolRepositoryMock.Setup(repo => repo.FindAllPaged()).Returns(tools);

            // Act
            var result = _toolServices.FindAllPaged(pagedDataDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tools.Count(), result!.Items!.Count());
            _mocker.GetMock<IToolRepository>().Verify(repo => repo.FindAllPaged(), Times.Once);
        }
    }
}
