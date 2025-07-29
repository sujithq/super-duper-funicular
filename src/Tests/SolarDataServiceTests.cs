using SolarScope.Models;
using SolarScope.Services;
using System.Text.Json;
using Xunit;

namespace SolarScope.Tests;

public class SolarDataServiceTests
{
    [Fact]
    public async Task LoadDataAsync_WithValidFile_ReturnsData()
    {
        // Arrange - Create test data in the correct format that matches the JSON structure
        var testDataDict = new Dictionary<int, List<BarChartData>>
        {
            [2023] = new List<BarChartData>
            {
                new BarChartData(
                    D: 1,
                    P: 15.5,
                    U: 12.3,
                    I: 3.2,
                    J: true,
                    S: false,
                    MS: new MeteoStatData(8.6, 7.5, 10.1, 0.2, 0, 242, 29.7, 57.4, 1010, 0),
                    M: true,
                    AS: new AnomalyData(0, 0, 0, false),
                    Q: new QuarterData(new(), new(), new(), new(), null, null, null),
                    C: true,
                    SRS: new SunRiseSet(DateTime.Parse("2023-01-01T07:37:48"), DateTime.Parse("2023-01-01T16:02:37"))
                )
            }
        };

        // Create a SolarData object and populate it with test data
        var testData = new SolarData();
        foreach (var kvp in testDataDict)
        {
            testData[kvp.Key] = kvp.Value;
        }

        // Serialize the SolarData object directly (not wrapped in a "years" property)
        var json = JsonSerializer.Serialize(testData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new SolarDataConverter() }
        });
        
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, json);

        var service = new SolarDataService(tempFile);

        // Act
        var result = await service.LoadDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GetYearData(2023));
        Assert.Equal(15.5, result.GetYearData(2023)[0].P);
        Assert.Single(result);
        Assert.Contains(2023, result.Keys);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void BarChartData_CalculatesPropertiesCorrectly()
    {
        // Arrange
        var data = new BarChartData(
            D: 1,
            P: 20.0,
            U: 15.0,
            I: 5.0,
            J: true,
            S: false,
            MS: new MeteoStatData(25.0, 20.0, 30.0, 0, 0, 180, 10, 20, 1013, 8),
            M: true,
            AS: new AnomalyData(0, 0, 0, false),
            Q: new QuarterData(
                ConsumptionReadings: new List<double> { 0.5, 1.0, 1.5 }, 
                InjectionReadings: new(), 
                GenerationReadings: new(), 
                ProductionReadings: new List<double> { 0.5, 1.0, 1.5 }, // Use ProductionReadings for PeakProduction test
                WaterReturnTemp: null, 
                WaterOutletTemp: null, 
                WaterPressure: null)
        );

        // Act & Assert
        Assert.Equal(75.0, data.Efficiency); // 15/20 * 100
        Assert.Equal(5.0, data.EnergyBalance); // 20 - 15
        Assert.True(data.IsEnergyPositive);
        Assert.Equal(1.5, data.PeakProduction); // Max of ProductionReadings
        Assert.Equal(1.0, data.AverageProduction, 1); // Average of ProductionReadings
    }

    [Fact]
    public async Task LoadDataAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var service = new SolarDataService("nonexistent.json");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await service.LoadDataAsync());
    }

    [Fact]
    public async Task LoadDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "invalid json content");

        var service = new SolarDataService(tempFile);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LoadDataAsync());
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Theory]
    [InlineData(0, 10, 0)] // No production = 0% efficiency
    [InlineData(10, 5, 50)] // Normal efficiency
    [InlineData(20, 20, 100)] // Perfect efficiency
    public void BarChartData_EnergyEfficiency_CalculatedCorrectly(double production, double consumption, double expected)
    {
        // Arrange
        var data = new BarChartData(
            D: 1,
            P: production,
            U: consumption,
            I: 0,
            J: false,
            S: false,
            MS: new MeteoStatData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            M: false,
            AS: new AnomalyData(0, 0, 0, false),
            Q: new QuarterData(new(), new(), new(), new(), null, null, null)
        );

        // Act
        var efficiency = data.Efficiency;

        // Assert
        Assert.Equal(expected, efficiency, precision: 2);
    }

    [Fact]
    public void SolarData_GetLatestYearData_ReturnsCorrectYear()
    {
        // Arrange
        var solarData = new SolarData();
        var testData2023 = new List<BarChartData>
        {
            new BarChartData(1, 10, 8, 2, false, false, new MeteoStatData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0), false, new AnomalyData(0, 0, 0, false), new QuarterData(new(), new(), new(), new(), null, null, null))
        };
        var testData2024 = new List<BarChartData>
        {
            new BarChartData(1, 15, 12, 3, false, false, new MeteoStatData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0), false, new AnomalyData(0, 0, 0, false), new QuarterData(new(), new(), new(), new(), null, null, null))
        };

        solarData[2023] = testData2023;
        solarData[2024] = testData2024;

        // Act
        var latestData = solarData.GetLatestYearData();
        var latestYear = solarData.LatestYear;

        // Assert
        Assert.Equal(2024, latestYear);
        Assert.Equal(testData2024, latestData);
        Assert.Equal(15, latestData[0].P);
    }

    [Fact]
    public void SolarData_GetYearData_ReturnsEmptyForNonExistentYear()
    {
        // Arrange
        var solarData = new SolarData();
        solarData[2023] = new List<BarChartData>();

        // Act
        var result = solarData.GetYearData(2025);

        // Assert
        Assert.Empty(result);
    }
}
