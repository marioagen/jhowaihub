namespace WoopiAiHub.Domain.DTOs;

public sealed record CustomQueryExecutionResult(string ResponseText, IReadOnlyList<QueryUsageDto> Usage);
