using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class SubscriptionPeriodServices : ISubscriptionPeriodServices
    {
        private readonly ISubscriptionPeriodRepository _subscriptionPeriodRepository;

        public SubscriptionPeriodServices(ISubscriptionPeriodRepository subscriptionPeriodRepository)
        {
            _subscriptionPeriodRepository = subscriptionPeriodRepository;
        }

        /// <summary>
        /// Asynchronously creates a new <see cref="SubscriptionPeriod"/> with the specified start and end dates and
        /// processed status.
        /// </summary>
        /// <param name="periodStart">The start date and time of the subscription period.</param>
        /// <param name="periodEnd">The end date and time of the subscription period. Must be greater than or equal to <paramref
        /// name="periodStart"/>.</param>
        /// <param name="isProcessed"><see langword="true"/> to mark the subscription period as processed; otherwise, <see langword="false"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see
        /// cref="SubscriptionPeriod"/> instance.</returns>
        public async Task<SubscriptionPeriod> CreateAsync(DateTime periodStart, DateTime periodEnd, bool isProcessed)
        {
            var subscriptionPeriod = new SubscriptionPeriod(periodStart, periodEnd, isProcessed);
            return await _subscriptionPeriodRepository.CreateAsync(subscriptionPeriod);
        }

        /// <summary>
        /// Asynchronously retrieves the most recent unprocessed subscription period, if one exists.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest unprocessed <see
        /// cref="SubscriptionPeriod"/>, or <see langword="null"/> if all periods have been processed.</returns>
        public async Task<SubscriptionPeriod?> GetLastUnprocessedAsync()
        {
            return await _subscriptionPeriodRepository.GetLastUnprocessedAsync();
        }

        /// <summary>
        /// Updates the subscription period with the specified identifier to a processed state asynchronously.
        /// </summary>
        /// <remarks>If no subscription period with the specified <paramref name="id"/> exists, the method
        /// completes without making any changes.</remarks>
        /// <param name="id">The unique identifier of the subscription period to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateToProcessedAsync(int id)
        {
            var period = await _subscriptionPeriodRepository.GetByIdAsync(id);
            if (period != null)
            {
                period.SetProcessed();
                await _subscriptionPeriodRepository.UpdateAsync(period);
            }
        }
    }
}
