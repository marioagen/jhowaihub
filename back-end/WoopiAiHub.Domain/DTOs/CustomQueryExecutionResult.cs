namespace WoopiAiHub.Domain.DTOs;

public record CustomQueryExecutionResult(string ResponseText, IReadOnlyList<QueryUsageDto> Usage);
