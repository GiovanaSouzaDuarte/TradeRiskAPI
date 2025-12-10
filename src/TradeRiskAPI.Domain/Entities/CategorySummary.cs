namespace TradeRiskAPI.Domain.Entities;

public class CategorySummary
{
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
    public string TopClient { get; set; } = string.Empty;
}
