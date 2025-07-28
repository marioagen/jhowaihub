using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;

public class PermissionServicesTests
{
    [Fact(DisplayName = "Test FindAll and returns valid permissions")]
    [Trait("FindAll", "Success")]
    public void FindAll_ReturnsPermissions_WhenPermissionsExist()
    {
        // Arrange
        var mockRepo = new Mock<IPermissionRepository>();
        mockRepo.Setup(r => r.FindAll()).Returns(new List<PermissionDto> { new PermissionDto { Id = 1, Name = "Test" } });
        var service = new PermissionServices(mockRepo.Object);

        // Act
        var result = service.FindAll();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test", result.First().Name);
    }

    [Fact(DisplayName = "Test FindAll and returns empty when no permissions exists")]
    [Trait("FindAll", "Empty")]
    public void FindAll_ReturnsEmpty_WhenNoPermissionsExist()
    {
        var mockRepo = new Mock<IPermissionRepository>();
        mockRepo.Setup(r => r.FindAll()).Returns(new List<PermissionDto>());
        var service = new PermissionServices(mockRepo.Object);

        var result = service.FindAll();

        Assert.Empty(result);
    }

    [Fact(DisplayName = "Test FindAll and throws exception when there is a database error")]
    [Trait("FindAll", "Fail")]
    public void FindAll_ThrowsException_WhenRepositoryThrows()
    {
        var mockRepo = new Mock<IPermissionRepository>();
        mockRepo.Setup(r => r.FindAll()).Throws(new Exception("DB error"));
        var service = new PermissionServices(mockRepo.Object);

        Assert.Throws<Exception>(() => service.FindAll());
    }
}
