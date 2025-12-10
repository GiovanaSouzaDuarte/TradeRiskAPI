using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.Application.Services;

public class TradeClassificationService : ITradeClassificationService
{
    private readonly IReadOnlyList<IClassificationRule> _rules;

    public TradeClassificationService(IEnumerable<IClassificationRule> rules)
    {
        _rules = rules.OrderBy(r => r.Priority).ToList();
    }

    public RiskCategory Classify(Trade trade)
    {
        foreach (var rule in _rules)
        {
            if (rule.Matches(trade))
            {
                return rule.Category;
            }
        }

        return RiskCategory.LOWRISK;
    }

    public IReadOnlyList<RiskCategory> ClassifyBatch(IEnumerable<Trade> trades)
    {
        return trades.Select(Classify).ToList();
    }
}
