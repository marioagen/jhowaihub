using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class ApiTemplateServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly ApiTemplateServices _apiTemplateServices;

        public ApiTemplateServicesTests()
        {
            _mocker = new AutoMocker();
            _apiTemplateServices = _mocker.CreateInstance<ApiTemplateServices>();
        }

        #region FindById Tests

        [Fact(DisplayName = "FindById should return template when template exists")]
        [Trait("FindById", "Success")]
        public async Task FindById_WhenTemplateExists_ShouldReturnTemplate()
        {
            // Arrange
            var templateId = 1;
            var expectedTemplate = new ApiTemplateDto
            {
                Id = templateId,
                Name = "Test Template",
                Method = "GET",
                Url = "https://api.example.com/test",
                Created = DateTime.UtcNow,
                QueryTemplate = "param=value",
                HeaderTemplate = "Authorization: Bearer token",
                BodyTemplate = null
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindById(templateId))
                .ReturnsAsync(expectedTemplate);

            // Act
            var result = await _apiTemplateServices.FindById(templateId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(templateId, result.Id);
            Assert.Equal("Test Template", result.Name);
            Assert.Equal("GET", result.Method);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindById(templateId), Times.Once);
        }

        [Fact(DisplayName = "FindById should throw AppException when template not found")]
        [Trait("FindById", "Fail")]
        public async Task FindById_WhenTemplateNotFound_ShouldThrowAppException()
        {
            // Arrange
            var templateId = 999;

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindById(templateId))
                .ReturnsAsync((ApiTemplateDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _apiTemplateServices.FindById(templateId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Template not found", exception.Message);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindById(templateId), Times.Once);
        }

        #endregion

        #region DeleteById Tests

        [Fact(DisplayName = "DeleteById should delete template when template exists")]
        [Trait("DeleteById", "Success")]
        public async Task DeleteById_WhenTemplateExists_ShouldDeleteSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var existingTemplate = new ApiTemplate(
                "Test Template",
                "POST",
                "https://api.example.com/test",
                null,
                null,
                "{\"key\":\"value\"}",
                null,
                false
            );

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync(existingTemplate);

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.DeleteById(templateId))
                .ReturnsAsync(true);

            // Act
            var result = await _apiTemplateServices.DeleteById(templateId);

            // Assert
            Assert.True(result);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindByIdReturnModel(templateId), Times.Once);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.DeleteById(templateId), Times.Once);
        }

        [Fact(DisplayName = "DeleteById should throw AppException when template not found")]
        [Trait("DeleteById", "Fail")]
        public async Task DeleteById_WhenTemplateNotFound_ShouldThrowAppException()
        {
            // Arrange
            var templateId = 999;

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync((ApiTemplate?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _apiTemplateServices.DeleteById(templateId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Template not found", exception.Message);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindByIdReturnModel(templateId), Times.Once);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.DeleteById(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region FindAll Tests

        [Fact(DisplayName = "FindAll should return collection of templates")]
        [Trait("FindAll", "Success")]
        public async Task FindAll_WhenTemplatesExist_ShouldReturnCollection()
        {
            // Arrange
            var filterDto = new ApiTemplateFilterDto
            {
                Input = "test",
                Method = "GET",
                OrderBy = "Name"
            };

            var templates = new List<ApiTemplateDto>
            {
                new ApiTemplateDto { Id = 1, Name = "Template 1", Method = "GET", Url = "https://api.example.com/1", Created = DateTime.Now },
                new ApiTemplateDto { Id = 2, Name = "Template 2", Method = "POST", Url = "https://api.example.com/2", Created = DateTime.Now }
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindAll(filterDto))
                .ReturnsAsync(templates);

            // Act
            var result = await _apiTemplateServices.FindAll(filterDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Name == "Template 1");
            Assert.Contains(result, t => t.Name == "Template 2");
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindAll(filterDto), Times.Once);
        }

        [Fact(DisplayName = "FindAll should return empty collection when no templates match")]
        [Trait("FindAll", "Success")]
        public async Task FindAll_WhenNoTemplatesMatch_ShouldReturnEmptyCollection()
        {
            // Arrange
            var filterDto = new ApiTemplateFilterDto
            {
                Input = "nonexistent",
                Method = "DELETE"
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindAll(filterDto))
                .ReturnsAsync(new List<ApiTemplateDto>());

            // Act
            var result = await _apiTemplateServices.FindAll(filterDto);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindAll(filterDto), Times.Once);
        }

        #endregion

        #region FindAllPaged Tests

        [Fact(DisplayName = "FindAllPaged should return paginated result when page is valid")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_WhenPageIsValid_ShouldReturnPaginatedResult()
        {
            // Arrange
            var pagedFilterDto = new ApiTemplatePagedFilterDto
            {
                Page = 1,
                PageSize = 10,
                Input = "test",
                Method = "GET"
            };

            var templates = new List<ApiTemplateDto>
            {
                new ApiTemplateDto { Id = 1, Name = "Template 1", Method = "GET", Url = "https://api.example.com/1", Created = DateTime.UtcNow },
                new ApiTemplateDto { Id = 2, Name = "Template 2", Method = "GET", Url = "https://api.example.com/2", Created = DateTime.UtcNow },
                new ApiTemplateDto { Id = 3, Name = "Template 3", Method = "GET", Url = "https://api.example.com/3", Created = DateTime.UtcNow }
            }.AsQueryable();

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindAllPaged(pagedFilterDto))
                .Returns(templates);

            // Act
            var result = _apiTemplateServices.FindAllPaged(pagedFilterDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Content);
            Assert.Equal(1, result.CurrentPage);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindAllPaged(pagedFilterDto), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged should throw ArgumentException when page is invalid")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_WhenPageIsInvalid_ShouldThrowArgumentException()
        {
            // Arrange
            var pagedFilterDto = new ApiTemplatePagedFilterDto
            {
                Page = 0,
                PageSize = 10
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _apiTemplateServices.FindAllPaged(pagedFilterDto));
            Assert.Equal("Invalid Page", exception.Message);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindAllPaged(It.IsAny<ApiTemplatePagedFilterDto>()), Times.Never);
            _mocker.GetMock<ILogger<ApiTemplateServices>>()
                .Verify(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged should throw ArgumentException when page is negative")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_WhenPageIsNegative_ShouldThrowArgumentException()
        {
            // Arrange
            var pagedFilterDto = new ApiTemplatePagedFilterDto
            {
                Page = -1,
                PageSize = 10
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _apiTemplateServices.FindAllPaged(pagedFilterDto));
            Assert.Equal("Invalid Page", exception.Message);
        }

        #endregion

        #region CreateAsync Tests

        [Fact(DisplayName = "CreateAsync should create template successfully")]
        [Trait("CreateAsync", "Success")]
        public async Task CreateAsync_WhenValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var createDto = new ApiTemplateCreateDto
            {
                Name = "New Template",
                Method = "POST",
                Url = "https://api.example.com/create",
                QueryTemplate = "param=value",
                HeaderTemplate = "Content-Type: application/json",
                BodyTemplate = "{\"data\":\"value\"}"
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.CreateAsync(It.IsAny<ApiTemplate>()))
                .ReturnsAsync(true);

            // Act
            var result = await _apiTemplateServices.CreateAsync(createDto);

            // Assert
            Assert.True(result);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.CreateAsync(It.Is<ApiTemplate>(t =>
                    t.Name == "New Template" &&
                    t.Method == "POST" &&
                    t.Url == "https://api.example.com/create")), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should create template with minimal data")]
        [Trait("CreateAsync", "Success")]
        public async Task CreateAsync_WhenMinimalData_ShouldCreateSuccessfully()
        {
            // Arrange
            var createDto = new ApiTemplateCreateDto
            {
                Name = "Minimal Template",
                Method = "GET",
                Url = "https://api.example.com/minimal",
                QueryTemplate = null,
                HeaderTemplate = null,
                BodyTemplate = null
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.CreateAsync(It.IsAny<ApiTemplate>()))
                .ReturnsAsync(true);

            // Act
            var result = await _apiTemplateServices.CreateAsync(createDto);

            // Assert
            Assert.True(result);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.CreateAsync(It.Is<ApiTemplate>(t =>
                    t.Name == "Minimal Template" &&
                    t.QueryTemplate == null &&
                    t.HeaderTemplate == null &&
                    t.BodyTemplate == null)), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync should throw ArgumentException when name is empty")]
        [Trait("CreateAsync", "Fail")]
        public async Task CreateAsync_WhenNameIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var createDto = new ApiTemplateCreateDto
            {
                Name = "",
                Method = "GET",
                Url = "https://api.example.com/test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _apiTemplateServices.CreateAsync(createDto));
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.CreateAsync(It.IsAny<ApiTemplate>()), Times.Never);
        }

        [Fact(DisplayName = "CreateAsync should throw ArgumentException when method is invalid")]
        [Trait("CreateAsync", "Fail")]
        public async Task CreateAsync_WhenMethodIsInvalid_ShouldThrowArgumentException()
        {
            // Arrange
            var createDto = new ApiTemplateCreateDto
            {
                Name = "Test Template",
                Method = "INVALID",
                Url = "https://api.example.com/test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _apiTemplateServices.CreateAsync(createDto));
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.CreateAsync(It.IsAny<ApiTemplate>()), Times.Never);
        }

        [Fact(DisplayName = "CreateAsync should throw ArgumentException when url is empty")]
        [Trait("CreateAsync", "Fail")]
        public async Task CreateAsync_WhenUrlIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var createDto = new ApiTemplateCreateDto
            {
                Name = "Test Template",
                Method = "GET",
                Url = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _apiTemplateServices.CreateAsync(createDto));
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.CreateAsync(It.IsAny<ApiTemplate>()), Times.Never);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact(DisplayName = "UpdateAsync should update template successfully")]
        [Trait("UpdateAsync", "Success")]
        public async Task UpdateAsync_WhenTemplateExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var existingTemplate = new ApiTemplate(
                "Old Name",
                "GET",
                "https://api.example.com/old",
                null,
                null,
                null,
                null,
                false
            );

            var updateDto = new ApiTemplateUpdateDto
            {
                Id = templateId,
                Name = "Updated Name",
                Method = "POST",
                Url = "https://api.example.com/updated",
                QueryTemplate = "new=param",
                HeaderTemplate = "Authorization: Bearer newtoken",
                BodyTemplate = "{\"updated\":\"data\"}"
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync(existingTemplate);

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<ApiTemplate>()))
                .ReturnsAsync(true);

            // Act
            var result = await _apiTemplateServices.UpdateAsync(updateDto);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Name", existingTemplate.Name);
            Assert.Equal("POST", existingTemplate.Method);
            Assert.Equal("https://api.example.com/updated", existingTemplate.Url);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindByIdReturnModel(templateId), Times.Once);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.UpdateAsync(existingTemplate), Times.Once);
        }

        [Fact(DisplayName = "UpdateAsync should throw AppException when template not found")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_WhenTemplateNotFound_ShouldThrowAppException()
        {
            // Arrange
            var templateId = 999;
            var updateDto = new ApiTemplateUpdateDto
            {
                Id = templateId,
                Name = "Updated Name",
                Method = "PUT",
                Url = "https://api.example.com/updated"
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync((ApiTemplate?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _apiTemplateServices.UpdateAsync(updateDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Template not found", exception.Message);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.FindByIdReturnModel(templateId), Times.Once);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.UpdateAsync(It.IsAny<ApiTemplate>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should throw ArgumentException when updated name is empty")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_WhenUpdatedNameIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var templateId = 1;
            var existingTemplate = new ApiTemplate(
                "Old Name",
                "GET",
                "https://api.example.com/old",
                null,
                null,
                null,
                null,
                false
            );

            var updateDto = new ApiTemplateUpdateDto
            {
                Id = templateId,
                Name = "",
                Method = "GET",
                Url = "https://api.example.com/updated"
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync(existingTemplate);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _apiTemplateServices.UpdateAsync(updateDto));
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.UpdateAsync(It.IsAny<ApiTemplate>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should throw ArgumentException when updated method is invalid")]
        [Trait("UpdateAsync", "Fail")]
        public async Task UpdateAsync_WhenUpdatedMethodIsInvalid_ShouldThrowArgumentException()
        {
            // Arrange
            var templateId = 1;
            var existingTemplate = new ApiTemplate(
                "Test Template",
                "GET",
                "https://api.example.com/test",
                null,
                null,
                null,
                null,
                false
            );

            var updateDto = new ApiTemplateUpdateDto
            {
                Id = templateId,
                Name = "Test Template",
                Method = "INVALID_METHOD",
                Url = "https://api.example.com/test"
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync(existingTemplate);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _apiTemplateServices.UpdateAsync(updateDto));
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.UpdateAsync(It.IsAny<ApiTemplate>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync should update template with null optional fields")]
        [Trait("UpdateAsync", "Success")]
        public async Task UpdateAsync_WhenOptionalFieldsAreNull_ShouldUpdateSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var existingTemplate = new ApiTemplate(
                "Test Template",
                "GET",
                "https://api.example.com/test",
                "old=query",
                "old: header",
                "{\"old\":\"body\"}",
                "Test APi",
                false
            );

            var updateDto = new ApiTemplateUpdateDto
            {
                Id = templateId,
                Name = "Updated Template",
                Method = "PUT",
                Url = "https://api.example.com/updated",
                QueryTemplate = null,
                HeaderTemplate = null,
                BodyTemplate = null
            };

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.FindByIdReturnModel(templateId))
                .ReturnsAsync(existingTemplate);

            _mocker.GetMock<IApiTemplateRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<ApiTemplate>()))
                .ReturnsAsync(true);

            // Act
            var result = await _apiTemplateServices.UpdateAsync(updateDto);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Template", existingTemplate.Name);
            Assert.Null(existingTemplate.QueryTemplate);
            Assert.Null(existingTemplate.HeaderTemplate);
            Assert.Null(existingTemplate.BodyTemplate);
            _mocker.GetMock<IApiTemplateRepository>()
                .Verify(x => x.UpdateAsync(existingTemplate), Times.Once);
        }

        #endregion
    }
}
