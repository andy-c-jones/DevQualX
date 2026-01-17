namespace DevQualX.Domain.Models;

/// <summary>
/// Weather forecast data transfer object.
/// </summary>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Temperature in Fahrenheit, calculated from Celsius.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
