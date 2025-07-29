using System.Text.Json;
using SolarScope.Models;

namespace SolarScope.Services;

/// <summary>
/// Service for loading and parsing solar system data
/// </summary>
public class SolarDataService
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SolarDataService(string dataFilePath = "data/sample.json")
    {
        _dataFilePath = dataFilePath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Loads solar data from the JSON file
    /// </summary>
    public async Task<SolarData?> LoadDataAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                throw new FileNotFoundException($"Data file not found: {_dataFilePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(_dataFilePath);
            return JsonSerializer.Deserialize<SolarData>(jsonContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load solar data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets data for a specific date range
    /// </summary>
    public List<BarChartData> GetDataForDateRange(SolarData data, int startDay, int endDay)
    {
        return data.Year2023
            .Where(d => d.Day >= startDay && d.Day <= endDay)
            .OrderBy(d => d.Day)
            .ToList();
    }

    /// <summary>
    /// Gets anomaly data above a certain threshold
    /// </summary>
    public List<BarChartData> GetAnomalousData(SolarData data, AnomalySeverity minSeverity = AnomalySeverity.Low)
    {
        return data.Year2023
            .Where(d => d.AnomalyStats.Severity >= minSeverity)
            .OrderByDescending(d => d.AnomalyStats.TotalAnomalyScore)
            .ToList();
    }

    /// <summary>
    /// Gets days with the best solar production
    /// </summary>
    public List<BarChartData> GetTopProductionDays(SolarData data, int count = 10)
    {
        return data.Year2023
            .OrderByDescending(d => d.TotalProduction)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets days with the worst weather conditions
    /// </summary>
    public List<BarChartData> GetWorstWeatherDays(SolarData data, int count = 10)
    {
        return data.Year2023
            .OrderBy(d => d.WeatherStats.SunshineHours)
            .ThenByDescending(d => d.WeatherStats.Precipitation)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Calculates monthly statistics
    /// </summary>
    public Dictionary<int, MonthlyStats> GetMonthlyStatistics(SolarData data)
    {
        var monthlyStats = new Dictionary<int, MonthlyStats>();
        
        // Group by month (assuming day of year)
        var grouped = data.Year2023.GroupBy(d => (d.Day - 1) / 30 + 1); // Rough month approximation
        
        foreach (var monthGroup in grouped)
        {
            var month = monthGroup.Key;
            var days = monthGroup.ToList();
            
            monthlyStats[month] = new MonthlyStats(
                month,
                days.Count,
                days.Sum(d => d.TotalProduction),
                days.Sum(d => d.TotalConsumption),
                days.Sum(d => d.GridInjection),
                days.Average(d => d.WeatherStats.AverageTemp),
                days.Sum(d => d.WeatherStats.Precipitation),
                days.Sum(d => d.WeatherStats.SunshineHours),
                days.Count(d => d.AnomalyStats.HasAnomaly)
            );
        }
        
        return monthlyStats;
    }

    /// <summary>
    /// Analyzes weather vs production correlation
    /// </summary>
    public WeatherCorrelationAnalysis AnalyzeWeatherCorrelation(SolarData data)
    {
        var validDays = data.Year2023.Where(d => d.TotalProduction > 0).ToList();
        
        if (validDays.Count < 2) return new WeatherCorrelationAnalysis(0, 0, 0, 0);
        
        // Simple correlation calculation
        var sunshineCorrelation = CalculateCorrelation(
            validDays.Select(d => d.WeatherStats.SunshineHours).ToArray(),
            validDays.Select(d => d.TotalProduction).ToArray()
        );
        
        var temperatureCorrelation = CalculateCorrelation(
            validDays.Select(d => d.WeatherStats.AverageTemp).ToArray(),
            validDays.Select(d => d.TotalProduction).ToArray()
        );
        
        var precipitationCorrelation = CalculateCorrelation(
            validDays.Select(d => d.WeatherStats.Precipitation).ToArray(),
            validDays.Select(d => d.TotalProduction).ToArray()
        );
        
        var windCorrelation = CalculateCorrelation(
            validDays.Select(d => d.WeatherStats.WindSpeed).ToArray(),
            validDays.Select(d => d.TotalProduction).ToArray()
        );
        
        return new WeatherCorrelationAnalysis(sunshineCorrelation, temperatureCorrelation, precipitationCorrelation, windCorrelation);
    }
    
    private static double CalculateCorrelation(double[] x, double[] y)
    {
        if (x.Length != y.Length || x.Length == 0) return 0;
        
        var meanX = x.Average();
        var meanY = y.Average();
        
        var numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        var denominator = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)) * y.Sum(yi => Math.Pow(yi - meanY, 2)));
        
        return denominator == 0 ? 0 : numerator / denominator;
    }
}

/// <summary>
/// Monthly statistics summary
/// </summary>
public record MonthlyStats(
    int Month,
    int DaysWithData,
    double TotalProduction,
    double TotalConsumption,
    double TotalInjection,
    double AverageTemperature,
    double TotalPrecipitation,
    double TotalSunshineHours,
    int AnomalyCount
)
{
    public double AverageProductionPerDay => DaysWithData > 0 ? TotalProduction / DaysWithData : 0;
    public double AverageConsumptionPerDay => DaysWithData > 0 ? TotalConsumption / DaysWithData : 0;
    public double EnergyBalance => TotalProduction - TotalConsumption;
    public double AnomalyRate => DaysWithData > 0 ? (double)AnomalyCount / DaysWithData * 100 : 0;
}

/// <summary>
/// Weather correlation analysis results
/// </summary>
public record WeatherCorrelationAnalysis(
    double SunshineCorrelation,
    double TemperatureCorrelation,
    double PrecipitationCorrelation,
    double WindCorrelation
)
{
    public string GetStrongestCorrelation()
    {
        var correlations = new Dictionary<string, double>
        {
            ["Sunshine Hours"] = Math.Abs(SunshineCorrelation),
            ["Temperature"] = Math.Abs(TemperatureCorrelation),
            ["Precipitation"] = Math.Abs(PrecipitationCorrelation),
            ["Wind Speed"] = Math.Abs(WindCorrelation)
        };
        
        return correlations.OrderByDescending(kvp => kvp.Value).First().Key;
    }
};
