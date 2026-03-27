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

        [Fact]
        public async Task ExecuteAsync_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.ExecuteAsync(null!));
        }

        [Fact]
        public async Task ExecuteAsync_NoDraftAndNoTemplateId_Throws()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Draft = null,
                    TemplateId = null,
                    Variables = new Dictionary<string, string>()
                }));

            Assert.Contains("Either Draft or a valid TemplateId", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_TemplateIdZero_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Draft = null,
                    TemplateId = 0,
                    Variables = new Dictionary<string, string>()
                }));
        }

        [Fact]
        public async Task ExecuteAsync_TemplateNotFound_Throws()
        {
            _templateRepository
                .Setup(r => r.FindByIdReturnModel(99))
                .ReturnsAsync((ApiTemplate?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    TemplateId = 99,
                    Draft = null,
                    Variables = new Dictionary<string, string>()
                }));

            Assert.Contains("99", ex.Message, StringComparison.Ordinal);
            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_NullVariables_DoesNotThrow()
        {
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });

            var result = await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "GET",
                    Url = "https://api.example.com/v1/"
                },
                Variables = null
            });

            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetAsync_PassesParsedHeaders()
        {
            Dictionary<string, string>? capturedHeaders = null;
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, Dictionary<string, string>?, CancellationToken>((_, h, _) => capturedHeaders = h)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });

            var headerJson = """[{"key":"Authorization","value":"Bearer t"},{"key":"X-Custom","value":"v"}]""";
            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "GET",
                    Url = "https://api.example.com/r",
                    HeaderTemplate = headerJson
                },
                Variables = new Dictionary<string, string>()
            });

            Assert.NotNull(capturedHeaders);
            Assert.Equal("Bearer t", capturedHeaders["Authorization"]);
            Assert.Equal("v", capturedHeaders["X-Custom"]);
        }

        [Fact]
        public async Task ExecuteAsync_UsesDraftName_WhenTemplateNameIsNull()
        {
            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });

            var result = await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "FromDraft",
                    Method = "GET",
                    Url = "https://api.example.com/"
                },
                Variables = new Dictionary<string, string>(),
                TemplateName = null
            });

            Assert.Equal("FromDraft", result.TemplateName);
        }

        [Fact]
        public async Task PostAsync_NullBodyTemplate_PassesNullContent()
        {
            HttpContent? capturedContent = null;
            _gateway
                .Setup(g => g.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, HttpContent?, Dictionary<string, string>?, CancellationToken>((_, c, _, _) => capturedContent = c)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "POST",
                    Url = "https://api.example.com/p",
                    BodyTemplate = null
                },
                Variables = new Dictionary<string, string>()
            });

            Assert.Null(capturedContent);
        }

        [Fact]
        public async Task PutAsync_SendsJsonBody()
        {
            const string raw = """{"a":1}""";
            HttpContent? capturedContent = null;
            _gateway
                .Setup(g => g.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, HttpContent?, Dictionary<string, string>?, CancellationToken>((_, c, _, _) => capturedContent = c)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "PUT",
                    Url = "https://api.example.com/u",
                    BodyTemplate = raw
                },
                Variables = new Dictionary<string, string>()
            });

            Assert.NotNull(capturedContent);
            Assert.Equal(raw, await capturedContent.ReadAsStringAsync());
            Assert.Equal("application/json", capturedContent.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task PatchAsync_SendsJsonBody()
        {
            const string raw = """{"p":true}""";
            HttpContent? capturedContent = null;
            _gateway
                .Setup(g => g.PatchAsync(It.IsAny<string>(), It.IsAny<HttpContent?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, HttpContent?, Dictionary<string, string>?, CancellationToken>((_, c, _, _) => capturedContent = c)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

            await _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "PATCH",
                    Url = "https://api.example.com/patch",
                    BodyTemplate = raw
                },
                Variables = new Dictionary<string, string>()
            });

            Assert.NotNull(capturedContent);
            Assert.Equal(raw, await capturedContent.ReadAsStringAsync());
        }

        [Fact]
        public async Task ExecuteAsync_InvalidQueryTemplateJson_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Draft = new ApiTemplateCreateDto
                    {
                        Name = "T",
                        Method = "GET",
                        Url = "https://api.example.com/r",
                        QueryTemplate = "not-json"
                    },
                    Variables = new Dictionary<string, string>()
                }));
        }

        [Fact]
        public async Task ExecuteAsync_InvalidHeaderTemplateJson_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Draft = new ApiTemplateCreateDto
                    {
                        Name = "T",
                        Method = "GET",
                        Url = "https://api.example.com/r",
                        HeaderTemplate = "{"
                    },
                    Variables = new Dictionary<string, string>()
                }));
        }

        [Fact]
        public async Task ExecuteAsync_NonAbsoluteUrl_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Draft = new ApiTemplateCreateDto
                    {
                        Name = "T",
                        Method = "GET",
                        Url = "/relative/path"
                    },
                    Variables = new Dictionary<string, string>()
                }));
        }

        [Fact]
        public async Task ExecuteAsync_MalformedDoubleEncodedJsonBody_Throws()
        {
            _gateway
                .Setup(g => g.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(new ApiTemplateRequestTestsRequestDto
                {
                    Draft = new ApiTemplateCreateDto
                    {
                        Name = "T",
                        Method = "POST",
                        Url = "https://api.example.com/p",
                        BodyTemplate = "\"\\uZZZZ\""
                    },
                    Variables = new Dictionary<string, string>()
                }));

            Assert.Contains("double-encoded", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(ex.InnerException);
        }
    }
}
