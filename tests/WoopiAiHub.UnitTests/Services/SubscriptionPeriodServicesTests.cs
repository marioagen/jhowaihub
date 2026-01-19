using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class SubscriptionPeriodServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly SubscriptionPeriodServices _subscriptionPeriodServices;

        public SubscriptionPeriodServicesTests()
        {
            _mocker = new AutoMocker();
            _subscriptionPeriodServices = _mocker.CreateInstance<SubscriptionPeriodServices>();
        }

        [Fact(DisplayName = "CreateAsync creates and returns a subscription period")]
        [Trait("CreateAsync", "Success")]
        public async Task CreateAsync_CreatesAndReturnsSubscriptionPeriod()
        {
            // Arrange
            var periodStart = DateTime.Now;
            var periodEnd = DateTime.Now.AddDays(30);
            var isProcessed = false;
            var subscriptionPeriod = new SubscriptionPeriod(periodStart, periodEnd, isProcessed);

            var mockRepo = _mocker.GetMock<ISubscriptionPeriodRepository>();
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<SubscriptionPeriod>()))
                .ReturnsAsync(subscriptionPeriod);

            // Act
            var result = await _subscriptionPeriodServices.CreateAsync(periodStart, periodEnd, isProcessed);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(periodStart, result.PeriodStart);
            Assert.Equal(periodEnd, result.PeriodEnd);
            Assert.Equal(isProcessed, result.IsProcessed);
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<SubscriptionPeriod>()), Times.Once);
        }

        [Fact(DisplayName = "GetLastUnprocessedAsync returns the last unprocessed period")]
        [Trait("GetLastUnprocessedAsync", "Success")]
        public async Task GetLastUnprocessedAsync_ReturnsLastUnprocessedPeriod()
        {
            // Arrange
            var expectedPeriod = new SubscriptionPeriod(DateTime.Now, DateTime.Now.AddDays(30), false);
            var mockRepo = _mocker.GetMock<ISubscriptionPeriodRepository>();
            mockRepo.Setup(r => r.GetLastUnprocessedAsync())
                .ReturnsAsync(expectedPeriod);

            // Act
            var result = await _subscriptionPeriodServices.GetLastUnprocessedAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPeriod, result);
            mockRepo.Verify(r => r.GetLastUnprocessedAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateToProcessedAsync updates the period to processed when it exists")]
        [Trait("UpdateToProcessedAsync", "Success")]
        public async Task UpdateToProcessedAsync_UpdatesPeriod_WhenPeriodExists()
        {
            // Arrange
            var id = 1;
            var period = new SubscriptionPeriod(DateTime.Now, DateTime.Now.AddDays(30), false);
            var mockRepo = _mocker.GetMock<ISubscriptionPeriodRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(period);

            // Act
            await _subscriptionPeriodServices.UpdateToProcessedAsync(id);

            // Assert
            Assert.True(period.IsProcessed);
            mockRepo.Verify(r => r.UpdateAsync(period), Times.Once);
        }

        [Fact(DisplayName = "UpdateToProcessedAsync does nothing when period does not exist")]
        [Trait("UpdateToProcessedAsync", "Fail")]
        public async Task UpdateToProcessedAsync_DoesNothing_WhenPeriodDoesNotExist()
        {
            // Arrange
            var id = 1;
            var mockRepo = _mocker.GetMock<ISubscriptionPeriodRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((SubscriptionPeriod?)null);

            // Act
            await _subscriptionPeriodServices.UpdateToProcessedAsync(id);

            // Assert
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPeriod>()), Times.Never);
        }
    }
}
