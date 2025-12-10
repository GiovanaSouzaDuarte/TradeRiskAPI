using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TradeRiskAPI.Application.DTOs;
using TradeRiskAPI.Domain.Entities;
using TradeRiskAPI.Domain.Interfaces;

namespace TradeRiskAPI.WebAPI.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly ITradeClassificationService _classificationService;
    private readonly ITradeAnalysisService _analysisService;
    private readonly IValidator<ClassifyRequest> _classifyValidator;
    private readonly IValidator<AnalyzeRequest> _analyzeValidator;
    private readonly ILogger<TradesController> _logger;

    public TradesController(
        ITradeClassificationService classificationService,
        ITradeAnalysisService analysisService,
        IValidator<ClassifyRequest> classifyValidator,
        IValidator<AnalyzeRequest> analyzeValidator,
        ILogger<TradesController> logger)
    {
        _classificationService = classificationService;
        _analysisService = analysisService;
        _classifyValidator = classifyValidator;
        _analyzeValidator = analyzeValidator;
        _logger = logger;
    }

    [HttpPost("classify")]
    [ProducesResponseType(typeof(ClassifyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Classify([FromBody] ClassifyRequest request)
    {
        _logger.LogInformation("Classifying {Count} trades", request.Trades?.Count ?? 0);

        var validationResult = await _classifyValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var trades = request?.Trades?.Select(t => new Trade
        {
            Value = t.Value,
            ClientSector = t.ClientSector
        });

        var categories = _classificationService.ClassifyBatch(trades);
        var response = new ClassifyResponse(categories.Select(c => c.ToString()).ToList());

        _logger.LogInformation("Classification completed for {Count} trades", request.Trades.Count);

        return Ok(response);
    }

    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalyzeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
    {
        _logger.LogInformation("Analyzing {Count} trades", request.Trades?.Count ?? 0);

        var validationResult = await _analyzeValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var trades = request?.Trades?.Select(t => new Trade
        {
            Value = t.Value,
            ClientSector = t.ClientSector,
            ClientId = t.ClientId
        });


        var (categories, summary, processingTimeMs) = _analysisService.Analyze(trades);

        var summaryDto = summary.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => new CategorySummaryDto(kvp.Value.Count, kvp.Value.TotalValue, kvp.Value.TopClient)
        );

        var response = new AnalyzeResponse(
            categories.Select(c => c.ToString()).ToList(),
            summaryDto,
            processingTimeMs
        );

        _logger.LogInformation("Analysis completed for {Count} trades in {Time}ms", 
            request?.Trades?.Count, processingTimeMs);

        return Ok(response);
    }
}
