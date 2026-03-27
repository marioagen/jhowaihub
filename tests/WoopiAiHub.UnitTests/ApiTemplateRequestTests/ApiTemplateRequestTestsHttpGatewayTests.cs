using System.Net;
using Moq;
using WoopiAiHub.Application.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsHttpGatewayTests
    {
        private sealed class CapturingHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
            }
        }

        private static (ApiTemplateRequestTestsHttpGateway Gateway, CapturingHttpMessageHandler Handler, Mock<IHttpClientFactory> Factory) CreateGateway()
        {
            var handler = new CapturingHttpMessageHandler();
            var client = new HttpClient(handler, disposeHandler: false);
            var factory = new Mock<IHttpClientFactory>();
            factory
                .Setup(f => f.CreateClient(ApiTemplateRequestTestsHttpGateway.NamedClient))
                .Returns(client);

            var gateway = new ApiTemplateRequestTestsHttpGateway(factory.Object);
            return (gateway, handler, factory);
        }

        [Fact]
        public async Task GetAsync_UsesNamedClient_GetMethod_AndUri()
        {
            var (gateway, capturing, factoryMock) = CreateGateway();

            await gateway.GetAsync("https://api.example.com/r", null, CancellationToken.None);

            factoryMock.Verify(f => f.CreateClient(ApiTemplateRequestTestsHttpGateway.NamedClient), Times.Once);
            Assert.NotNull(capturing.LastRequest);
            Assert.Equal(HttpMethod.Get, capturing.LastRequest.Method);
            Assert.Equal("https://api.example.com/r", capturing.LastRequest.RequestUri!.ToString());
        }

        [Fact]
        public async Task PostAsync_UsesPost_AttachesBody()
        {
            var (gateway, capturing, _) = CreateGateway();
            using var body = new StringContent("""{"a":1}""", System.Text.Encoding.UTF8, "application/json");

            await gateway.PostAsync("https://api.example.com/p", body, null, CancellationToken.None);

            Assert.NotNull(capturing.LastRequest);
            Assert.Equal(HttpMethod.Post, capturing.LastRequest.Method);
            Assert.Same(body, capturing.LastRequest.Content);
        }

        [Fact]
        public async Task PutAsync_UsesPut()
        {
            var (gateway, capturing, _) = CreateGateway();

            await gateway.PutAsync("https://api.example.com/u", null, null, CancellationToken.None);

            Assert.Equal(HttpMethod.Put, capturing.LastRequest!.Method);
        }

        [Fact]
        public async Task PatchAsync_UsesPatch()
        {
            var (gateway, capturing, _) = CreateGateway();

            await gateway.PatchAsync("https://api.example.com/x", null, null, CancellationToken.None);

            Assert.Equal(HttpMethod.Patch, capturing.LastRequest!.Method);
        }

        [Fact]
        public async Task DeleteAsync_UsesDelete_WithoutContent()
        {
            var (gateway, capturing, _) = CreateGateway();

            await gateway.DeleteAsync("https://api.example.com/d", null, CancellationToken.None);

            Assert.Equal(HttpMethod.Delete, capturing.LastRequest!.Method);
            Assert.Null(capturing.LastRequest.Content);
        }

        [Fact]
        public async Task GetAsync_NullUrl_ThrowsArgumentNullException()
        {
            var (gateway, _, _) = CreateGateway();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                gateway.GetAsync(null!, null, CancellationToken.None));
        }

        [Fact]
        public async Task GetAsync_AddsHeaders_ToRequest()
        {
            var (gateway, capturing, _) = CreateGateway();
            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["X-Test"] = "v1",
                ["Authorization"] = "Bearer t"
            };

            await gateway.GetAsync("https://api.example.com/h", headers, CancellationToken.None);

            Assert.True(capturing.LastRequest!.Headers.TryGetValues("X-Test", out var xv));
            Assert.Equal("v1", Assert.Single(xv));
            Assert.True(capturing.LastRequest.Headers.TryGetValues("Authorization", out var av));
            Assert.Equal("Bearer t", Assert.Single(av));
        }

        [Fact]
        public async Task GetAsync_SkipsHostHeader()
        {
            var (gateway, capturing, _) = CreateGateway();
            var headers = new Dictionary<string, string> { ["Host"] = "evil.example.com" };

            await gateway.GetAsync("https://api.example.com/safe", headers, CancellationToken.None);

            Assert.False(capturing.LastRequest!.Headers.Contains("Host"));
        }

        [Theory]
        [InlineData("host")]
        [InlineData("HOST")]
        [InlineData("HoSt")]
        public async Task GetAsync_SkipsHostHeader_CaseInsensitive(string hostHeaderName)
        {
            var (gateway, capturing, _) = CreateGateway();
            var headers = new Dictionary<string, string> { [hostHeaderName] = "evil.example.com" };

            await gateway.GetAsync("https://api.example.com/safe", headers, CancellationToken.None);

            Assert.False(capturing.LastRequest!.Headers.Contains("Host"));
        }

        [Fact]
        public async Task GetAsync_NullHeaders_DoesNotApplyCustomHeaders()
        {
            var (gateway, capturing, _) = CreateGateway();

            await gateway.GetAsync("https://api.example.com/h", headers: null, CancellationToken.None);

            Assert.False(capturing.LastRequest!.Headers.TryGetValues("X-None", out _));
        }

        [Fact]
        public async Task GetAsync_EmptyHeaders_DoesNotApplyCustomHeaders()
        {
            var (gateway, capturing, _) = CreateGateway();
            var headers = new Dictionary<string, string>();

            await gateway.GetAsync("https://api.example.com/h", headers, CancellationToken.None);

            Assert.False(capturing.LastRequest!.Headers.TryGetValues("X-None", out _));
        }

        [Fact]
        public async Task GetAsync_SkipsEmptyHeaderName()
        {
            var (gateway, capturing, _) = CreateGateway();
            var headers = new Dictionary<string, string> { [""] = "x", ["Valid"] = "y" };

            await gateway.GetAsync("https://api.example.com/h", headers, CancellationToken.None);

            Assert.True(capturing.LastRequest!.Headers.TryGetValues("Valid", out _));
        }

        [Fact]
        public async Task PostAsync_ContentTypeHeader_UpdatesBodyContentType()
        {
            var (gateway, _, _) = CreateGateway();
            using var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = "application/xml"
            };

            await gateway.PostAsync("https://api.example.com/p", body, headers, CancellationToken.None);

            Assert.Equal("application/xml", body.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task PostAsync_WithBody_ContentTypeAndOtherHeaders_AppliesBoth()
        {
            var (gateway, capturing, _) = CreateGateway();
            using var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = "application/xml",
                ["X-After-Content-Type"] = "yes"
            };

            await gateway.PostAsync("https://api.example.com/p", body, headers, CancellationToken.None);

            Assert.Equal("application/xml", body.Headers.ContentType!.MediaType);
            Assert.True(capturing.LastRequest!.Headers.TryGetValues("X-After-Content-Type", out var v));
            Assert.Equal("yes", Assert.Single(v));
        }

        [Fact]
        public async Task PostAsync_ContentTypeHeader_LowercaseName_UpdatesBodyContentType()
        {
            var (gateway, _, _) = CreateGateway();
            using var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>
            {
                ["content-type"] = "application/xml"
            };

            await gateway.PostAsync("https://api.example.com/p", body, headers, CancellationToken.None);

            Assert.Equal("application/xml", body.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task PostAsync_InvalidContentTypeValue_LeavesBodyContentTypeUnchanged()
        {
            var (gateway, _, _) = CreateGateway();
            using var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var originalMediaType = body.Headers.ContentType!.MediaType;
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = ";;;"
            };

            await gateway.PostAsync("https://api.example.com/p", body, headers, CancellationToken.None);

            Assert.Equal(originalMediaType, body.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task PatchAsync_ContentTypeHeader_UpdatesBodyContentType()
        {
            var (gateway, _, _) = CreateGateway();
            using var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = "text/plain"
            };

            await gateway.PatchAsync("https://api.example.com/x", body, headers, CancellationToken.None);

            Assert.Equal("text/plain", body.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task PostAsync_WhenBodyNull_ContentTypeInHeaders_DoesNotThrow_AndLeavesRequestWithoutContent()
        {
            var (gateway, capturing, _) = CreateGateway();
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = "application/json"
            };

            using var response = await gateway.PostAsync("https://api.example.com/p", null, headers, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(capturing.LastRequest!.Content);
        }

        [Fact]
        public async Task GetAsync_Returns_ResponseMessage_FromClient()
        {
            var (gateway, _, _) = CreateGateway();

            using var response = await gateway.GetAsync("https://api.example.com/r", null, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("ok", await response.Content.ReadAsStringAsync());
        }
    }
}
