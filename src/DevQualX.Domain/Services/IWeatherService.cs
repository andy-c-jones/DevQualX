using DevQualX.Domain.Models;

namespace DevQualX.Domain.Services;

/// <summary>
/// Domain service interface for weather-related operations.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets weather forecast data.
    /// </summary>
    /// <param name="maxItems">Maximum number of forecast items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of weather forecasts.</returns>
    Task<WeatherForecast[]> GetForecastAsync(int maxItems = 10, CancellationToken cancellationToken = default);
}
