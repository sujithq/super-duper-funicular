using System.Text.Json;
using System.Text.Json.Serialization;
using SolarScope.Models;

namespace SolarScope.Services;

/// <summary>
/// Custom JSON converter for SolarData to handle string to int key conversion
/// </summary>
public class SolarDataConverter : JsonConverter<SolarData>
{
    public override SolarData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var result = new SolarData();
        
        foreach (var property in jsonDocument.RootElement.EnumerateObject())
        {
            if (int.TryParse(property.Name, out var year))
            {
                var yearData = JsonSerializer.Deserialize<List<BarChartData>>(property.Value.GetRawText(), options);
                if (yearData != null)
                {
                    result[year] = yearData;
                }
            }
        }
        
        return result;
    }

    public override void Write(Utf8JsonWriter writer, SolarData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }
        writer.WriteEndObject();
    }
}

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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new SolarDataConverter() }
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
    /// Gets data for a specific date range from a specific year
    /// </summary>
    public List<BarChartData> GetDataForDateRange(SolarData data, int year, int startDay, int endDay)
    {
        if (!data.ContainsKey(year))
            return new List<BarChartData>();
            
        return data[year]
            .Where(d => d.D >= startDay && d.D <= endDay)
            .OrderBy(d => d.D)
            .ToList();
    }

    /// <summary>
    /// Gets data for a specific date range (defaults to first available year)
    /// </summary>
    public List<BarChartData> GetDataForDateRange(SolarData data, int startDay, int endDay)
    {
        var firstYear = data.AvailableYears.FirstOrDefault();
        return GetDataForDateRange(data, firstYear, startDay, endDay);
    }

    /// <summary>
    /// Gets anomaly data above a certain threshold for a specific year
    /// </summary>
    public List<BarChartData> GetAnomalousData(SolarData data, int year, AnomalySeverity minSeverity = AnomalySeverity.Low)
    {
        if (!data.ContainsKey(year))
            return new List<BarChartData>();
            
        return data[year]
            .Where(d => d.AS.Severity >= minSeverity)
            .OrderByDescending(d => d.AS.TotalAnomalyScore)
            .ToList();
    }

    /// <summary>
    /// Gets anomaly data above a certain threshold (across all years)
    /// </summary>
    public List<BarChartData> GetAnomalousData(SolarData data, AnomalySeverity minSeverity = AnomalySeverity.Low)
    {
        return data.Values
            .SelectMany(yearData => yearData)
            .Where(d => d.AS.Severity >= minSeverity)
            .OrderByDescending(d => d.AS.TotalAnomalyScore)
            .ToList();
    }

    /// <summary>
    /// Gets days with the best solar production for a specific year
    /// </summary>
    public List<BarChartData> GetTopProductionDays(SolarData data, int year, int count = 10)
    {
        if (!data.ContainsKey(year))
            return new List<BarChartData>();
            
        return data[year]
            .OrderByDescending(d => d.P)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets days with the best solar production (across all years)
    /// </summary>
    public List<BarChartData> GetTopProductionDays(SolarData data, int count = 10)
    {
        return data.Values
            .SelectMany(yearData => yearData)
            .OrderByDescending(d => d.P)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets days with the worst weather conditions for a specific year
    /// </summary>
    public List<BarChartData> GetWorstWeatherDays(SolarData data, int year, int count = 10)
    {
        if (!data.ContainsKey(year))
            return new List<BarChartData>();
            
        return data[year]
            .OrderBy(d => d.MS.SunshineHours)
            .ThenByDescending(d => d.MS.Precipitation)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets days with the worst weather conditions (across all years)
    /// </summary>
    public List<BarChartData> GetWorstWeatherDays(SolarData data, int count = 10)
    {
        return data.Values
            .SelectMany(yearData => yearData)
            .OrderBy(d => d.MS.SunshineHours)
            .ThenByDescending(d => d.MS.Precipitation)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Calculates monthly statistics for a specific year
    /// </summary>
    public Dictionary<int, MonthlyStats> GetMonthlyStatistics(SolarData data, int year)
    {
        if (!data.ContainsKey(year))
            return new Dictionary<int, MonthlyStats>();
            
        var monthlyStats = new Dictionary<int, MonthlyStats>();
        
        // Group by month (assuming day of year)
        var grouped = data[year].GroupBy(d => (d.D - 1) / 30 + 1); // Rough month approximation
        
        foreach (var monthGroup in grouped)
        {
            var month = monthGroup.Key;
            var days = monthGroup.ToList();
            
            monthlyStats[month] = new MonthlyStats(
                month,
                days.Count,
                days.Sum(d => d.P),
                days.Sum(d => d.U),
                days.Sum(d => d.I),
                days.Average(d => d.MS.AverageTemp),
                days.Sum(d => d.MS.Precipitation),
                days.Sum(d => d.MS.SunshineHours),
                days.Count(d => d.AS.HasAnomaly)
            );
        }
        
        return monthlyStats;
    }

    /// <summary>
    /// Calculates monthly statistics (defaults to first available year)
    /// </summary>
    public Dictionary<int, MonthlyStats> GetMonthlyStatistics(SolarData data)
    {
        var firstYear = data.AvailableYears.FirstOrDefault();
        return GetMonthlyStatistics(data, firstYear);
    }

    /// <summary>
    /// Analyzes weather vs production correlation for a specific year
    /// </summary>
    public WeatherCorrelationAnalysis AnalyzeWeatherCorrelation(SolarData data, int year)
    {
        if (!data.ContainsKey(year))
            return new WeatherCorrelationAnalysis(0, 0, 0, 0);
            
        var validDays = data[year].Where(d => d.P > 0).ToList();
        
        if (validDays.Count < 2) return new WeatherCorrelationAnalysis(0, 0, 0, 0);
        
        // Simple correlation calculation
        var sunshineCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.SunshineHours).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        var temperatureCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.AverageTemp).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        var precipitationCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.Precipitation).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        var windCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.WindSpeed).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        return new WeatherCorrelationAnalysis(sunshineCorrelation, temperatureCorrelation, precipitationCorrelation, windCorrelation);
    }

    /// <summary>
    /// Analyzes weather vs production correlation (across all years)
    /// </summary>
    public WeatherCorrelationAnalysis AnalyzeWeatherCorrelation(SolarData data)
    {
        var validDays = data.Values
            .SelectMany(yearData => yearData)
            .Where(d => d.P > 0)
            .ToList();
        
        if (validDays.Count < 2) return new WeatherCorrelationAnalysis(0, 0, 0, 0);
        
        // Simple correlation calculation
        var sunshineCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.SunshineHours).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        var temperatureCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.AverageTemp).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        var precipitationCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.Precipitation).ToArray(),
            validDays.Select(d => d.P).ToArray()
        );
        
        var windCorrelation = CalculateCorrelation(
            validDays.Select(d => d.MS.WindSpeed).ToArray(),
            validDays.Select(d => d.P).ToArray()
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
