using DevQualX.Domain.Models;
using DevQualX.Functional;

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

    public Task<Result<WeatherForecast[], Error>> GetForecastAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        // Validation: maxItems must be between 1 and 30
        if (maxItems < 1)
        {
            return Task.FromResult<Result<WeatherForecast[], Error>>(
                new ValidationError
                {
                    Message = "Maximum items must be at least 1",
                    Code = "WEATHER001",
                    Errors = new Dictionary<string, string[]>
                    {
                        [nameof(maxItems)] = ["Value must be at least 1"]
                    }
                });
        }

        if (maxItems > 30)
        {
            return Task.FromResult<Result<WeatherForecast[], Error>>(
                new ValidationError
                {
                    Message = "Maximum items cannot exceed 30",
                    Code = "WEATHER002",
                    Errors = new Dictionary<string, string[]>
                    {
                        [nameof(maxItems)] = ["Value cannot exceed 30"]
                    }
                });
        }

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var forecasts = Enumerable.Range(1, maxItems).Select(index => new WeatherForecast
        (
            startDate.AddDays(index),
            Random.Shared.Next(-20, 55),
            Summaries[Random.Shared.Next(Summaries.Length)]
        )).ToArray();

        // Implicit conversion to Success<WeatherForecast[], Error>
        return Task.FromResult<Result<WeatherForecast[], Error>>(forecasts);
    }
}
