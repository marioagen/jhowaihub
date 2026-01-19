using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class SubscriptionPeriodRepository : ISubscriptionPeriodRepository
    {
        protected readonly ApplicationDbContext _context;

        public SubscriptionPeriodRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously creates a new subscription period in the data store.
        /// </summary>
        /// <param name="subscriptionPeriod">The <see cref="SubscriptionPeriod"/> instance to add. Must not be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see
        /// cref="SubscriptionPeriod"/> instance.</returns>
        public async Task<SubscriptionPeriod> CreateAsync(SubscriptionPeriod subscriptionPeriod)
        {
            await _context.SubscriptionPeriods.AddAsync(subscriptionPeriod);
            await _context.SaveChangesAsync();
            return subscriptionPeriod;
        }

        /// <summary>
        /// Asynchronously retrieves the most recently created subscription period that has not been processed.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the most recent <see
        /// cref="SubscriptionPeriod"/> that is unprocessed, or <see langword="null"/> if no such period exists.</returns>
        public async Task<SubscriptionPeriod?> FindLastUnprocessedAsync()
        {
            return await _context.SubscriptionPeriods
                .Where(x => !x.IsProcessed)
                .OrderByDescending(x => x.Created)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="SubscriptionPeriod"/> entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription period to retrieve. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see
        /// cref="SubscriptionPeriod"/> with the specified identifier, or <see langword="null"/> if no matching entity
        /// is found.</returns>
        public async Task<SubscriptionPeriod?> FindByIdAsync(int id)
        {
            return await _context.SubscriptionPeriods.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Updates the specified <see cref="SubscriptionPeriod"/> entity in the data store asynchronously.
        /// </summary>
        /// <param name="subscriptionPeriod">The <see cref="SubscriptionPeriod"/> instance to update. Must not be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public async Task UpdateAsync(SubscriptionPeriod subscriptionPeriod)
        {
            _context.SubscriptionPeriods.Update(subscriptionPeriod);
            await _context.SaveChangesAsync();
        }
    }
}
