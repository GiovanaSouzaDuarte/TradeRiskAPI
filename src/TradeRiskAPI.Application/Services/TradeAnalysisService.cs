using System.Diagnostics;
using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.Application.Services;

public class TradeAnalysisService : ITradeAnalysisService
{
    private readonly ITradeClassificationService _classificationService;

    public TradeAnalysisService(ITradeClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public (IReadOnlyList<RiskCategory> Categories, Dictionary<RiskCategory, CategorySummary> Summary, long ProcessingTimeMs) Analyze(IEnumerable<Trade> trades)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var tradeList = trades.ToList();
        var categories = new List<RiskCategory>(tradeList.Count);
        
        var summaryData = new Dictionary<RiskCategory, (int Count, decimal TotalValue, Dictionary<string, decimal> ClientValues)>
        {
            [RiskCategory.LOWRISK] = (0, 0m, new Dictionary<string, decimal>()),
            [RiskCategory.MEDIUMRISK] = (0, 0m, new Dictionary<string, decimal>()),
            [RiskCategory.HIGHRISK] = (0, 0m, new Dictionary<string, decimal>())
        };

        foreach (var trade in tradeList)
        {
            var category = _classificationService.Classify(trade);
            categories.Add(category);

            var (count, totalValue, clientValues) = summaryData[category];
            summaryData[category] = (count + 1, totalValue + trade.Value, clientValues);

            if (!string.IsNullOrEmpty(trade.ClientId))
            {
                if (!clientValues.ContainsKey(trade.ClientId))
                {
                    clientValues[trade.ClientId] = 0;
                }
                clientValues[trade.ClientId] += trade.Value;
            }
        }

        var summary = new Dictionary<RiskCategory, CategorySummary>();
        
        foreach (var kvp in summaryData)
        {
            var topClient = kvp.Value.ClientValues.Count > 0
                ? kvp.Value.ClientValues.OrderByDescending(c => c.Value).First().Key
                : string.Empty;

            summary[kvp.Key] = new CategorySummary
            {
                Count = kvp.Value.Count,
                TotalValue = kvp.Value.TotalValue,
                TopClient = topClient
            };
        }

        stopwatch.Stop();
        
        return (categories, summary, stopwatch.ElapsedMilliseconds);
    }
}
