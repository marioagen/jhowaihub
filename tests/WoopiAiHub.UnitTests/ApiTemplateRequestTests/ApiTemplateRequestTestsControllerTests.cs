using Microsoft.AspNetCore.Mvc;
using Moq;
using WoopiAiHub.Api.Controllers;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;
using Xunit;

namespace WoopiAiHub.UnitTests.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsControllerTests
    {
        private readonly Mock<IApiTemplateRequestTestsHandler> _handler = new();
        private readonly ApiTemplateRequestTestsController _controller;

        public ApiTemplateRequestTestsControllerTests()
        {
            _controller = new ApiTemplateRequestTestsController(_handler.Object);
        }

        [Fact]
        public async Task Execute_ReturnsOk_WithHandlerResponse()
        {
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto
                {
                    Name = "T",
                    Method = "GET",
                    Url = "https://api.example.com/"
                },
                Variables = new Dictionary<string, string>()
            };

            var expected = new ApiTemplateRequestTestsResponseDto
            {
                StatusCode = 200,
                Content = "{}",
                TemplateName = "T",
                Tenant = "t1",
                Email = "a@b.c",
                ExecutionId = 5
            };

            _handler
                .Setup(h => h.ExecuteAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Execute(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ApiTemplateRequestTestsResponseDto>(ok.Value);
            Assert.Same(expected, dto);
        }

        [Fact]
        public async Task Execute_PassesCancellationToken_ToHandler()
        {
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto { Name = "T", Method = "GET", Url = "https://x/" },
                Variables = new Dictionary<string, string>()
            };

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _handler
                .Setup(h => h.ExecuteAsync(It.IsAny<ApiTemplateRequestTestsRequestDto>(), token))
                .ReturnsAsync(new ApiTemplateRequestTestsResponseDto { StatusCode = 204 });

            await _controller.Execute(request, token);

            _handler.Verify(h => h.ExecuteAsync(request, token), Times.Once);
        }

        [Fact]
        public async Task Execute_Rethrows_WhenHandlerThrows()
        {
            var request = new ApiTemplateRequestTestsRequestDto
            {
                Draft = new ApiTemplateCreateDto { Name = "T", Method = "GET", Url = "https://x/" },
                Variables = new Dictionary<string, string>()
            };

            _handler
                .Setup(h => h.ExecuteAsync(It.IsAny<ApiTemplateRequestTestsRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("invalid method"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.Execute(request, CancellationToken.None));

            Assert.Equal("invalid method", ex.Message);
        }
    }
}
