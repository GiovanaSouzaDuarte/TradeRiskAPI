using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.Application.Rules;

public class LowRiskRule : IClassificationRule
{
    private const decimal Threshold = 1_000_000m;

    public int Priority => 1;
    public RiskCategory Category => RiskCategory.LOWRISK;

    public bool Matches(Trade trade)
    {
        return trade.Value < Threshold;
    }
}
