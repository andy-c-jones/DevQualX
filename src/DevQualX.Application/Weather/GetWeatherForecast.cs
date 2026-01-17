using DevQualX.Domain.Models;
using DevQualX.Domain.Services;

namespace DevQualX.Application.Weather;

/// <summary>
/// Application service for retrieving weather forecast data.
/// Follows Interaction-Driven Design (IDD) - application services should never call other application services.
/// </summary>
public class GetWeatherForecast(IWeatherService weatherService)
{
    /// <summary>
    /// Executes the weather forecast retrieval.
    /// </summary>
    /// <param name="maxItems">Maximum number of forecast items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of weather forecasts.</returns>
    public async Task<WeatherForecast[]> ExecuteAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        return await weatherService.GetForecastAsync(maxItems, cancellationToken);
    }
}
