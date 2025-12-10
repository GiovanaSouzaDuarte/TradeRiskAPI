using FluentAssertions;
using TradeRiskAPI.Application.Rules;
using TradeRiskAPI.Application.Services;
using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.UnitTests.Services;

public class TradeAnalysisServiceTests
{
    private readonly ITradeAnalysisService _service;

    public TradeAnalysisServiceTests()
    {
        var rules = new IClassificationRule[]
        {
            new LowRiskRule(),
            new MediumRiskRule(),
            new HighRiskRule()
        };
        var classificationService = new TradeClassificationService(rules);
        _service = new TradeAnalysisService(classificationService);
    }

    [Fact]
    public void Analyze_ShouldReturnCorrectCategories()
    {
        var trades = new[]
        {
            new Trade { Value = 2_000_000, ClientSector = "Private", ClientId = "CLI001" },
            new Trade { Value = 400_000, ClientSector = "Public", ClientId = "CLI002" },
            new Trade { Value = 500_000, ClientSector = "Public", ClientId = "CLI003" },
            new Trade { Value = 3_000_000, ClientSector = "Public", ClientId = "CLI004" }
        };

        var (categories, _, _) = _service.Analyze(trades);

        categories.Should().HaveCount(4);
        categories[0].Should().Be(RiskCategory.HIGHRISK);
        categories[1].Should().Be(RiskCategory.LOWRISK);
        categories[2].Should().Be(RiskCategory.LOWRISK);
        categories[3].Should().Be(RiskCategory.MEDIUMRISK);
    }

    [Fact]
    public void Analyze_ShouldReturnCorrectSummary()
    {
        var trades = new[]
        {
            new Trade { Value = 2_000_000, ClientSector = "Private", ClientId = "CLI001" },
            new Trade { Value = 400_000, ClientSector = "Public", ClientId = "CLI002" },
            new Trade { Value = 500_000, ClientSector = "Public", ClientId = "CLI003" },
            new Trade { Value = 3_000_000, ClientSector = "Public", ClientId = "CLI004" }
        };

        var (_, summary, _) = _service.Analyze(trades);

        summary.Should().ContainKey(RiskCategory.LOWRISK);
        summary.Should().ContainKey(RiskCategory.MEDIUMRISK);
        summary.Should().ContainKey(RiskCategory.HIGHRISK);

        summary[RiskCategory.LOWRISK].Count.Should().Be(2);
        summary[RiskCategory.LOWRISK].TotalValue.Should().Be(900_000);

        summary[RiskCategory.MEDIUMRISK].Count.Should().Be(1);
        summary[RiskCategory.MEDIUMRISK].TotalValue.Should().Be(3_000_000);

        summary[RiskCategory.HIGHRISK].Count.Should().Be(1);
        summary[RiskCategory.HIGHRISK].TotalValue.Should().Be(2_000_000);
    }

    [Fact]
    public void Analyze_ShouldIdentifyTopClient()
    {
        var trades = new[]
        {
            new Trade { Value = 500_000, ClientSector = "Public", ClientId = "CLI001" },
            new Trade { Value = 300_000, ClientSector = "Public", ClientId = "CLI002" },
            new Trade { Value = 100_000, ClientSector = "Public", ClientId = "CLI001" }
        };

        var (_, summary, _) = _service.Analyze(trades);

        summary[RiskCategory.LOWRISK].TopClient.Should().Be("CLI001");
    }

    [Fact]
    public void Analyze_ShouldReturnProcessingTime()
    {
        var trades = new[]
        {
            new Trade { Value = 500_000, ClientSector = "Public", ClientId = "CLI001" }
        };

        var (_, _, processingTimeMs) = _service.Analyze(trades);

        processingTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Analyze_ShouldHandleEmptyList()
    {
        var trades = Array.Empty<Trade>();

        var (categories, summary, _) = _service.Analyze(trades);

        categories.Should().BeEmpty();
        summary[RiskCategory.LOWRISK].Count.Should().Be(0);
        summary[RiskCategory.MEDIUMRISK].Count.Should().Be(0);
        summary[RiskCategory.HIGHRISK].Count.Should().Be(0);
    }

    [Fact]
    public void Analyze_ShouldHandleLargeVolume()
    {
        var trades = Enumerable.Range(0, 10_000)
            .Select(i => new Trade
            {
                Value = i % 2 == 0 ? 500_000 : 2_000_000,
                ClientSector = i % 3 == 0 ? "Public" : "Private",
                ClientId = $"CLI{i % 100:D3}"
            })
            .ToList();

        var (categories, summary, processingTimeMs) = _service.Analyze(trades);

        categories.Should().HaveCount(10_000);
        summary.Values.Sum(s => s.Count).Should().Be(10_000);
    }
}
