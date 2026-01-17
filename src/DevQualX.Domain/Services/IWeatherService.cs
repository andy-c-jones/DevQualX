using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Domain.Services;

/// <summary>
/// Domain service interface for weather-related operations.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets weather forecast data.
    /// </summary>
    /// <param name="maxItems">Maximum number of forecast items to return. Must be between 1 and 30.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing an array of weather forecasts or an error.</returns>
    Task<Result<WeatherForecast[], Error>> GetForecastAsync(int maxItems = 10, CancellationToken cancellationToken = default);
}
