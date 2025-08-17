using CsvImportApi;
using CsvImportApi.Controllers;
using CsvImportApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text;
using Xunit;

public class CsvImportControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CsvImportControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadCSV_ReturnsOk_WhenFileIsValid()
    {
        // Подготовка файла для отправки
        var csvContent = "Date;Value;ExecutionTime\n2025-08-17T12:00:00;3.5;10";
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");

        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.csv");

        var response = await _client.PostAsync("/api/CsvImport/csv", formData);

        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadAsStringAsync();
        Assert.Equal("Файл успешно обработан", message);
    }

    [Fact]
    public async Task UploadCSV_ReturnsBadRequest_WhenFileIsEmpty()
    {
        var emptyFile = new ByteArrayContent(Array.Empty<byte>());
        emptyFile.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");

        var content = new MultipartFormDataContent
        {
            { emptyFile, "file", "empty.csv" }
        };

        var response = await _client.PostAsync("/api/CsvImport/csv", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Файл не загружен", errorMessage);
    }

    [Fact]
    public async Task UploadCSV_ReturnsBadRequest_WhenDataIsInvalid()
    {
        var invalidCsv = "Date;Value;ExecutionTime\n1999-08-17T12:00:00;3.5;10"; // Неправильный год
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(invalidCsv));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");

        var content = new MultipartFormDataContent
        {
            { fileContent, "file", "invalid.csv" }
        };

        var response = await _client.PostAsync("/api/CsvImport/csv", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
       
    }

}

