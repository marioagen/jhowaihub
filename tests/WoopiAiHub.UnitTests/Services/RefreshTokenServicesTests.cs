using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Moq.AutoMock;
using System.Text;
using WoopiAiHub.Application.Services;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class RefreshTokenServicesTests
    {
        private readonly AutoMocker _mocker;
        public readonly RefreshTokenServices _refreshTokenServices;

        public RefreshTokenServicesTests()
        {
            _mocker = new AutoMocker();
            _refreshTokenServices = _mocker.CreateInstance<RefreshTokenServices>();
        }

        [Fact(DisplayName = "SaveAsync stores email with correct key and options")]
        [Trait("SaveAsync", "Success")]
        public async Task SaveAsync_ShouldCallSetAsync_WithExpectedArguments()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();
            var email = "user@example.com";
            var refreshToken = "token123";
            var expectedKey = $"refresh_token:{refreshToken}";

            DistributedCacheEntryOptions? capturedOptions = null;
            byte[]? capturedValue = null;

            cacheMock
                .Setup(c => c.SetAsync(
                    It.Is<string>(k => k == expectedKey),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((k, v, o, ct) =>
                {
                    capturedOptions = o;
                    capturedValue = v;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _refreshTokenServices.SaveAsync(email, refreshToken);

            // Assert
            cacheMock.Verify(c => c.SetAsync(
                It.Is<string>(k => k == expectedKey),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.NotNull(capturedOptions);
            Assert.True(capturedOptions!.AbsoluteExpirationRelativeToNow.HasValue);
            Assert.Equal(TimeSpan.FromDays(7), capturedOptions.AbsoluteExpirationRelativeToNow.Value);

            Assert.NotNull(capturedValue);
            var decoded = Encoding.UTF8.GetString(capturedValue!);
            Assert.Equal(email, decoded);
        }

        [Fact(DisplayName = "FindUserByRefreshTokenAsync returns stored email")]
        [Trait("FindUserByRefreshTokenAsync", "Success")]
        public async Task FindUserByRefreshTokenAsync_ShouldReturnEmail_WhenExists()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();
            var refreshToken = "tokenABC";
            var expectedKey = $"refresh_token:{refreshToken}";
            var expectedEmail = "foo@bar.com";
            var encoded = Encoding.UTF8.GetBytes(expectedEmail);

            cacheMock
                .Setup(c => c.GetAsync(
                    It.Is<string>(k => k == expectedKey),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(encoded);

            // Act
            var result = await _refreshTokenServices.FindUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.Equal(expectedEmail, result);
        }

        [Fact(DisplayName = "FindUserByRefreshTokenAsync returns null when missing")]
        [Trait("FindUserByRefreshTokenAsync", "Fail")]
        public async Task FindUserByRefreshTokenAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();
            var refreshToken = "nonexistent";
            var expectedKey = $"refresh_token:{refreshToken}";

            cacheMock
                .Setup(c => c.GetAsync(
                    It.Is<string>(k => k == expectedKey),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            var result = await _refreshTokenServices.FindUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "RevokeAsync removes the key")]
        [Trait("RevokeAsync", "Success")]
        public async Task RevokeAsync_ShouldCallRemoveAsync_WithExpectedKey()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();
            var refreshToken = "toRevoke";
            var expectedKey = $"refresh_token:{refreshToken}";

            cacheMock
                .Setup(c => c.RemoveAsync(
                    It.Is<string>(k => k == expectedKey),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _refreshTokenServices.RevokeAsync(refreshToken);

            // Assert
            cacheMock.Verify(c => c.RemoveAsync(
                It.Is<string>(k => k == expectedKey),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
