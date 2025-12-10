using FluentAssertions;
using TradeRiskAPI.Application.Rules;
using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Enums;

namespace TradeRiskAPI.UnitTests.Rules;

public class ClassificationRulesTests
{
    private readonly LowRiskRule _lowRiskRule = new();
    private readonly MediumRiskRule _mediumRiskRule = new();
    private readonly HighRiskRule _highRiskRule = new();

    [Theory]
    [InlineData(0)]
    [InlineData(500_000)]
    [InlineData(999_999.99)]
    public void LowRiskRule_ShouldMatch_WhenValueBelowThreshold(decimal value)
    {
        var trade = new Trade { Value = value, ClientSector = "Public" };

        var result = _lowRiskRule.Matches(trade);

        result.Should().BeTrue();
        _lowRiskRule.Category.Should().Be(RiskCategory.LOWRISK);
    }

    [Theory]
    [InlineData(1_000_000)]
    [InlineData(2_000_000)]
    [InlineData(10_000_000)]
    public void LowRiskRule_ShouldNotMatch_WhenValueAtOrAboveThreshold(decimal value)
    {
        var trade = new Trade { Value = value, ClientSector = "Public" };

        var result = _lowRiskRule.Matches(trade);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1_000_000, "Public")]
    [InlineData(3_000_000, "public")]
    [InlineData(5_000_000, "PUBLIC")]
    public void MediumRiskRule_ShouldMatch_WhenValueAboveThresholdAndPublicSector(decimal value, string sector)
    {
        var trade = new Trade { Value = value, ClientSector = sector };

        var result = _mediumRiskRule.Matches(trade);

        result.Should().BeTrue();
        _mediumRiskRule.Category.Should().Be(RiskCategory.MEDIUMRISK);
    }

    [Fact]
    public void MediumRiskRule_ShouldNotMatch_WhenPrivateSector()
    {
        var trade = new Trade { Value = 2_000_000, ClientSector = "Private" };

        var result = _mediumRiskRule.Matches(trade);

        result.Should().BeFalse();
    }

    [Fact]
    public void MediumRiskRule_ShouldNotMatch_WhenValueBelowThreshold()
    {
        var trade = new Trade { Value = 500_000, ClientSector = "Public" };

        var result = _mediumRiskRule.Matches(trade);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1_000_000, "Private")]
    [InlineData(2_000_000, "private")]
    [InlineData(5_000_000, "PRIVATE")]
    public void HighRiskRule_ShouldMatch_WhenValueAboveThresholdAndPrivateSector(decimal value, string sector)
    {
        var trade = new Trade { Value = value, ClientSector = sector };

        var result = _highRiskRule.Matches(trade);

        result.Should().BeTrue();
        _highRiskRule.Category.Should().Be(RiskCategory.HIGHRISK);
    }

    [Fact]
    public void HighRiskRule_ShouldNotMatch_WhenPublicSector()
    {
        var trade = new Trade { Value = 2_000_000, ClientSector = "Public" };

        var result = _highRiskRule.Matches(trade);

        result.Should().BeFalse();
    }

    [Fact]
    public void HighRiskRule_ShouldNotMatch_WhenValueBelowThreshold()
    {
        var trade = new Trade { Value = 500_000, ClientSector = "Private" };

        var result = _highRiskRule.Matches(trade);

        result.Should().BeFalse();
    }

    [Fact]
    public void Rules_ShouldHaveCorrectPriority()
    {
        _lowRiskRule.Priority.Should().Be(1);
        _mediumRiskRule.Priority.Should().Be(2);
        _highRiskRule.Priority.Should().Be(3);
    }
}
