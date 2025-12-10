using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TradeRiskAPI.IntegrationTests;

public class TradesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TradesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Classify_ShouldReturnCorrectCategories()
    {
        var request = new
        {
            trades = new[]
            {
                new { value = 2000000, clientSector = "Private" },
                new { value = 400000, clientSector = "Public" },
                new { value = 500000, clientSector = "Public" },
                new { value = 3000000, clientSector = "Public" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/trades/classify", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ClassifyResponseDto>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Categories.Should().HaveCount(4);
        result.Categories[0].Should().Be("HIGHRISK");
        result.Categories[1].Should().Be("LOWRISK");
        result.Categories[2].Should().Be("LOWRISK");
        result.Categories[3].Should().Be("MEDIUMRISK");
    }

    [Fact]
    public async Task Classify_ShouldReturnBadRequest_WhenInvalidSector()
    {
        var request = new
        {
            trades = new[]
            {
                new { value = 1000000, clientSector = "InvalidSector" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/trades/classify", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Classify_ShouldReturnBadRequest_WhenEmptyTrades()
    {
        var request = new
        {
            trades = Array.Empty<object>()
        };

        var response = await _client.PostAsJsonAsync("/api/trades/classify", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Analyze_ShouldReturnCategoriesAndSummary()
    {
        var request = new
        {
            trades = new[]
            {
                new { value = 2000000, clientSector = "Private", clientId = "CLI001" },
                new { value = 400000, clientSector = "Public", clientId = "CLI002" },
                new { value = 500000, clientSector = "Public", clientId = "CLI003" },
                new { value = 3000000, clientSector = "Public", clientId = "CLI004" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/trades/analyze", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnalyzeResponseDto>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Categories.Should().HaveCount(4);
        result.Summary.Should().ContainKey("LOWRISK");
        result.Summary.Should().ContainKey("MEDIUMRISK");
        result.Summary.Should().ContainKey("HIGHRISK");
        result.ProcessingTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Analyze_ShouldReturnCorrectSummaryValues()
    {
        var request = new
        {
            trades = new[]
            {
                new { value = 2000000, clientSector = "Private", clientId = "CLI001" },
                new { value = 400000, clientSector = "Public", clientId = "CLI002" },
                new { value = 500000, clientSector = "Public", clientId = "CLI003" },
                new { value = 3000000, clientSector = "Public", clientId = "CLI004" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/trades/analyze", request);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnalyzeResponseDto>(content, _jsonOptions);

        result!.Summary["LOWRISK"].Count.Should().Be(2);
        result.Summary["LOWRISK"].TotalValue.Should().Be(900000);

        result.Summary["MEDIUMRISK"].Count.Should().Be(1);
        result.Summary["MEDIUMRISK"].TotalValue.Should().Be(3000000);
        result.Summary["MEDIUMRISK"].TopClient.Should().Be("CLI004");

        result.Summary["HIGHRISK"].Count.Should().Be(1);
        result.Summary["HIGHRISK"].TotalValue.Should().Be(2000000);
        result.Summary["HIGHRISK"].TopClient.Should().Be("CLI001");
    }

    [Fact]
    public async Task Analyze_ShouldReturnBadRequest_WhenMissingClientId()
    {
        var request = new
        {
            trades = new[]
            {
                new { value = 1000000, clientSector = "Public", clientId = "" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/trades/analyze", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Classify_ShouldHandleCaseInsensitiveSector()
    {
        var request = new
        {
            trades = new[]
            {
                new { value = 2000000, clientSector = "PRIVATE" },
                new { value = 2000000, clientSector = "public" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/trades/classify", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ClassifyResponseDto>(content, _jsonOptions);

        result!.Categories[0].Should().Be("HIGHRISK");
        result.Categories[1].Should().Be("MEDIUMRISK");
    }

    private record ClassifyResponseDto(List<string> Categories);
    
    private record CategorySummaryDto(int Count, decimal TotalValue, string TopClient);
    
    private record AnalyzeResponseDto(
        List<string> Categories,
        Dictionary<string, CategorySummaryDto> Summary,
        long ProcessingTimeMs
    );
}
