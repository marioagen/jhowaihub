using WoopiAiHub.Application.Services.Hubs;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Hubs
{
    public class ConnectionMappingServiceTests
    {
        private readonly ConnectionMappingService _connectionMappingService;
        public ConnectionMappingServiceTests()
        {
            _connectionMappingService = new ConnectionMappingService();
        }

        [Fact(DisplayName = "Get connections")]
        [Trait("GetConnections", "Success")]
        public void AddConnection_ShouldAddConnection()
        {
            // Arrange
            var userId = "user1";
            var connectionId = "connection1";

            // Act
            _connectionMappingService.AddConnection(userId, connectionId);
            //Repete, se existir não deve adicionar novamente
            _connectionMappingService.AddConnection(userId, connectionId);

            // Assert
            Assert.Contains(connectionId, _connectionMappingService.GetConnections(userId));
        }


        [Fact(DisplayName = "Remove connection")]
        [Trait("RemoveConnection", "Success")]
        public void RemoveConnection_ShouldRemoveConnection()
        {
            // Arrange
            var userId = "user1";
            var connectionId = "connection1";
            _connectionMappingService.AddConnection(userId, connectionId);

            // Act
            _connectionMappingService.RemoveConnection(userId, connectionId);

            // Assert
            Assert.DoesNotContain(connectionId, _connectionMappingService.GetConnections(userId));
        }
    }
}
