namespace TradeRiskAPI.Application.DTOs;

public record TradeDto(decimal Value, string ClientSector);

public record TradeWithClientDto(decimal Value, string ClientSector, string ClientId);

public record ClassifyRequest(List<TradeDto> Trades);

public record ClassifyResponse(List<string> Categories);

public record AnalyzeRequest(List<TradeWithClientDto> Trades);

public record CategorySummaryDto(int Count, decimal TotalValue, string TopClient);

public record AnalyzeResponse(
    List<string> Categories,
    Dictionary<string, CategorySummaryDto> Summary,
    long ProcessingTimeMs
);
