using DevQualX.Domain.Services;

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

        // Assert
        await Assert.That(result).HasCount().EqualTo(expectedCount);
    }
}
