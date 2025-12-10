using FluentValidation;
using TradeRiskAPI.Application.DTOs;

namespace TradeRiskAPI.Application.Validators;

public class TradeWithClientDtoValidator : AbstractValidator<TradeWithClientDto>
{
    private static readonly string[] ValidSectors = { "Public", "Private" };

    public TradeWithClientDtoValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Trade value must be non-negative");

        RuleFor(x => x.ClientSector)
            .NotEmpty()
            .WithMessage("Client sector is required")
            .Must(sector => ValidSectors.Contains(sector, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Client sector must be 'Public' or 'Private'");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required for analysis");
    }
}

public class AnalyzeRequestValidator : AbstractValidator<AnalyzeRequest>
{
    private const int MaxTrades = 100_000;

    public AnalyzeRequestValidator()
    {
        RuleFor(x => x.Trades)
            .NotNull()
            .WithMessage("Trades list is required")
            .NotEmpty()
            .WithMessage("Trades list cannot be empty")
            .Must(trades => trades.Count <= MaxTrades)
            .WithMessage($"Maximum of {MaxTrades:N0} trades allowed per request");

        RuleForEach(x => x.Trades)
            .SetValidator(new TradeWithClientDtoValidator());
    }
}
