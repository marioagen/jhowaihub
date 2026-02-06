using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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
        private const string _email = "user@example.com";
        private const string _refreshToken = "token123";
        private const string _expectedKey = $"refresh_token:{_refreshToken}";

        public RefreshTokenServicesTests()
        {
            _mocker = new AutoMocker();
            var configData = new Dictionary<string, string?> { ["JWT:RefreshTokenExpirationDays"] = "7" };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configData!).Build();
            _mocker.Use<IConfiguration>(config);
            _refreshTokenServices = _mocker.CreateInstance<RefreshTokenServices>();
        }

        [Fact(DisplayName = "SaveAsync stores email with correct key and options")]
        [Trait("SaveAsync", "Success")]
        public async Task SaveAsync_ShouldCallSetAsync_WithExpectedArguments()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();

            DistributedCacheEntryOptions? capturedOptions = null;
            byte[]? capturedValue = null;

            cacheMock
                .Setup(c => c.SetAsync(
                    It.Is<string>(k => k == _expectedKey),
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
            await _refreshTokenServices.SaveAsync(_email, _refreshToken);

            // Assert
            cacheMock.Verify(c => c.SetAsync(
                It.Is<string>(k => k == _expectedKey),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.NotNull(capturedOptions);
            Assert.True(capturedOptions!.AbsoluteExpirationRelativeToNow.HasValue);
            Assert.Equal(TimeSpan.FromDays(7), capturedOptions.AbsoluteExpirationRelativeToNow.Value);

            Assert.NotNull(capturedValue);
            var decoded = Encoding.UTF8.GetString(capturedValue!);
            Assert.Equal(_email, decoded);
        }

        [Fact(DisplayName = "FindUserByRefreshTokenAsync returns stored email")]
        [Trait("FindUserByRefreshTokenAsync", "Success")]
        public async Task FindUserByRefreshTokenAsync_ShouldReturnEmail_WhenExists()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();
            var encoded = Encoding.UTF8.GetBytes(_email);

            cacheMock
                .Setup(c => c.GetAsync(
                    It.Is<string>(k => k == _expectedKey),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(encoded);

            // Act
            var result = await _refreshTokenServices.FindUserByRefreshTokenAsync(_refreshToken);

            // Assert
            Assert.Equal(_email, result);
        }

        [Fact(DisplayName = "FindUserByRefreshTokenAsync returns null when missing")]
        [Trait("FindUserByRefreshTokenAsync", "Fail")]
        public async Task FindUserByRefreshTokenAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();

            cacheMock
                .Setup(c => c.GetAsync(
                    It.Is<string>(k => k == _expectedKey),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            var result = await _refreshTokenServices.FindUserByRefreshTokenAsync(_refreshToken);

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "RevokeAsync removes the key")]
        [Trait("RevokeAsync", "Success")]
        public async Task RevokeAsync_ShouldCallRemoveAsync_WithExpectedKey()
        {
            // Arrange
            var cacheMock = _mocker.GetMock<IDistributedCache>();

            cacheMock
                .Setup(c => c.RemoveAsync(
                    It.Is<string>(k => k == _expectedKey),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _refreshTokenServices.RevokeAsync(_refreshToken);

            // Assert
            cacheMock.Verify(c => c.RemoveAsync(
                It.Is<string>(k => k == _expectedKey),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
