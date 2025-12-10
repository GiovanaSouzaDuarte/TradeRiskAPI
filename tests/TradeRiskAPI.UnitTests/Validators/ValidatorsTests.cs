using FluentAssertions;
using TradeRiskAPI.Application.DTOs;
using TradeRiskAPI.Application.Validators;

namespace TradeRiskAPI.UnitTests.Validators;

public class ValidatorsTests
{
    private readonly ClassifyRequestValidator _classifyValidator = new();
    private readonly AnalyzeRequestValidator _analyzeValidator = new();

    [Fact]
    public void ClassifyRequestValidator_ShouldPass_WhenValidRequest()
    {
        var request = new ClassifyRequest(new List<TradeDto>
        {
            new(1_000_000, "Public"),
            new(500_000, "Private")
        });

        var result = _classifyValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ClassifyRequestValidator_ShouldFail_WhenTradesIsNull()
    {
        var request = new ClassifyRequest(null!);

        var result = _classifyValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Trades");
    }

    [Fact]
    public void ClassifyRequestValidator_ShouldFail_WhenTradesIsEmpty()
    {
        var request = new ClassifyRequest(new List<TradeDto>());

        var result = _classifyValidator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ClassifyRequestValidator_ShouldFail_WhenInvalidSector()
    {
        var request = new ClassifyRequest(new List<TradeDto>
        {
            new(1_000_000, "InvalidSector")
        });

        var result = _classifyValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Public") || e.ErrorMessage.Contains("Private"));
    }

    [Fact]
    public void ClassifyRequestValidator_ShouldFail_WhenNegativeValue()
    {
        var request = new ClassifyRequest(new List<TradeDto>
        {
            new(-100, "Public")
        });

        var result = _classifyValidator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AnalyzeRequestValidator_ShouldPass_WhenValidRequest()
    {
        var request = new AnalyzeRequest(new List<TradeWithClientDto>
        {
            new(1_000_000, "Public", "CLI001"),
            new(500_000, "Private", "CLI002")
        });

        var result = _analyzeValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnalyzeRequestValidator_ShouldFail_WhenClientIdIsEmpty()
    {
        var request = new AnalyzeRequest(new List<TradeWithClientDto>
        {
            new(1_000_000, "Public", "")
        });

        var result = _analyzeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Client ID"));
    }

    [Fact]
    public void AnalyzeRequestValidator_ShouldFail_WhenExceedsMaxTrades()
    {
        var trades = Enumerable.Range(0, 100_001)
            .Select(i => new TradeWithClientDto(1_000_000, "Public", $"CLI{i}"))
            .ToList();
        var request = new AnalyzeRequest(trades);

        var result = _analyzeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("100,000"));
    }
}
