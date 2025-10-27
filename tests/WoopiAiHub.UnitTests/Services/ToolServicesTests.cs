using Moq;
using Moq.AutoMock;
using Refit;
using System.Linq.Dynamic.Core;
using System.Net;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Utils;
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
        private readonly Mock<IToolTypeRepository> _toolTypeRepositoryMock;
        private readonly Mock<IApiClientFactory> _apiClientFactoryMock;
        private readonly Mock<In8NConnector> _in8nConnectorMock;
        private readonly Mock<IKeyVaultServices> _keyVaultServicesMock;

        public ToolServicesTests()
        {
            _mocker = new AutoMocker();

            _apiClientFactoryMock = _mocker.GetMock<IApiClientFactory>();
            _in8nConnectorMock = _mocker.GetMock<In8NConnector>();
            _toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            _toolTypeRepositoryMock = _mocker.GetMock<IToolTypeRepository>();
            _keyVaultServicesMock = _mocker.GetMock<IKeyVaultServices>();

            _toolServices = _mocker.CreateInstance<ToolServices>();
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
        public void DeleteAsync_ShouldDeleteAllToolsInList()
        {
            // Arrange
            var ids =  new List<int>() { 1, 2};
            _toolRepositoryMock.Setup(x => x.Delete(It.IsAny<List<int>>())).Returns(true);

            // Act
            var result = _toolServices.Delete(ids);

            // Assert
            Assert.True(result);        
            _toolRepositoryMock.Verify(x => x.Delete(ids), Times.Once);
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
            var toolType = ToolTypeFixture.FindValidToolType();

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>()))
                                   .ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()))
                .ReturnsAsync(false);
            _keyVaultServicesMock.Setup(k => k.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                                 .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.CreateAsync(toolCreateDto));
            Assert.Equal(ErrorCode.Duplicated, exception.ErrorCode);
            Assert.Equal("Duplicated Tool", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()), Times.Once);
            _toolTypeRepositoryMock.Verify(tt => tt.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should throw AppException when connector is required")]
        [Trait("CreateAsync", "Fail")]
        public async Task CreateAsync_ShouldThrowAppException_WhenCoonectorIsRequired()
        {
            // Arrange
            var toolCreateDto = ToolFixture.FindValidToolCreateDto();
            toolCreateDto.ConnectorUrl = string.Empty;
            var toolType = ToolTypeFixture.FindValidToolTypeWithName("n8n");          

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>())).ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.CreateAsync(toolCreateDto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal("Coonector Url and Connector Api Key are required", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()), Times.Never);
            _toolTypeRepositoryMock.Verify(tt => tt.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should return true when Tool is created successfully")]
        [Trait("CreateAsync", "Success")]
        public async Task CreateAsync_ShouldReturnTrue_WhenToolIsCreatedSuccessfully()
        {
            // Arrange
            var toolCreateDto = ToolFixture.FindValidToolCreateDto();
            var toolType = ToolTypeFixture.FindValidToolTypeWithName("n8n");

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>())).ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()))
                .ReturnsAsync(true);

            // Act
            var result = await _toolServices.CreateAsync(toolCreateDto);

            // Assert
            Assert.True(result);
            _toolRepositoryMock.Verify(repo => repo.CreateUniqueAsync(It.IsAny<Tool>()), Times.Once);
            _toolTypeRepositoryMock.Verify(tt => tt.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
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

        [Fact(DisplayName = "UpdateAsync should throw AppException when ToolType not found")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_ToolTypeNotFound_ThrowsAppException()
        {
            // Arrange
            var tool = ToolFixture.FindValidToolModel();
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();

            _toolTypeRepositoryMock.Setup(tt => tt.FindByAsync(It.IsAny<int>()))
                       .ReturnsAsync((ToolTypeDto?)null);
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                   .ReturnsAsync(tool);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.UpdateAsync(toolUpdateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("ToolType not found", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Tool>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should throw AppException when connector url is empty")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_EmptyConnectorUrl_ThrowsAppException()
        {
            // Arrange
            var tool = ToolFixture.FindValidToolModel();
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            toolUpdateDto.ConnectorUrl = string.Empty;
            var toolType = ToolTypeFixture.FindValidToolTypeWithName("n8n");            

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>()))
                                   .ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                               .ReturnsAsync(tool);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.UpdateAsync(toolUpdateDto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal("Coonector Url is required", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Tool>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should throw AppException when connector api key is empty")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_EmptyConnectorApiKey_ThrowsAppException()
        {
            // Arrange
            var tool = ToolFixture.FindValidToolModel();
            tool.Update(tool.Name, tool.ToolTypeId, tool.InputDataId, tool.OutputDataId, tool.IsEditableInput, tool.ConnectorUrl, string.Empty);

            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            toolUpdateDto.ConnectorApiKey = string.Empty;
            var toolType = ToolTypeFixture.FindValidToolTypeWithName("n8n");            

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>()))
                                   .ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                               .ReturnsAsync(tool);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.UpdateAsync(toolUpdateDto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal("Coonector Api Key is required", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Tool>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should throw AppException when  when Tool is duplicated")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_UpdateFails_ThrowsAppException()
        {
            // Arrange
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            var tool = ToolFixture.FindValidToolModel();
            var toolType = ToolTypeFixture.FindValidToolType();

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>()))
                                   .ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(toolUpdateDto.Id))
                               .ReturnsAsync(tool);
            _toolRepositoryMock.Setup(repo => repo.UpdateAsync(tool))
                               .ReturnsAsync(false);
            _keyVaultServicesMock.Setup(k => k.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                                 .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.UpdateAsync(toolUpdateDto));
            Assert.Equal(ErrorCode.Duplicated, exception.ErrorCode);
            Assert.Equal("Duplicated Tool", exception.Message);
            _toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(toolUpdateDto.Id), Times.Once);
            _toolRepositoryMock.Verify(repo => repo.UpdateAsync(tool), Times.Once);
            _toolTypeRepositoryMock.Verify(tt => tt.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should return true when Tool is updated successfully")]
        [Trait("CreateAsync", "Success")]
        public async Task UpdateAsync_SuccessfulUpdate_ReturnsTrue()
        {
            // Arrange
            var toolUpdateDto = ToolFixture.FindValidToolUpdateDto();
            var tool = ToolFixture.FindValidToolModel();
            var toolType = ToolTypeFixture.FindValidToolType();

            _toolTypeRepositoryMock.Setup(tt => tt.FindModelByIdAsync(It.IsAny<int>()))
                                   .ReturnsAsync(toolType);
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(toolUpdateDto.Id))
                               .ReturnsAsync(tool);
            _toolRepositoryMock.Setup(repo => repo.UpdateAsync(tool))
                               .ReturnsAsync(true);

            // Act
            var result = await _toolServices.UpdateAsync(toolUpdateDto);

            // Assert
            Assert.True(result);
            _toolTypeRepositoryMock.Verify(repo => repo.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
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

        [Fact(DisplayName = "ValidateConnector should throw exception when connector Url is empty")]
        [Trait("ValidateConnector", "Fail")]
        public async Task ValidateConnector_ShouldThrowException_WhenConnectorUrlIsEmpty()
        {
            // Arrange
            var toolConnectorDto = ToolFixture.FindValidToolConnectorDto();
            toolConnectorDto.ConnectorUrl = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.ValidateConnector(toolConnectorDto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
        }

        [Fact(DisplayName = "ValidateConnector should throw exception when connector ApiKey is empty")]
        [Trait("ValidateConnector", "Fail")]
        public async Task ValidateConnector_ShouldThrowException_WhenConnectorApiKeyIsEmpty()
        {
            // Arrange
            var toolConnectorDto = ToolFixture.FindValidToolConnectorDto();
            toolConnectorDto.ConnectorApiKey = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _toolServices.ValidateConnector(toolConnectorDto));
            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
        }

        [Fact(DisplayName = "ValidateConnector should return true when response is success")]
        [Trait("ValidateConnector", "Success")]
        public async Task ValidateConnector_ShouldReturnTrue_WhenResponseIsSuccess()
        {
            // Arrange
            var toolConnectorDto = ToolFixture.FindValidToolConnectorDto();

            var response = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.OK), string.Empty, new RefitSettings());

            _in8nConnectorMock.Setup(x => x.FindWorkflows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                              .ReturnsAsync(response);

            _apiClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                 .Returns(_in8nConnectorMock.Object);

            // Act
            var result = await _toolServices.ValidateConnector(toolConnectorDto);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "ValidateConnector should return false when response is not success")]
        [Trait("ValidateConnector", "Fail")]
        public async Task ValidateConnector_ShouldReturnFalse_WhenResponseIsNotSuccess()
        {
            // Arrange
            var toolConnectorDto = ToolFixture.FindValidToolConnectorDto();

            var response = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.BadRequest), string.Empty, new RefitSettings());

            _in8nConnectorMock.Setup(x => x.FindWorkflows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                              .ReturnsAsync(response);

            _apiClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                 .Returns(_in8nConnectorMock.Object);

            // Act
            var result = await _toolServices.ValidateConnector(toolConnectorDto);

            // Assert
            Assert.False(result);
        }
    }
}
