using System.Net;
using Moq;
using WoopiAiHub.Application.ApiTemplateRequestTests;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;

namespace WoopiAiHub.UnitTests.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsHandlerExecuteTests
    {
        private readonly Mock<IApiTemplateRequestTestsHttpGateway> _gateway = new();
        private readonly Mock<IApiTemplateRepository> _templateRepository = new();
        private readonly ApiTemplateRequestTestsHandler _handler;

        public ApiTemplateRequestTestsHandlerExecuteTests()
        {
            _handler = new ApiTemplateRequestTestsHandler(_gateway.Object, _templateRepository.Object);
        }

        [Fact(DisplayName = "ExecuteAsync should return DTO from gateway response")]
        [Trait("ExecuteAsync", "Success")]
        public async Task ExecuteAsync_ReturnsDto_FromGatewayResponse()
        {
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "GET",
                    Url = "https://api.example.com/"
                },
                Variables = new Dictionary<string, string>(),
                Tenant = "t1",
                Email = "a@b.c",
                ExecutionId = 5
            };

            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

            var result = await _handler.ExecuteAsync(request, CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("{}", result.Content);
            Assert.Equal("T", result.TemplateName);
            Assert.Equal("t1", result.Tenant);
            Assert.Equal("a@b.c", result.Email);
            Assert.Equal(5, result.ExecutionId);
        }

        [Fact(DisplayName = "ExecuteAsync should pass cancellation token to gateway")]
        [Trait("ExecuteAsync", "Success")]
        public async Task ExecuteAsync_PassesCancellationToken_ToGateway()
        {
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto { Name = "T", Method = "GET", Url = "https://x/" },
                Variables = new Dictionary<string, string>()
            };

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), token))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            await _handler.ExecuteAsync(request, token);

            _gateway.Verify(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), token), Times.Once);
        }

        [Fact(DisplayName = "ExecuteAsync should propagate when gateway throws")]
        [Trait("ExecuteAsync", "Fail")]
        public async Task ExecuteAsync_Propagates_WhenGatewayThrows()
        {
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto { Name = "T", Method = "GET", Url = "https://x/" },
                Variables = new Dictionary<string, string>()
            };

            _gateway
                .Setup(g => g.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("gateway failure"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteAsync(request, CancellationToken.None));

            Assert.Equal("gateway failure", ex.Message);
        }
    }
}
