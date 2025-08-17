using CsvImportApi;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class GetTenLatestDatasTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetTenLatestDatasTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTenLatestDatas_ReturnsEmpty_WhenNoValues()
    {
        var response = await _client.GetAsync("/api/CsvImport/values/latest/nonexistent.csv"); // Такого файла нет
        response.EnsureSuccessStatusCode();

        var values = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(values);
        Assert.Empty(values); 
    }

    [Fact]
    public async Task GetLatestDatas_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/CsvImport/values/latest/_big_test"); // Такой файл есть в Testing_data
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<object[]>();
        Assert.NotNull(results);
    }
}
