    using System.Text.Json.Serialization;

namespace SolarScope.Models;

/// <summary>
/// Represents a daily solar system and weather data entry
/// </summary>
public record BarChartData(
    [property: JsonPropertyName("D")] int Day,
    [property: JsonPropertyName("P")] double TotalProduction,
    [property: JsonPropertyName("U")] double TotalConsumption,
    [property: JsonPropertyName("I")] double GridInjection,
    [property: JsonPropertyName("J")] bool JuneEnergyProcessed,
    [property: JsonPropertyName("S")] bool SunGrowProcessed,
    [property: JsonPropertyName("MS")] MeteoStatData WeatherStats,
    [property: JsonPropertyName("M")] bool MeteoStatProcessed,
    [property: JsonPropertyName("AS")] AnomalyData AnomalyStats,
    [property: JsonPropertyName("Q")] QuarterData QuarterlyData,
    [property: JsonPropertyName("C")] bool IsComplete = false,
    [property: JsonPropertyName("SRS")] SunRiseSet SunTimes = default!
)
{
    /// <summary>
    /// Calculates the energy efficiency as a percentage
    /// </summary>
    public double Efficiency => TotalProduction > 0 ? (TotalConsumption / TotalProduction) * 100 : 0;
    
    /// <summary>
    /// Calculates the energy surplus/deficit
    /// </summary>
    public double EnergyBalance => TotalProduction - TotalConsumption;
    
    /// <summary>
    /// Determines if this day had net energy production (surplus)
    /// </summary>
    public bool IsEnergyPositive => EnergyBalance > 0;
    
    /// <summary>
    /// Gets the peak quarter-hourly production value
    /// </summary>
    public double PeakProduction => QuarterlyData.ConsumptionReadings.Count > 0 ? QuarterlyData.ConsumptionReadings.Max() : 0;
    
    /// <summary>
    /// Gets the average quarter-hourly production
    /// </summary>
    public double AverageProduction => QuarterlyData.ConsumptionReadings.Count > 0 ? QuarterlyData.ConsumptionReadings.Average() : 0;
};

/// <summary>
/// Represents anomaly detection data
/// </summary>
public record AnomalyData(
    [property: JsonPropertyName("P")] double ProductionAnomaly,
    [property: JsonPropertyName("U")] double ConsumptionAnomaly,
    [property: JsonPropertyName("I")] double InjectionAnomaly,
    [property: JsonPropertyName("A")] bool HasAnomaly
)
{
    /// <summary>
    /// Gets the total anomaly score
    /// </summary>
    public double TotalAnomalyScore => Math.Abs(ProductionAnomaly) + Math.Abs(ConsumptionAnomaly) + Math.Abs(InjectionAnomaly);
    
    /// <summary>
    /// Determines the anomaly severity level
    /// </summary>
    public AnomalySeverity Severity => TotalAnomalyScore switch
    {
        < 1.0 => AnomalySeverity.None,
        < 5.0 => AnomalySeverity.Low,
        < 10.0 => AnomalySeverity.Medium,
        _ => AnomalySeverity.High
    };
};

/// <summary>
/// Anomaly severity levels
/// </summary>
public enum AnomalySeverity
{
    None,
    Low,
    Medium,
    High
}

/// <summary>
/// Represents anomaly data for modal analysis
/// </summary>
public record AnomalyModalData(
    [property: JsonPropertyName("Y")] int Year,
    [property: JsonPropertyName("D")] int Day,
    [property: JsonPropertyName("A")] AnomalyData Anomaly
);

/// <summary>
/// Represents quarter-hourly measurement data
/// </summary>
public record QuarterData(
    [property: JsonPropertyName("C")] List<double> ConsumptionReadings,
    [property: JsonPropertyName("I")] List<double> InjectionReadings,
    [property: JsonPropertyName("G")] List<double> GenerationReadings,
    [property: JsonPropertyName("P")] List<double> ProductionReadings,
    [property: JsonPropertyName("WRT")] List<double>? WaterReturnTemp,
    [property: JsonPropertyName("WOT")] List<double>? WaterOutletTemp,
    [property: JsonPropertyName("WP")] List<double>? WaterPressure
)
{
    /// <summary>
    /// Gets the total number of readings
    /// </summary>
    public int TotalReadings => ConsumptionReadings.Count;
    
    /// <summary>
    /// Gets the time span covered by readings (in hours)
    /// </summary>
    public double TimeSpanHours => TotalReadings * 0.25; // Each reading is 15 minutes
    
    /// <summary>
    /// Calculates peak demand time (hour of day)
    /// </summary>
    public double PeakDemandHour
    {
        get
        {
            if (ConsumptionReadings.Count == 0) return 0;
            var maxIndex = ConsumptionReadings.IndexOf(ConsumptionReadings.Max());
            return maxIndex * 0.25; // Convert to hour of day
        }
    }
    
    /// <summary>
    /// Calculates peak generation time (hour of day)
    /// </summary>
    public double PeakGenerationHour
    {
        get
        {
            if (GenerationReadings.Count == 0) return 0;
            var maxIndex = GenerationReadings.IndexOf(GenerationReadings.Max());
            return maxIndex * 0.25; // Convert to hour of day
        }
    }
};

/// <summary>
/// Represents sunrise and sunset times
/// </summary>
public record SunRiseSet(
    [property: JsonPropertyName("R")] DateTime Sunrise,
    [property: JsonPropertyName("S")] DateTime Sunset
)
{
    /// <summary>
    /// Calculates the daylight duration in hours
    /// </summary>
    public double DaylightHours => (Sunset - Sunrise).TotalHours;
    
    /// <summary>
    /// Determines if it's currently daylight
    /// </summary>
    public bool IsDaylight(DateTime currentTime) => currentTime >= Sunrise && currentTime <= Sunset;
};

/// <summary>
/// Represents weather statistical data
/// </summary>
public record MeteoStatData(
    [property: JsonPropertyName("tavg")] double AverageTemp,
    [property: JsonPropertyName("tmin")] double MinTemp,
    [property: JsonPropertyName("tmax")] double MaxTemp,
    [property: JsonPropertyName("prcp")] double Precipitation,
    [property: JsonPropertyName("snow")] double Snow,
    [property: JsonPropertyName("wdir")] double WindDirection,
    [property: JsonPropertyName("wspd")] double WindSpeed,
    [property: JsonPropertyName("wpgt")] double WindPeakGust,
    [property: JsonPropertyName("pres")] double Pressure,
    [property: JsonPropertyName("tsun")] double SunshineHours
)
{
    /// <summary>
    /// Temperature range for the day
    /// </summary>
    public double TemperatureRange => MaxTemp - MinTemp;
    
    /// <summary>
    /// Weather condition based on precipitation and sunshine
    /// </summary>
    public WeatherCondition Condition => (Precipitation, SunshineHours) switch
    {
        (> 5, _) => WeatherCondition.Rainy,
        (> 0, < 2) => WeatherCondition.Overcast,
        (0, > 8) => WeatherCondition.Sunny,
        (0, > 4) => WeatherCondition.PartlyCloudy,
        _ => WeatherCondition.Cloudy
    };
    
    /// <summary>
    /// Wind classification based on speed
    /// </summary>
    public WindCondition WindCondition => WindSpeed switch
    {
        < 5 => WindCondition.Calm,
        < 15 => WindCondition.Light,
        < 25 => WindCondition.Moderate,
        < 35 => WindCondition.Strong,
        _ => WindCondition.Gale
    };
};

/// <summary>
/// Weather condition classifications
/// </summary>
public enum WeatherCondition
{
    Sunny,
    PartlyCloudy,
    Cloudy,
    Overcast,
    Rainy
}

/// <summary>
/// Wind condition classifications
/// </summary>
public enum WindCondition
{
    Calm,
    Light,
    Moderate,
    Strong,
    Gale
}

/// <summary>
/// Root data structure for yearly solar data
/// </summary>
public record SolarData(
    [property: JsonPropertyName("years")] Dictionary<string, List<BarChartData>> Years
)
{
    /// <summary>
    /// Gets all available years of data as integer keys
    /// </summary>
    public Dictionary<int, List<BarChartData>> AllYears => 
        Years.ToDictionary(kvp => int.Parse(kvp.Key), kvp => kvp.Value);
    
    /// <summary>
    /// Gets data for 2023 (for backward compatibility)
    /// </summary>
    public List<BarChartData> Year2023 => Years.ContainsKey("2023") ? Years["2023"] : new List<BarChartData>();
    
    /// <summary>
    /// Gets the total number of days with data across all years
    /// </summary>
    public int TotalDays => Years.Values.Sum(yearData => yearData.Count);
    
    /// <summary>
    /// Calculates yearly totals for a specific year
    /// </summary>
    public (double Production, double Consumption, double Injection) GetYearlyTotals(int year)
    {
        var yearKey = year.ToString();
        if (!Years.ContainsKey(yearKey))
            return (0, 0, 0);
            
        var yearData = Years[yearKey];
        return (
            yearData.Sum(d => d.TotalProduction),
            yearData.Sum(d => d.TotalConsumption),
            yearData.Sum(d => d.GridInjection)
        );
    }
    
    /// <summary>
    /// Calculates totals for all years combined
    /// </summary>
    public (double Production, double Consumption, double Injection) YearlyTotals =>
        (
            Years.Values.SelectMany(yearData => yearData).Sum(d => d.TotalProduction),
            Years.Values.SelectMany(yearData => yearData).Sum(d => d.TotalConsumption),
            Years.Values.SelectMany(yearData => yearData).Sum(d => d.GridInjection)
        );
    
    /// <summary>
    /// Gets available years as a list
    /// </summary>
    public List<int> AvailableYears => Years.Keys.Select(int.Parse).OrderBy(y => y).ToList();
};