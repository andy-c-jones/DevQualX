using DevQualX.Application.Weather;
using DevQualX.Domain.Models;
using DevQualX.Domain.Services;

namespace DevQualX.Application.Tests.Weather;

public class GetWeatherForecastShould
{
    [Test]
    public async Task Call_weather_service_with_correct_parameters()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        var expectedForecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), 20, "Mild")
        };
        A.CallTo(() => fakeWeatherService.GetForecastAsync(A<int>._, A<CancellationToken>._))
            .Returns(expectedForecasts);

        var service = new GetWeatherForecast(fakeWeatherService);

        // Act
        var result = await service.ExecuteAsync(5);

        // Assert
        await Assert.That(result).IsEqualTo(expectedForecasts);
        A.CallTo(() => fakeWeatherService.GetForecastAsync(5, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
