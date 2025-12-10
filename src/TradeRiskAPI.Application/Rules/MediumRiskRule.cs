using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.Application.Rules;

public class MediumRiskRule : IClassificationRule
{
    private const decimal Threshold = 1_000_000m;
    private const string PublicSector = "Public";

    public int Priority => 2;
    public RiskCategory Category => RiskCategory.MEDIUMRISK;

    public bool Matches(Trade trade)
    {
        return trade.Value >= Threshold && 
               string.Equals(trade.ClientSector, PublicSector, StringComparison.OrdinalIgnoreCase);
    }
}
