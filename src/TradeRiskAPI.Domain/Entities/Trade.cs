namespace TradeRiskAPI.Domain.Entities;

public class Trade
{
    public decimal Value { get; set; }
    public string ClientSector { get; set; } = string.Empty;
    public string? ClientId { get; set; }
}
