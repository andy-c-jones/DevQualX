using DevQualX.Domain.Services;
using DevQualX.Functional;

namespace DevQualX.Domain.Tests.Services;

public class WeatherServiceShould
{
    [Test]
    public async Task Return_requested_number_of_forecasts()
    {
        // Arrange
        var service = new WeatherService();
        const int expectedCount = 5;

        // Act
        var result = await service.GetForecastAsync(expectedCount);

        // Assert - result should be Success
        await Assert.That(result.IsSuccess).IsTrue();
        
        // Extract the forecasts from the Success result
        var forecasts = result.Match(
            success: data => data,
            failure: _ => Array.Empty<DevQualX.Domain.Models.WeatherForecast>()
        );
        
        await Assert.That(forecasts).HasCount().EqualTo(expectedCount);
    }
    
    [Test]
    public async Task Return_validation_error_when_maxItems_is_less_than_one()
    {
        // Arrange
        var service = new WeatherService();

        // Act
        var result = await service.GetForecastAsync(maxItems: 0);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        
        var error = result.Match(
            success: _ => null as Error,
            failure: err => err
        );
        
        await Assert.That(error).IsNotNull();
        await Assert.That(error).IsTypeOf<ValidationError>();
    }
    
    [Test]
    public async Task Return_validation_error_when_maxItems_exceeds_thirty()
    {
        // Arrange
        var service = new WeatherService();

        // Act
        var result = await service.GetForecastAsync(maxItems: 31);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        
        var error = result.Match(
            success: _ => null as Error,
            failure: err => err
        );
        
        await Assert.That(error).IsNotNull();
        await Assert.That(error).IsTypeOf<ValidationError>();
    }
}
