using CsvImportApi;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class GetDataWithFiltersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetDataWithFiltersTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    
    [Fact]
    public async Task GetDataWithFilters_ReturnsEmpty_WhenNoResults()
    {
        var response = await _client.GetAsync("/api/CsvImport/results?fileName=not_exist.csv"); // Такого файла нет
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(results);
        Assert.Empty(results); 
    }

    [Fact]
    public async Task GetDataWithFilters_ReturnsFilteredResults()
    {
        var response = await _client.GetAsync("/api/CsvImport/results?fileName=_big_test.csv&avgValueFrom=3.5&avgValueTo=4.0");// Заранее загруженный файл из папки Testing_data


        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(results);
        Assert.All(results, r => Assert.True(((dynamic)r).AvgValue >= 3.0 && ((dynamic)r).AvgValue <= 4.0));
    }

}
