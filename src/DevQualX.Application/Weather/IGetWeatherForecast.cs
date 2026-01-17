using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Weather;

/// <summary>
/// Interface for the weather forecast retrieval application service.
/// Follows Interaction-Driven Design (IDD) - each application service has its own interface.
/// </summary>
public interface IGetWeatherForecast
{
    /// <summary>
    /// Executes the weather forecast retrieval.
    /// </summary>
    /// <param name="maxItems">Maximum number of forecast items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing an array of weather forecasts or an error.</returns>
    Task<Result<WeatherForecast[], Error>> ExecuteAsync(int maxItems = 10, CancellationToken cancellationToken = default);
}
