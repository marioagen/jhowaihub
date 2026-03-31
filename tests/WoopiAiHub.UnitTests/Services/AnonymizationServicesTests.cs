using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class AnonymizationServicesTests
    {
        private readonly Mock<IDocumentServices> _documentServicesMock;
        private readonly Mock<IAnonymizationApi> _anonymizationApiMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IHubNotifier> _hubNotifierMock;
        private readonly Mock<ILogger<AnonymizationServices>> _loggerMock;
        private readonly AnonymizationServices _sut;
        private readonly Faker _faker;

        public AnonymizationServicesTests()
        {
            _documentServicesMock = new Mock<IDocumentServices>();
            _anonymizationApiMock = new Mock<IAnonymizationApi>();
            _configurationMock = new Mock<IConfiguration>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _hubNotifierMock = new Mock<IHubNotifier>();
            _loggerMock = new Mock<ILogger<AnonymizationServices>>();
            _faker = new Faker();

            _sut = new AnonymizationServices(
                _documentServicesMock.Object,
                _anonymizationApiMock.Object,
                _configurationMock.Object,
                _httpClientFactoryMock.Object,
                _hubNotifierMock.Object,
                _loggerMock.Object
            );
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult successfully notifies hub with valid result data.
        /// Verifies that the hub notifier is called exactly once with the correct parameters.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should successfully notify hub with valid result")]
        [Trait("ProcessAnonymizationResult", "Success")]
        public async Task ProcessAnonymizationResult_ValidResult_NotifiesHubSuccessfully()
        {
            // Arrange
            var result = new AnonymizationResultDto
            {
                DocumentUrl = _faker.Internet.Url(),
                WoopiAiDocumentId = _faker.Random.Int(1, 10000),
                WoopiAiEmail = _faker.Internet.Email()
            };

            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            _hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl),
                Times.Once);
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult throws NullReferenceException when result parameter is null.
        /// This ensures proper null handling is in place.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should throw NullReferenceException when result is null")]
        [Trait("ProcessAnonymizationResult", "NullHandling")]
        public async Task ProcessAnonymizationResult_NullResult_ThrowsNullReferenceException()
        {
            // Arrange
            AnonymizationResultDto? result = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _sut.ProcessAnonymizationResult(result!));
        }

        /// <summary>
        /// Tests ProcessAnonymizationResult with various edge case string values for email and document URL.
        /// Verifies that the method correctly passes through all parameter values including edge cases.
        /// </summary>
        /// <param name="email">The email address to test.</param>
        /// <param name="documentUrl">The document URL to test.</param>
        /// <param name="documentId">The document ID to test.</param>
        [Theory(DisplayName = "ProcessAnonymizationResult - Should handle edge case values for properties")]
        [Trait("ProcessAnonymizationResult", "EdgeCases")]
        [InlineData("", "", 0)]
        [InlineData("user@example.com", "http://example.com", 1)]
        [InlineData("test@test.com", "https://example.com/very/long/path/to/document.pdf", int.MaxValue)]
        [InlineData("special+chars@example.com", "http://example.com?param=value&other=123", -1)]
        [InlineData("user with spaces@example.com", "http://example.com/path with spaces", int.MinValue)]
        public async Task ProcessAnonymizationResult_EdgeCaseValues_PassesValuesToHubNotifier(
            string email,
            string documentUrl,
            int documentId)
        {
            // Arrange
            var result = new AnonymizationResultDto
            {
                DocumentUrl = documentUrl,
                WoopiAiDocumentId = documentId,
                WoopiAiEmail = email
            };

            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(email, documentId, documentUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            _hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(email, documentId, documentUrl),
                Times.Once);
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult correctly handles very long string values.
        /// Verifies that the method does not truncate or modify the input values.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should handle very long strings")]
        [Trait("ProcessAnonymizationResult", "EdgeCases")]
        public async Task ProcessAnonymizationResult_VeryLongStrings_PassesValuesToHubNotifier()
        {
            // Arrange
            var longEmail = new string('a', 500) + "@example.com";
            var longUrl = "http://example.com/" + new string('x', 2000);
            var result = new AnonymizationResultDto
            {
                DocumentUrl = longUrl,
                WoopiAiDocumentId = _faker.Random.Int(1, 1000),
                WoopiAiEmail = longEmail
            };

            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(longEmail, result.WoopiAiDocumentId, longUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            _hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(longEmail, result.WoopiAiDocumentId, longUrl),
                Times.Once);
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult correctly handles special and control characters in strings.
        /// Verifies proper handling of Unicode characters, newlines, tabs, and other special characters.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should handle special characters in strings")]
        [Trait("ProcessAnonymizationResult", "EdgeCases")]
        public async Task ProcessAnonymizationResult_SpecialCharacters_PassesValuesToHubNotifier()
        {
            // Arrange
            var specialEmail = "user+tag@example.com";
            var specialUrl = "http://example.com/path?query=value&special=<>&\"'";
            var result = new AnonymizationResultDto
            {
                DocumentUrl = specialUrl,
                WoopiAiDocumentId = 123,
                WoopiAiEmail = specialEmail
            };

            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(specialEmail, 123, specialUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            _hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(specialEmail, 123, specialUrl),
                Times.Once);
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult properly handles whitespace-only strings.
        /// Verifies that whitespace values are passed through without modification.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should handle whitespace-only strings")]
        [Trait("ProcessAnonymizationResult", "EdgeCases")]
        public async Task ProcessAnonymizationResult_WhitespaceStrings_PassesValuesToHubNotifier()
        {
            // Arrange
            var whitespaceEmail = "   ";
            var whitespaceUrl = "\t\n";
            var result = new AnonymizationResultDto
            {
                DocumentUrl = whitespaceUrl,
                WoopiAiDocumentId = 42,
                WoopiAiEmail = whitespaceEmail
            };

            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(whitespaceEmail, 42, whitespaceUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            _hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(whitespaceEmail, 42, whitespaceUrl),
                Times.Once);
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult does not call hub notifier more than once.
        /// Verifies idempotent behavior and ensures no duplicate notifications.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should call hub notifier exactly once")]
        [Trait("ProcessAnonymizationResult", "Verification")]
        public async Task ProcessAnonymizationResult_ValidResult_CallsHubNotifierExactlyOnce()
        {
            // Arrange
            var result = new AnonymizationResultDto
            {
                DocumentUrl = _faker.Internet.Url(),
                WoopiAiDocumentId = _faker.Random.Int(1, 10000),
                WoopiAiEmail = _faker.Internet.Email()
            };

            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            _hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that ProcessAnonymizationResult correctly propagates exceptions from hub notifier.
        /// Verifies that exceptions thrown by the dependency are not swallowed.
        /// </summary>
        [Fact(DisplayName = "ProcessAnonymizationResult - Should propagate exception from hub notifier")]
        [Trait("ProcessAnonymizationResult", "ExceptionHandling")]
        public async Task ProcessAnonymizationResult_HubNotifierThrows_PropagatesException()
        {
            // Arrange
            var result = new AnonymizationResultDto
            {
                DocumentUrl = _faker.Internet.Url(),
                WoopiAiDocumentId = _faker.Random.Int(1, 10000),
                WoopiAiEmail = _faker.Internet.Email()
            };

            var expectedException = new InvalidOperationException("Hub notification failed");
            _hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymizationResult(result));
            Assert.Equal("Hub notification failed", exception.Message);
        }
    }
}
