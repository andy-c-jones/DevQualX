using DevQualX.Domain.Models;

namespace DevQualX.Domain.Services;

/// <summary>
/// In-memory implementation of weather service for development/testing.
/// </summary>
public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<WeatherForecast[]> GetForecastAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var forecasts = Enumerable.Range(1, maxItems).Select(index => new WeatherForecast
        (
            startDate.AddDays(index),
            Random.Shared.Next(-20, 55),
            Summaries[Random.Shared.Next(Summaries.Length)]
        )).ToArray();

        return Task.FromResult(forecasts);
    }
}
