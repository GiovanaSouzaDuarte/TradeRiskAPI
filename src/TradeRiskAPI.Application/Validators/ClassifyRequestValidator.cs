using FluentValidation;
using TradeRiskAPI.Application.DTOs;

namespace TradeRiskAPI.Application.Validators;

public class TradeDtoValidator : AbstractValidator<TradeDto>
{
    private static readonly string[] ValidSectors = { "Public", "Private" };

    public TradeDtoValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Trade value must be non-negative");

        RuleFor(x => x.ClientSector)
            .NotEmpty()
            .WithMessage("Client sector is required")
            .Must(sector => ValidSectors.Contains(sector, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Client sector must be 'Public' or 'Private'");
    }
}

public class ClassifyRequestValidator : AbstractValidator<ClassifyRequest>
{
    public ClassifyRequestValidator()
    {
        RuleFor(x => x.Trades)
            .NotNull()
            .WithMessage("Trades list is required")
            .NotEmpty()
            .WithMessage("Trades list cannot be empty");

        RuleForEach(x => x.Trades)
            .SetValidator(new TradeDtoValidator());
    }
}
