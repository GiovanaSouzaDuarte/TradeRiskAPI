using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;

namespace TradeRiskAPI.Domain.Interfaces;

public interface ITradeAnalysisService
{
    (IReadOnlyList<RiskCategory> Categories, Dictionary<RiskCategory, CategorySummary> Summary, long ProcessingTimeMs) Analyze(IEnumerable<Trade> trades);
}
