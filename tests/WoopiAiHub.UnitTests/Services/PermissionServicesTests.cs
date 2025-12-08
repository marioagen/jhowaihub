using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;

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
            var mockRepo = _mocker.GetMock<IPermissionRepository > ();
            mockRepo.Setup(r => r.FindAll()).Throws(new Exception("DB error"));

            Assert.Throws<Exception>(() => _permissionServices.FindAll());
        }
    }
}
