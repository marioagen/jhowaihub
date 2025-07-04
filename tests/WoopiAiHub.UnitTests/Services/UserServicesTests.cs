using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class UserServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserServices>> _loggerMock;
        private readonly Mock<IMarketPlaceApi> _marketPlaceApiMock;
        private readonly UserServices _userServices;

        public UserServicesTests()
        {
            _mocker = new AutoMocker();
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserServices>>();
            _marketPlaceApiMock = new Mock<IMarketPlaceApi>();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(config => config[It.Is<string>(s => s == "keyAccess")]).Returns("mockKeyAccess");
            configMock.Setup(x => x.GetSection("KeyAccess").Value).Returns(Guid.NewGuid().ToString());

            _mocker.Use(configMock);

            _userServices = new UserServices(
                _userRepositoryMock.Object,
                _loggerMock.Object,
                _marketPlaceApiMock.Object,
                configMock.Object
            );
        }

        [Fact]
        public async Task Create_ShouldReturnTrue_WhenUserIsCreated()
        {
            // Arrange
            var userCreateDto = new UserCreateDto { Name = "Test", Email = "test@email.com", Teams = new List<Team>() };
            var headersDto = new HeadersDto { Tenant = "tenant" };
            var userId = Guid.NewGuid();

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.Create(It.IsAny<User>()))
                .Returns(true);

            // Act
            var result = await _userServices.Create(userCreateDto, headersDto);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.Create(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturnFalse_WhenUserNotEnabled()
        {
            // Arrange
            var userCreateDto = new UserCreateDto { Name = "Test", Email = "test@email.com", Teams = new List<Team>() };
            var headersDto = new HeadersDto { Tenant = "tenant" };

            // Act
            var result = await _userServices.Create(userCreateDto, headersDto);

            // Assert
            Assert.False(result);
        }

        //[Fact]
        //public void DeleteByIds_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        //{
        //    // Arrange
        //    var ids = new List<Guid> { Guid.NewGuid() };
        //    _userRepositoryMock.Setup(repo => repo.DeleteByIds(ids)).Returns(true);

        //    // Act
        //    var result = _userServices.DeleteByIds(ids);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public void DeleteByIds_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        //{
        //    // Arrange
        //    var ids = new List<Guid> { Guid.NewGuid() };
        //    _userRepositoryMock.Setup(repo => repo.DeleteByIds(ids)).Returns(false);

        //    // Act
        //    var result = _userServices.DeleteByIds(ids);

        //    // Assert
        //    Assert.False(result);
        //}

        //[Fact]
        //public void Update_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        //{
        //    // Arrange
        //    var updateDto = new UserUpdateDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@email.com" };
        //    _userRepositoryMock.Setup(repo => repo.Update(updateDto)).Returns(true);

        //    // Act
        //    var result = _userServices.Update(updateDto);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public void Update_ShouldThrowArgumentException_WhenRepositoryReturnsFalse()
        //{
        //    // Arrange
        //    var updateDto = new UserUpdateDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@email.com" };
        //    _userRepositoryMock.Setup(repo => repo.Update(updateDto)).Returns(false);

        //    // Act & Assert
        //    var ex = Assert.Throws<ArgumentException>(() => _userServices.Update(updateDto));
        //    Assert.Equal("Duplicated User", ex.Message);
        //}
    }
}

