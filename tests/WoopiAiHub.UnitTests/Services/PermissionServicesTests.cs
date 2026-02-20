using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;
using Moq;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.UnitTests.Services
{
    public class PermissionServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly PermissionServices _permissionServices;

        public PermissionServicesTests()
        {
            _mocker = new AutoMocker();
            _permissionServices = _mocker.CreateInstance<PermissionServices>();
        }

        [Fact(DisplayName = "Test FindAll and returns valid permissions")]
        [Trait("FindAll", "Success")]
        public void FindAll_ReturnsPermissions_WhenPermissionsExist()
        {
            // Arrange
            var mockRepo = _mocker.GetMock<IPermissionRepository>();
            mockRepo.Setup(r => r.FindAll()).Returns(new List<PermissionDto> { new PermissionDto { Id = 1, Name = "Test" } });

            // Act
            var result = _permissionServices.FindAll();

            // Assert
            Assert.Single(result);
        }

        [Fact(DisplayName = "Test FindAll and returns empty when no permissions exists")]
        [Trait("FindAll", "Empty")]
        public void FindAll_ReturnsEmpty_WhenNoPermissionsExist()
        {
            var mockRepo = _mocker.GetMock<IPermissionRepository>();
            mockRepo.Setup(r => r.FindAll()).Returns(new List<PermissionDto>());

            var result = _permissionServices.FindAll();

            Assert.Empty(result);
        }

        [Fact(DisplayName = "Test FindAll and throws exception when there is a database error")]
        [Trait("FindAll", "Fail")]
        public void FindAll_ThrowsException_WhenRepositoryThrows()
        {
            var mockRepo = _mocker.GetMock<IPermissionRepository>();
            mockRepo.Setup(r => r.FindAll()).Throws(new Exception("DB error"));

            Assert.Throws<Exception>(() => _permissionServices.FindAll());
        }

        [Fact(DisplayName = "UserHasPermissionAsync should throw AppException when user is not found")]
        [Trait("UserHasPermissionAsync", "Fail")]
        public async Task UserHasPermissionAsync_ShouldThrowAppException_WhenUserNotFound()
        {
            // Arrange
            var email = "notfound@example.com";
            var group = "Actions";
            var permission = "Read";

            var userRepositoryMock = _mocker.GetMock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserProfilesByEmailAsync(email))
                .ReturnsAsync((List<string>)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _permissionServices.UserHasPermissionAsync(email, group, permission));

            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("User not found", exception.Message);
            Assert.Equal(UserLabel.NotFound, exception.LabelError);
            userRepositoryMock.Verify(repo => repo.FindUserProfilesByEmailAsync(email), Times.Once);
        }

        [Fact(DisplayName = "UserHasPermissionAsync should return true when user is admin")]
        [Trait("UserHasPermissionAsync", "Success")]
        public async Task UserHasPermissionAsync_ShouldReturnTrue_WhenUserIsAdmin()
        {
            // Arrange
            var email = "admin@example.com";
            var group = "Actions";
            var permission = "Delete";
            var profiles = new List<string> { "admin", "user" };
            var permissions = new Dictionary<string, List<string>>
            {
                { "Actions", new List<string> { "Read", "Write" } }
            };

            var userRepositoryMock = _mocker.GetMock<IUserRepository>();
            var permissionRepositoryMock = _mocker.GetMock<IPermissionRepository>();

            userRepositoryMock.Setup(repo => repo.FindUserProfilesByEmailAsync(email))
                .ReturnsAsync(profiles);
            permissionRepositoryMock.Setup(repo => repo.FindUserPermissionsAsync(email))
                .ReturnsAsync(permissions);

            // Act
            var result = await _permissionServices.UserHasPermissionAsync(email, group, permission);

            // Assert
            Assert.True(result);
            userRepositoryMock.Verify(repo => repo.FindUserProfilesByEmailAsync(email), Times.Once);
            permissionRepositoryMock.Verify(repo => repo.FindUserPermissionsAsync(email), Times.Once);
        }

        [Fact(DisplayName = "UserHasPermissionAsync should return true when user has the specific permission")]
        [Trait("UserHasPermissionAsync", "Success")]
        public async Task UserHasPermissionAsync_ShouldReturnTrue_WhenUserHasPermission()
        {
            // Arrange
            var email = "user@example.com";
            var group = "Actions";
            var permission = "Read";
            var profiles = new List<string> { "user", "analyst" };
            var permissions = new Dictionary<string, List<string>>
            {
                { "Actions", new List<string> { "Read", "Write" } }
            };

            var userRepositoryMock = _mocker.GetMock<IUserRepository>();
            var permissionRepositoryMock = _mocker.GetMock<IPermissionRepository>();

            userRepositoryMock.Setup(repo => repo.FindUserProfilesByEmailAsync(email))
                .ReturnsAsync(profiles);
            permissionRepositoryMock.Setup(repo => repo.FindUserPermissionsAsync(email))
                .ReturnsAsync(permissions);

            // Act
            var result = await _permissionServices.UserHasPermissionAsync(email, group, permission);

            // Assert
            Assert.True(result);
            userRepositoryMock.Verify(repo => repo.FindUserProfilesByEmailAsync(email), Times.Once);
            permissionRepositoryMock.Verify(repo => repo.FindUserPermissionsAsync(email), Times.Once);
        }

        [Fact(DisplayName = "UserHasPermissionAsync should return false when user does not have the permission")]
        [Trait("UserHasPermissionAsync", "Fail")]
        public async Task UserHasPermissionAsync_ShouldReturnFalse_WhenUserDoesNotHavePermission()
        {
            // Arrange
            var email = "user@example.com";
            var group = "Actions";
            var permission = "Delete";
            var profiles = new List<string> { "user" };
            var permissions = new Dictionary<string, List<string>>
            {
                { "Actions", new List<string> { "Read", "Write" } }
            };

            var userRepositoryMock = _mocker.GetMock<IUserRepository>();
            var permissionRepositoryMock = _mocker.GetMock<IPermissionRepository>();

            userRepositoryMock.Setup(repo => repo.FindUserProfilesByEmailAsync(email))
                .ReturnsAsync(profiles);
            permissionRepositoryMock.Setup(repo => repo.FindUserPermissionsAsync(email))
                .ReturnsAsync(permissions);

            // Act
            var result = await _permissionServices.UserHasPermissionAsync(email, group, permission);

            // Assert
            Assert.False(result);
            userRepositoryMock.Verify(repo => repo.FindUserProfilesByEmailAsync(email), Times.Once);
            permissionRepositoryMock.Verify(repo => repo.FindUserPermissionsAsync(email), Times.Once);
        }

        [Fact(DisplayName = "UserHasPermissionAsync should return false when user permissions are null")]
        [Trait("UserHasPermissionAsync", "Fail")]
        public async Task UserHasPermissionAsync_ShouldReturnFalse_WhenUserPermissionsAreNull()
        {
            // Arrange
            var email = "user@example.com";
            var group = "Actions";
            var permission = "Read";
            var profiles = new List<string> { "user" };

            var userRepositoryMock = _mocker.GetMock<IUserRepository>();
            var permissionRepositoryMock = _mocker.GetMock<IPermissionRepository>();

            userRepositoryMock.Setup(repo => repo.FindUserProfilesByEmailAsync(email))
                .ReturnsAsync(profiles);
            permissionRepositoryMock.Setup(repo => repo.FindUserPermissionsAsync(email))
                .ReturnsAsync((Dictionary<string, List<string>>)null);

            // Act
            var result = await _permissionServices.UserHasPermissionAsync(email, group, permission);

            // Assert
            Assert.False(result);
            userRepositoryMock.Verify(repo => repo.FindUserProfilesByEmailAsync(email), Times.Once);
            permissionRepositoryMock.Verify(repo => repo.FindUserPermissionsAsync(email), Times.Once);
        }
    }
}
