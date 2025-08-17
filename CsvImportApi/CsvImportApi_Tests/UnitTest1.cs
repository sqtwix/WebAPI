using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class CsvImportControllerTests : IClassFixture<WebApplicationFactory<Program>>
{

    // Тестирования 2 и 3 метода
    private readonly HttpClient _client;

    public CsvImportControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDataWithFilters_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/CsvImport/results");

        // Assert
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(results);
    }

    [Fact]
    public async Task GetLatestResults_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/CsvImport/values/latest/_big_test");

        // Assert
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(results);
    }
}
