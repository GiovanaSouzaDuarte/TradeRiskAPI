using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TradeRiskAPI.Application.Rules;
using TradeRiskAPI.Application.Services;
using TradeRiskAPI.Application.Validators;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IClassificationRule, LowRiskRule>();
        services.AddSingleton<IClassificationRule, MediumRiskRule>();
        services.AddSingleton<IClassificationRule, HighRiskRule>();

        services.AddScoped<ITradeClassificationService, TradeClassificationService>();
        services.AddScoped<ITradeAnalysisService, TradeAnalysisService>();

        services.AddValidatorsFromAssemblyContaining<ClassifyRequestValidator>();

        return services;
    }
}
