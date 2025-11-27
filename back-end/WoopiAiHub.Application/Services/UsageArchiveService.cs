using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageArchiveService : IUsageArchiveService
    {
        private readonly IUsageDailyRepository _usageDailyRepository;
        private readonly IUsageLogRepository _usageLogRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsageArchiveService> _logger;

        public UsageArchiveService(
            IUsageDailyRepository usageDailyRepository,
            IUsageLogRepository usageLogRepository,
            IConfiguration configuration,
            ILogger<UsageArchiveService> logger)
        {
            _usageDailyRepository = usageDailyRepository ?? throw new ArgumentNullException(nameof(usageDailyRepository));
            _usageLogRepository = usageLogRepository ?? throw new ArgumentNullException(nameof(usageLogRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ArchiveOldUsageAsync(CancellationToken ct = default)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;
            var batchSize = _configuration.GetValue<int>("UsageManagement:BatchSize", 5000);
            var monthsThreshold = _configuration.GetValue<int>("UsageManagement:ArchiveMonthsThreshold", 3);
            var cutoffDate = DateTime.UtcNow.AddMonths(-monthsThreshold);
            var totalArchived = 0;

            _logger.LogInformation("[{CorrelationId}] Starting archive process for records older than {CutoffDate} with batch size {BatchSize}",
                correlationId, cutoffDate, batchSize);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var oldRecords = await _usageDailyRepository.GetOldRecordsAsync(cutoffDate, batchSize, ct);

                    if (!oldRecords.Any())
                    {
                        _logger.LogInformation("[{CorrelationId}] No more old records to archive", correlationId);
                        break;
                    }

                    // Convert to UsageLog entities
                    var logsToInsert = new List<UsageLog>();

                    foreach (var record in oldRecords)
                    {
                        // Check idempotency - don't insert if already exists
                        var exists = await _usageLogRepository.ExistsAsync(record.Id, record.Created, ct);

                        if (!exists)
                        {
                            var logEntry = new UsageLog(
                                id: record.Id,
                                created: record.Created,
                                userId: record.UserId,
                                usageTypeId: record.UsageTypeId,
                                usageCount: record.UsageCount,
                                processed: record.Processed,
                                modelEmbeddingId: record.ModelEmbeddingId
                            );

                            logsToInsert.Add(logEntry);
                        }
                    }

                    // Bulk insert into UsageLog
                    if (logsToInsert.Any())
                    {
                        await _usageLogRepository.BulkInsertAsync(logsToInsert, ct);
                        _logger.LogInformation("[{CorrelationId}] Inserted {Count} records into UsageLog",
                            correlationId, logsToInsert.Count);
                    }

                    // Delete from UsageDaily
                    var recordIds = oldRecords.Select(r => r.Id).ToList();
                    await _usageDailyRepository.BulkDeleteAsync(recordIds, ct);

                    totalArchived += oldRecords.Count;

                    _logger.LogInformation("[{CorrelationId}] Archived batch of {Count} records, total so far: {TotalArchived}",
                        correlationId, oldRecords.Count, totalArchived);
                }

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger.LogInformation("[{CorrelationId}] Archive process completed. Total archived: {TotalArchived}, Duration: {Duration}s",
                    correlationId, totalArchived, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] Error during archive process", correlationId);
                throw;
            }
        }
    }
}
