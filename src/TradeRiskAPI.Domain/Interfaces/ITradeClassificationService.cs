using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;

namespace TradeRiskAPI.Domain.Interfaces;

public interface ITradeClassificationService
{
    RiskCategory Classify(Trade trade);
    IReadOnlyList<RiskCategory> ClassifyBatch(IEnumerable<Trade> trades);
}
