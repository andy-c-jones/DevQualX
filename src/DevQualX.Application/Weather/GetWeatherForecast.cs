using DevQualX.Domain.Models;
using DevQualX.Domain.Services;
using DevQualX.Functional;

namespace DevQualX.Application.Weather;

/// <summary>
/// Application service for retrieving weather forecast data.
/// Follows Interaction-Driven Design (IDD) - application services should never call other application services.
/// </summary>
public class GetWeatherForecast(IWeatherService weatherService) : IGetWeatherForecast
{
    /// <summary>
    /// Executes the weather forecast retrieval.
    /// </summary>
    /// <param name="maxItems">Maximum number of forecast items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing an array of weather forecasts or an error.</returns>
    public async Task<Result<WeatherForecast[], Error>> ExecuteAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        return await weatherService.GetForecastAsync(maxItems, cancellationToken);
    }
}
