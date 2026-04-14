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

        [Fact(DisplayName = "Get connections for unknown user returns empty")]
        [Trait("GetConnections", "Empty")]
        public void GetConnections_UnknownUser_ShouldReturnEmpty()
        {
            // Act
            var result = _connectionMappingService.GetConnections("unknown@user.com");

            // Assert
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Multiple connections per user are all stored")]
        [Trait("AddConnection", "MultipleConnections")]
        public void AddConnection_MultipleConnectionsSameUser_ShouldStoreAll()
        {
            // Arrange — simula usuário com 3 abas abertas
            var userId = "user@test.com";
            var connectionIds = new[] { "conn-1", "conn-2", "conn-3" };

            // Act
            foreach (var id in connectionIds)
                _connectionMappingService.AddConnection(userId, id);

            // Assert
            var stored = _connectionMappingService.GetConnections(userId);
            Assert.Equal(3, stored.Count);
            foreach (var id in connectionIds)
                Assert.Contains(id, stored);
        }

        [Fact(DisplayName = "Remove last connection clears the user entry entirely")]
        [Trait("RemoveConnection", "ClearsEntry")]
        public void RemoveConnection_LastConnection_ShouldClearUserEntry()
        {
            // Arrange
            var userId = "user@test.com";
            _connectionMappingService.AddConnection(userId, "conn-only");

            // Act
            _connectionMappingService.RemoveConnection(userId, "conn-only");

            // Assert — GetConnections não deve lançar exceção e deve retornar vazio
            var result = _connectionMappingService.GetConnections(userId);
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Concurrent add and get connections does not throw")]
        [Trait("GetConnections", "ThreadSafety")]
        public async Task GetConnections_ConcurrentAddAndRead_ShouldNotThrow()
        {
            // Arrange
            var userId = "concurrent@user.com";
            const int totalConnections = 200;
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            // Act — threads adicionando e lendo simultaneamente
            var writers = Enumerable.Range(0, totalConnections).Select(i => Task.Run(() =>
            {
                try { _connectionMappingService.AddConnection(userId, $"conn-{i}"); }
                catch (Exception ex) { exceptions.Add(ex); }
            })).ToArray();

            var readers = Enumerable.Range(0, 50).Select(__ => Task.Run(() =>
            {
                try { _connectionMappingService.GetConnections(userId); }
                catch (Exception ex) { exceptions.Add(ex); }
            })).ToArray();

            await Task.WhenAll(writers.Concat(readers));

            // Assert — nenhuma exceção de concorrência (ex: InvalidOperationException)
            Assert.Empty(exceptions);
            var finalCount = _connectionMappingService.GetConnections(userId).Count;
            Assert.Equal(totalConnections, finalCount);
        }

        [Fact(DisplayName = "Concurrent add and remove does not throw")]
        [Trait("RemoveConnection", "ThreadSafety")]
        public async Task RemoveConnection_ConcurrentAddAndRemove_ShouldNotThrow()
        {
            // Arrange
            var userId = "concurrent-remove@user.com";
            const int totalConnections = 100;
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            for (var i = 0; i < totalConnections; i++)
                _connectionMappingService.AddConnection(userId, $"conn-{i}");

            // Act — threads removendo e lendo simultaneamente
            var removers = Enumerable.Range(0, totalConnections).Select(i => Task.Run(() =>
            {
                try { _connectionMappingService.RemoveConnection(userId, $"conn-{i}"); }
                catch (Exception ex) { exceptions.Add(ex); }
            })).ToArray();

            var readers = Enumerable.Range(0, 50).Select(__ => Task.Run(() =>
            {
                try { _connectionMappingService.GetConnections(userId); }
                catch (Exception ex) { exceptions.Add(ex); }
            })).ToArray();

            await Task.WhenAll(removers.Concat(readers));

            // Assert
            Assert.Empty(exceptions);
            Assert.Empty(_connectionMappingService.GetConnections(userId));
        }
    }
}
