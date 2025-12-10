using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;

namespace TradeRiskAPI.Domain.Interfaces;

public interface IClassificationRule
{
    int Priority { get; }
    bool Matches(Trade trade);
    RiskCategory Category { get; }
}
