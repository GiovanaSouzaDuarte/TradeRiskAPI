using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.Application.Rules;

public class HighRiskRule : IClassificationRule
{
    private const decimal Threshold = 1_000_000m;
    private const string PrivateSector = "Private";

    public int Priority => 3;
    public RiskCategory Category => RiskCategory.HIGHRISK;

    public bool Matches(Trade trade)
    {
        return trade.Value >= Threshold && 
               string.Equals(trade.ClientSector, PrivateSector, StringComparison.OrdinalIgnoreCase);
    }
}
