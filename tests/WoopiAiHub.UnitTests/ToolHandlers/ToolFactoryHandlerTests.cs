using Moq;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.ToolHandlers
{
    [Collection(nameof(ToolHandlerCollection))]
    public class ToolFactoryHandlerTests
    {
        private readonly Mock<IToolHandler> _mockHandler1;
        private readonly Mock<IToolHandler> _mockHandler2;
        private readonly ToolFactoryHandler _toolFactoryHandler;

        public ToolFactoryHandlerTests()
        {
            _mockHandler1 = new Mock<IToolHandler>();
            _mockHandler1.Setup(h => h.Type).Returns("Type1");

            _mockHandler2 = new Mock<IToolHandler>();
            _mockHandler2.Setup(h => h.Type).Returns("Type2");

            var handlers = new List<IToolHandler> { _mockHandler1.Object, _mockHandler2.Object };
            _toolFactoryHandler = new ToolFactoryHandler(handlers);
        }

        [Fact(DisplayName = "GetHandler returns handler whenn valid Type")]
        [Trait("GetHandler", "Success")]
        public void GetHandler_ValidType_ReturnsCorrectHandler()
        {
            // Act
            var handler = _toolFactoryHandler.GetHandler("Type1");

            // Assert
            Assert.Equal(_mockHandler1.Object, handler);
        }

        [Fact(DisplayName = "GetHandler throws  ArgumentException when invalid Type")]
        [Trait("GetHandler", "Fail")]
        public void GetHandler_InvalidType_ThrowsArgumentException()
        {
            //Arrange 
            string typeName = "InvalidType";
            var toolType = ToolTypeFixture.FindValidToolTypeWithName(typeName);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _toolFactoryHandler.GetHandler(typeName));
            Assert.Equal($"Handler for type '{typeName}' not found.", exception.Message);
        }
    }
}
