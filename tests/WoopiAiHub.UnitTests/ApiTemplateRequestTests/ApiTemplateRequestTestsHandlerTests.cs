using System.Net;
using System.Text.Json;
using Moq;
using WoopiAiHub.Application.ApiTemplateRequestTests;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsHandlerTests
    {
        private readonly Mock<IApiTemplateRequestTestsHttpGateway> _gateway = new();
        private readonly Mock<IApiTemplateRepository> _templateRepository = new();
        private readonly ApiTemplateRequestTestsHandler _handler;

        public ApiTemplateRequestTestsHandlerTests()
        {
            _handler = new ApiTemplateRequestTestsHandler(_gateway.Object, _templateRepository.Object);
        }

        [Fact]
        public async Task GetAsync_MergesQueryTemplateIntoUrl()
        {
            string? capturedUrl = null;
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, Dictionary<string, string>?, CancellationToken>((u, _, _) => capturedUrl = u)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("x") });

            var queryJson = """[{"key":"a","value":"1"},{"key":"b","value":"two"}]""";
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "GET",
                    Url = "https://api.example.com/v1/items",
                    QueryTemplate = queryJson
                },
                Variables = new Dictionary<string, string>()
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
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "POST",
                    Url = "https://api.example.com/p",
                    BodyTemplate = doubleEncoded
                },
                Variables = new Dictionary<string, string>()
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
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "POST",
                    Url = "https://api.example.com/p",
                    BodyTemplate = raw
                },
                Variables = new Dictionary<string, string>()
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
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T1",
                    Method = "DELETE",
                    Url = "https://api.example.com/r"
                },
                Variables = new Dictionary<string, string>(),
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
                    Draft = new ApiTemplateCreateDto
                    {
                        Name = "T",
                        Method = "OPTIONS",
                        Url = "https://api.example.com/r"
                    },
                    Variables = new Dictionary<string, string>()
                }));
        }

        [Fact]
        public async Task ExecuteAsync_SubstitutesVariablesInUrl()
        {
            string? capturedUrl = null;
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, Dictionary<string, string>?, CancellationToken>((u, _, _) => capturedUrl = u)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });

            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "GET",
                    Url = "https://api.example.com/v1/{{id}}"
                },
                Variables = new Dictionary<string, string> { ["id"] = "42" }
            });

            Assert.Equal("https://api.example.com/v1/42", capturedUrl);
        }

        [Fact]
        public async Task ExecuteAsync_LoadsFromTemplateId_WhenDraftIsNull()
        {
            var model = new ApiTemplate("Db", "GET", "https://api.example.com/from-db", null, null, null);
            _templateRepository
                .Setup(r => r.FindByIdReturnModel(7))
                .ReturnsAsync(model);

            string? capturedUrl = null;
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, Dictionary<string, string>?, CancellationToken>((u, _, _) => capturedUrl = u)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });

            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                TemplateId = 7,
                Draft = null,
                Variables = new Dictionary<string, string>()
            });

            Assert.Equal("https://api.example.com/from-db", capturedUrl);
        }
    }
}
