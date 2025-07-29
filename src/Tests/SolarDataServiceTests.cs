using SolarScope.Models;
using SolarScope.Services;
using System.Text.Json;

namespace SolarScope.Tests;

public class SolarDataServiceTests
{
    [Fact]
    public async Task LoadDataAsync_WithValidFile_ReturnsData()
    {
        // Arrange
        var testData = new SolarData(new List<BarChartData>
        {
            new BarChartData(
                Day: 1,
                TotalProduction: 15.5,
                TotalConsumption: 12.3,
                GridInjection: 3.2,
                HasJanuary: true,
                HasSummer: false,
                WeatherStats: new MeteoStatData(8.6, 7.5, 10.1, 0.2, 0, 242, 29.7, 57.4, 1010, 0),
                HasMeasurements: true,
                AnomalyStats: new AnomalyData(0, 0, 0, false),
                QuarterlyData: new QuarterData(new(), new(), new(), new(), null, null, null),
                IsComplete: true,
                SunTimes: new SunRiseSet(DateTime.Parse("2023-01-01T07:37:48"), DateTime.Parse("2023-01-01T16:02:37"))
            )
        });

        var json = JsonSerializer.Serialize(new { _2023 = testData.Year2023 });
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, json);

        var service = new SolarDataService(tempFile);

        // Act
        var result = await service.LoadDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Year2023);
        Assert.Equal(15.5, result.Year2023[0].TotalProduction);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void BarChartData_CalculatesPropertiesCorrectly()
    {
        // Arrange
        var data = new BarChartData(
            Day: 1,
            TotalProduction: 20.0,
            TotalConsumption: 15.0,
            GridInjection: 5.0,
            HasJanuary: true,
            HasSummer: false,
            WeatherStats: new MeteoStatData(25.0, 20.0, 30.0, 0, 0, 180, 10, 20, 1013, 8),
            HasMeasurements: true,
            AnomalyStats: new AnomalyData(0, 0, 0, false),
            QuarterlyData: new QuarterData(new List<double> { 0.5, 1.0, 1.5 }, new(), new(), new(), null, null, null)
        );

        // Act & Assert
        Assert.Equal(75.0, data.Efficiency); // 15/20 * 100
        Assert.Equal(5.0, data.EnergyBalance); // 20 - 15
        Assert.True(data.IsEnergyPositive);
        Assert.Equal(1.5, data.PeakProduction);
        Assert.Equal(1.0, data.AverageProduction, 1);
    }
}
