using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(UsageCollection))]
    public class UsageDailyServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly UsageDailyServices _service;
        private readonly Mock<IUsageDailyRepository> _usageDailyRepositoryMock;
        private readonly Mock<IUsageTypeServices> _usageTypeServicesMock;
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly Mock<IModelEmbeddingRepository> _modelEmbeddingRepositoryMock;
        private readonly UsageFixture _fixture;

        public UsageDailyServicesTests()
        {
            _fixture = new UsageFixture();
            _mocker = new AutoMocker();
            _usageDailyRepositoryMock = _mocker.GetMock<IUsageDailyRepository>();
            _usageTypeServicesMock = _mocker.GetMock<IUsageTypeServices>();
            _userServicesMock = _mocker.GetMock<IUserServices>();
            _modelEmbeddingRepositoryMock = _mocker.GetMock<IModelEmbeddingRepository>();
            _service = _mocker.CreateInstance<UsageDailyServices>();
        }

        [Fact(DisplayName = "AddAsync should return true when usage daily is added successfully")]
        [Trait("AddAsync", "Success")]
        public async Task AddAsync_ShouldReturnTrue_WhenUsageDailyIsAdded()
        {
            // Arrange
            var usageDailyDto = _fixture.CreateValidUsageDailyDto();
            _usageDailyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<UsageDaily>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddAsync(usageDailyDto);

            // Assert
            Assert.True(result);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Once);
        }

        [Fact(DisplayName = "AddAsync should return false when repository fails")]
        [Trait("AddAsync", "Fail")]
        public async Task AddAsync_ShouldReturnFalse_WhenRepositoryFails()
        {
            // Arrange
            var usageDailyDto = _fixture.CreateValidUsageDailyDto();
            _usageDailyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<UsageDaily>()))
                                    .ReturnsAsync(false);

            // Act
            var result = await _service.AddAsync(usageDailyDto);

            // Assert
            Assert.False(result);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Once);
        }

        [Fact(DisplayName = "AddByValuesAsync should return true when all dependencies are found")]
        [Trait("AddByValuesAsync", "Success")]
        public async Task AddByValuesAsync_ShouldReturnTrue_WhenAllDependenciesAreFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var count = 5;

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(userId);
            _usageDailyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<UsageDaily>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddByValuesAsync(usageType.Name, email, count);

            // Assert
            Assert.True(result);
            _usageTypeServicesMock.Verify(s => s.FindByNameAsync(usageType.Name), Times.Once);
            _userServicesMock.Verify(s => s.FindIdByEmail(email), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Once);
        }

        [Fact(DisplayName = "AddByValuesAsync should return false when usage type is not found")]
        [Trait("AddByValuesAsync", "Fail")]
        public async Task AddByValuesAsync_ShouldReturnFalse_WhenUsageTypeNotFound()
        {
            // Arrange
            var email = "test@example.com";
            var count = 5;

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(It.IsAny<string>()))
                                 .ReturnsAsync((UsageType?)null);

            // Act
            var result = await _service.AddByValuesAsync("InvalidType", email, count);

            // Assert
            Assert.False(result);
            _usageTypeServicesMock.Verify(s => s.FindByNameAsync("InvalidType"), Times.Once);
            _userServicesMock.Verify(s => s.FindIdByEmail(It.IsAny<string>()), Times.Never);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Never);
        }

        [Fact(DisplayName = "AddByValuesAsync should return false when user is not found")]
        [Trait("AddByValuesAsync", "Fail")]
        public async Task AddByValuesAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var email = "test@example.com";
            var count = 5;

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(Guid.Empty);

            // Act
            var result = await _service.AddByValuesAsync(usageType.Name, email, count);

            // Assert
            Assert.False(result);
            _usageTypeServicesMock.Verify(s => s.FindByNameAsync(usageType.Name), Times.Once);
            _userServicesMock.Verify(s => s.FindIdByEmail(email), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Never);
        }

        [Fact(DisplayName = "AddByValuesAsync should retrieve model embedding ID when model embedding name is provided")]
        [Trait("AddByValuesAsync", "Success With Model Embedding")]
        public async Task AddByValuesAsync_ShouldRetrieveModelEmbeddingId_WhenModelEmbeddingNameProvided()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var modelEmbedding = _fixture.CreateValidModelEmbedding();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var count = 5;

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(userId);
            _modelEmbeddingRepositoryMock.Setup(r => r.FindByNameAsync(modelEmbedding.Name))
                                        .ReturnsAsync(modelEmbedding);
            _usageDailyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<UsageDaily>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddByValuesAsync(usageType.Name, email, count, modelEmbedding.Name);

            // Assert
            Assert.True(result);
            _modelEmbeddingRepositoryMock.Verify(r => r.FindByNameAsync(modelEmbedding.Name), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.Is<UsageDaily>(u => u.ModelEmbeddingId == modelEmbedding.Id)), Times.Once);
        }

        [Fact(DisplayName = "AddByValuesAsync should use zero for model embedding ID when model embedding is not found")]
        [Trait("AddByValuesAsync", "Success With Missing Model Embedding")]
        public async Task AddByValuesAsync_ShouldUseZeroForModelEmbeddingId_WhenModelEmbeddingNotFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var count = 5;
            var modelEmbeddingName = "NonExistentModel";

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(userId);
            _modelEmbeddingRepositoryMock.Setup(r => r.FindByNameAsync(modelEmbeddingName))
                                        .ReturnsAsync((ModelEmbedding?)null);
            _usageDailyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<UsageDaily>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddByValuesAsync(usageType.Name, email, count, modelEmbeddingName);

            // Assert
            Assert.True(result);
            _modelEmbeddingRepositoryMock.Verify(r => r.FindByNameAsync(modelEmbeddingName), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Once);
        }

        [Fact(DisplayName = "AddByValuesAsync should not call model embedding repository when model embedding name is empty")]
        [Trait("AddByValuesAsync", "Success Without Model Embedding")]
        public async Task AddByValuesAsync_ShouldNotCallModelEmbeddingRepository_WhenModelEmbeddingNameIsEmpty()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var count = 5;

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(It.IsAny<string>()))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(It.IsAny<string>()))
                            .Returns(userId);
            _usageDailyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<UsageDaily>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddByValuesAsync(usageType.Name, email, count, "");

            // Assert
            Assert.True(result);
            _modelEmbeddingRepositoryMock.Verify(r => r.FindByNameAsync(It.IsAny<string>()), Times.Never);
            _usageDailyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UsageDaily>()), Times.Once);
        }

        [Fact(DisplayName = "AddRangeAsync should return true when usage dailies are added successfully")]
        [Trait("AddRangeAsync", "Success")]
        public async Task AddRangeAsync_ShouldReturnTrue_WhenUsageDailiesAreAdded()
        {
            // Arrange
            var usageDailyDtos = new List<UsageDailyDto>
            {
                _fixture.CreateValidUsageDailyDto(),
                _fixture.CreateValidUsageDailyDto()
            };
            _usageDailyRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddRangeAsync(usageDailyDtos);

            // Assert
            Assert.True(result);
            _usageDailyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()), Times.Once);
        }

        [Fact(DisplayName = "AddRangeAsync should return false when repository fails")]
        [Trait("AddRangeAsync", "Fail")]
        public async Task AddRangeAsync_ShouldReturnFalse_WhenRepositoryFails()
        {
            // Arrange
            var usageDailyDtos = new List<UsageDailyDto>
            {
                _fixture.CreateValidUsageDailyDto()
            };
            _usageDailyRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()))
                                    .ReturnsAsync(false);

            // Act
            var result = await _service.AddRangeAsync(usageDailyDtos);

            // Assert
            Assert.False(result);
            _usageDailyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()), Times.Once);
        }

        [Fact(DisplayName = "AddByRangeValuesAsync should return true when all dependencies are found")]
        [Trait("AddByRangeValuesAsync", "Success")]
        public async Task AddByRangeValuesAsync_ShouldReturnTrue_WhenAllDependenciesAreFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var modelEmbedding = _fixture.CreateValidModelEmbedding();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var usages = new List<(string Model, int TotalUsage)>
            {
                (modelEmbedding.Name, 10)
            };

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(userId);
            _modelEmbeddingRepositoryMock.Setup(r => r.FindByNameAsync(modelEmbedding.Name))
                                        .ReturnsAsync(modelEmbedding);
            _usageDailyRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddByRangeValuesAsync(usageType.Name, email, usages);

            // Assert
            Assert.True(result);
            _usageTypeServicesMock.Verify(s => s.FindByNameAsync(usageType.Name), Times.Once);
            _userServicesMock.Verify(s => s.FindIdByEmail(email), Times.Once);
            _modelEmbeddingRepositoryMock.Verify(r => r.FindByNameAsync(modelEmbedding.Name), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()), Times.Once);
        }

        [Fact(DisplayName = "AddByRangeValuesAsync should return false when usage type is not found")]
        [Trait("AddByRangeValuesAsync", "Fail")]
        public async Task AddByRangeValuesAsync_ShouldReturnFalse_WhenUsageTypeNotFound()
        {
            // Arrange
            var email = "test@example.com";
            var usages = new List<(string Model, int TotalUsage)> { ("Model1", 10) };

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(It.IsAny<string>()))
                                 .ReturnsAsync((UsageType?)null);

            // Act
            var result = await _service.AddByRangeValuesAsync("InvalidType", email, usages);

            // Assert
            Assert.False(result);
            _usageTypeServicesMock.Verify(s => s.FindByNameAsync("InvalidType"), Times.Once);
            _userServicesMock.Verify(s => s.FindIdByEmail(It.IsAny<string>()), Times.Never);
            _usageDailyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()), Times.Never);
        }

        [Fact(DisplayName = "AddByRangeValuesAsync should return false when user is not found")]
        [Trait("AddByRangeValuesAsync", "Fail")]
        public async Task AddByRangeValuesAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var email = "test@example.com";
            var usages = new List<(string Model, int TotalUsage)> { ("Model1", 10) };

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(Guid.Empty);

            // Act
            var result = await _service.AddByRangeValuesAsync(usageType.Name, email, usages);

            // Assert
            Assert.False(result);
            _usageTypeServicesMock.Verify(s => s.FindByNameAsync(usageType.Name), Times.Once);
            _userServicesMock.Verify(s => s.FindIdByEmail(email), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()), Times.Never);
        }

        [Fact(DisplayName = "AddByRangeValuesAsync should continue when some model embeddings are not found")]
        [Trait("AddByRangeValuesAsync", "Partial Success")]
        public async Task AddByRangeValuesAsync_ShouldContinue_WhenModelEmbeddingNotFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            var modelEmbedding = _fixture.CreateValidModelEmbedding();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var usages = new List<(string Model, int TotalUsage)>
            {
                (modelEmbedding.Name, 10),
                ("NonExistentModel", 5)
            };

            _usageTypeServicesMock.Setup(s => s.FindByNameAsync(usageType.Name))
                                 .ReturnsAsync(usageType);
            _userServicesMock.Setup(s => s.FindIdByEmail(email))
                            .Returns(userId);
            _modelEmbeddingRepositoryMock.Setup(r => r.FindByNameAsync(modelEmbedding.Name))
                                        .ReturnsAsync(modelEmbedding);
            _modelEmbeddingRepositoryMock.Setup(r => r.FindByNameAsync("NonExistentModel"))
                                        .ReturnsAsync((ModelEmbedding?)null);
            _usageDailyRepositoryMock.Setup(r => r.AddRangeAsync(It.Is<List<UsageDaily>>(list => list.Count == 1)))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddByRangeValuesAsync(usageType.Name, email, usages);

            // Assert
            Assert.True(result);
            _modelEmbeddingRepositoryMock.Verify(r => r.FindByNameAsync(modelEmbedding.Name), Times.Once);
            _modelEmbeddingRepositoryMock.Verify(r => r.FindByNameAsync("NonExistentModel"), Times.Once);
            _usageDailyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<UsageDaily>>()), Times.Once);
        }
    }
}
