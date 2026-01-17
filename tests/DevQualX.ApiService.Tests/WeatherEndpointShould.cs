using DevQualX.Application.Weather;
using DevQualX.Domain.Models;
using DevQualX.Domain.Services;
using System.Net.Http.Json;

namespace DevQualX.ApiService.Tests;

/// <summary>
/// Unit tests for the weather forecast endpoint logic.
/// These tests mock dependencies and test the endpoint handler in isolation.
/// </summary>
public class WeatherEndpointShould
{
    [Test]
    public async Task Return_forecasts_from_application_service()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        var expectedForecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), 20, "Mild"),
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 22, "Warm")
        };
        A.CallTo(() => fakeWeatherService.GetForecastAsync(A<int>._, A<CancellationToken>._))
            .Returns(expectedForecasts);

        var applicationService = new GetWeatherForecast(fakeWeatherService);

        // Act
        var result = await applicationService.ExecuteAsync(5);

        // Assert
        await Assert.That(result).IsEqualTo(expectedForecasts);
    }
}

/// <summary>
/// Service tests for the weather forecast endpoint.
/// These tests use WebApplicationFactory to test the full HTTP pipeline with real DI.
/// Service tests validate that dependency injection is configured correctly.
/// Keep service tests minimal - prefer unit tests for most scenarios.
/// </summary>
public class WeatherEndpointServiceShould : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WeatherEndpointServiceShould()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task Return_success_status_code()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task Return_array_of_forecasts()
    {
        // Act
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");

        // Assert
        await Assert.That(forecasts).IsNotNull();
        await Assert.That(forecasts!.Length).IsGreaterThan(0);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}
