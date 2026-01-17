using DevQualX.Application.Weather;
using DevQualX.Domain.Models;
using DevQualX.Domain.Services;
using DevQualX.Functional;
using DevQualX.Web.Components.Pages;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using FunctionalError = DevQualX.Functional.Error;

namespace DevQualX.Web.Tests;

/// <summary>
/// bUnit tests for the Weather Blazor component.
/// These tests verify component rendering and behavior using bUnit's TestContext.
/// </summary>
public class WeatherPageShould : Bunit.TestContext
{
    [Test]
    public void Display_loading_message_initially()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        Services.AddSingleton(fakeWeatherService);
        Services.AddSingleton<IGetWeatherForecast, GetWeatherForecast>();

        // Act
        var cut = RenderComponent<Weather>();

        // Assert
        cut.MarkupMatches("<h1>Weather</h1><p>This component demonstrates showing data with functional error handling.</p><p><em>Loading...</em></p>");
    }

    [Test]
    public async Task Display_weather_forecasts_after_loading()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        var testForecasts = new[]
        {
            new WeatherForecast(new DateOnly(2026, 1, 20), 20, "Mild"),
            new WeatherForecast(new DateOnly(2026, 1, 21), 22, "Warm")
        };
        
        // Return a Success Result
        Result<WeatherForecast[], FunctionalError> successResult = testForecasts;
        A.CallTo(() => fakeWeatherService.GetForecastAsync(A<int>._, A<CancellationToken>._))
            .Returns(successResult);

        Services.AddSingleton(fakeWeatherService);
        Services.AddSingleton<IGetWeatherForecast, GetWeatherForecast>();

        // Act
        var cut = RenderComponent<Weather>();
        
        // Wait for the component to finish loading (it has a 500ms delay)
        await Task.Delay(600);

        // Assert
        var tableRows = cut.FindAll("tbody tr");
        await Assert.That(tableRows.Count).IsEqualTo(2);
        await Assert.That(cut.Markup).Contains("Mild");
        await Assert.That(cut.Markup).Contains("Warm");
    }
}
