using System.Net;
using System.Text.Json;
using Moq;
using WoopiAiHub.Application.ApiTemplateRequestTests;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;
using Xunit;

namespace WoopiAiHub.UnitTests.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsHandlerTests
    {
        private readonly Mock<IApiTemplateRequestTestsHttpGateway> _gateway = new();
        private readonly ApiTemplateRequestTestsHandler _handler;

        public ApiTemplateRequestTestsHandlerTests()
        {
            _handler = new ApiTemplateRequestTestsHandler(_gateway.Object);
        }

        [Fact]
        public async Task GetAsync_MergesQueryIntoUrl()
        {
            string? capturedUrl = null;
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, Dictionary<string, string>?, CancellationToken>((u, _, _) => capturedUrl = u)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("x") });

            var request = new ApiTemplateRequestTestsRequestDto
            {
                Url = "https://api.example.com/v1/items",
                Method = "GET",
                Query = new Dictionary<string, string> { ["a"] = "1", ["b"] = "two" }
            };

            await _handler.ExecuteAsync(request);

            Assert.NotNull(capturedUrl);
            Assert.Contains("a=1", capturedUrl, StringComparison.Ordinal);
            Assert.Contains("b=two", capturedUrl, StringComparison.Ordinal);
            Assert.StartsWith("https://api.example.com/v1/items", capturedUrl, StringComparison.Ordinal);
        }

        [Fact]
        public async Task PostAsync_SendsUnwrappedJsonBody()
        {
            var innerPayload = """{"k":42}""";
            var doubleEncoded = JsonSerializer.Serialize(innerPayload);

            HttpContent? capturedContent = null;
            _gateway
                .Setup(g => g.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, HttpContent?, Dictionary<string, string>?, CancellationToken>((_, content, _, _) => capturedContent = content)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

            var request = new ApiTemplateRequestTestsRequestDto
            {
                Url = "https://api.example.com/p",
                Method = "POST",
                Body = doubleEncoded
            };

            await _handler.ExecuteAsync(request);

            Assert.NotNull(capturedContent);
            var body = await capturedContent.ReadAsStringAsync();
            Assert.Equal(innerPayload, body);
        }

        [Fact]
        public async Task PostAsync_LeavesObjectJsonBodyUnchanged()
        {
            const string raw = """{"k":42}""";
            HttpContent? capturedContent = null;
            _gateway
                .Setup(g => g.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, HttpContent?, Dictionary<string, string>?, CancellationToken>((_, content, _, _) => capturedContent = content)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Url = "https://api.example.com/p",
                Method = "POST",
                Body = raw
            });

            Assert.NotNull(capturedContent);
            Assert.Equal(raw, await capturedContent.ReadAsStringAsync());
        }

        [Fact]
        public async Task ExecuteAsync_EchoesMetadata()
        {
            _gateway
                .Setup(g => g.DeleteAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("gone") });

            var result = await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Url = "https://api.example.com/r",
                Method = "DELETE",
                TemplateName = "T1",
                Tenant = "tn",
                Email = "e@x.com",
                ExecutionId = 99
            });

            Assert.Equal(404, result.StatusCode);
            Assert.Equal("gone", result.Content);
            Assert.Equal("T1", result.TemplateName);
            Assert.Equal("tn", result.Tenant);
            Assert.Equal("e@x.com", result.Email);
            Assert.Equal(99, result.ExecutionId);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidMethod_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Url = "https://api.example.com/r",
                    Method = "OPTIONS"
                }));
        }
    }
}
