using FluentAssertions;
using TradeRiskAPI.Application.Rules;
using TradeRiskAPI.Application.Services;
using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.UnitTests.Services;

public class TradeClassificationServiceTests
{
    private readonly ITradeClassificationService _service;

    public TradeClassificationServiceTests()
    {
        var rules = new IClassificationRule[]
        {
            new LowRiskRule(),
            new MediumRiskRule(),
            new HighRiskRule()
        };
        _service = new TradeClassificationService(rules);
    }

    [Fact]
    public void Classify_ShouldReturnLowRisk_WhenValueBelowThreshold()
    {
        var trade = new Trade { Value = 400_000, ClientSector = "Public" };

        var result = _service.Classify(trade);

        result.Should().Be(RiskCategory.LOWRISK);
    }

    [Fact]
    public void Classify_ShouldReturnMediumRisk_WhenValueAboveThresholdAndPublicSector()
    {
        var trade = new Trade { Value = 3_000_000, ClientSector = "Public" };

        var result = _service.Classify(trade);

        result.Should().Be(RiskCategory.MEDIUMRISK);
    }

    [Fact]
    public void Classify_ShouldReturnHighRisk_WhenValueAboveThresholdAndPrivateSector()
    {
        var trade = new Trade { Value = 2_000_000, ClientSector = "Private" };

        var result = _service.Classify(trade);

        result.Should().Be(RiskCategory.HIGHRISK);
    }

    [Fact]
    public void ClassifyBatch_ShouldReturnCorrectCategories_ForMultipleTrades()
    {
        var trades = new[]
        {
            new Trade { Value = 2_000_000, ClientSector = "Private" },
            new Trade { Value = 400_000, ClientSector = "Public" },
            new Trade { Value = 500_000, ClientSector = "Public" },
            new Trade { Value = 3_000_000, ClientSector = "Public" }
        };

        var result = _service.ClassifyBatch(trades);

        result.Should().HaveCount(4);
        result[0].Should().Be(RiskCategory.HIGHRISK);
        result[1].Should().Be(RiskCategory.LOWRISK);
        result[2].Should().Be(RiskCategory.LOWRISK);
        result[3].Should().Be(RiskCategory.MEDIUMRISK);
    }

    [Fact]
    public void ClassifyBatch_ShouldReturnEmptyList_WhenNoTrades()
    {
        var trades = Array.Empty<Trade>();

        var result = _service.ClassifyBatch(trades);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(999_999.99, "Private", RiskCategory.LOWRISK)]
    [InlineData(1_000_000, "Private", RiskCategory.HIGHRISK)]
    [InlineData(1_000_000, "Public", RiskCategory.MEDIUMRISK)]
    public void Classify_ShouldHandleBoundaryValues(decimal value, string sector, RiskCategory expected)
    {
        var trade = new Trade { Value = value, ClientSector = sector };

        var result = _service.Classify(trade);

        result.Should().Be(expected);
    }
}
