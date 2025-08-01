using System.Text.Json.Serialization;

namespace SolarScope.Models;

/// <summary>
/// Represents a daily solar system and weather data entry
/// </summary>
public record BarChartData(
    [property: JsonPropertyName("D")] int D,
    [property: JsonPropertyName("P")] double P,
    [property: JsonPropertyName("U")] double U,
    [property: JsonPropertyName("I")] double I,
    [property: JsonPropertyName("J")] bool J,
    [property: JsonPropertyName("S")] bool S,
    [property: JsonPropertyName("MS")] MeteoStatData MS,
    [property: JsonPropertyName("M")] bool M,
    [property: JsonPropertyName("AS")] AnomalyData AS,
    [property: JsonPropertyName("Q")] QuarterData Q,
    [property: JsonPropertyName("C")] bool C = false,
    [property: JsonPropertyName("SRS")] SunRiseSet SRS = default!
)
{
    /// <summary>
    /// Calculates the energy efficiency as a percentage
    /// </summary>
    public double Efficiency => P > 0 ? (U / P) * 100 : 0;
    
    /// <summary>
    /// Calculates the energy surplus/deficit
    /// </summary>
    public double EnergyBalance => P - U;
    
    /// <summary>
    /// Determines if this day had net energy production (surplus)
    /// </summary>
    public bool IsEnergyPositive => EnergyBalance > 0;
    
    /// <summary>
    /// Gets the peak quarter-hourly production value
    /// </summary>
    public double PeakProduction => Q.ProductionReadings.Count > 0 ? Q.ProductionReadings.Max() : 0;
    
    /// <summary>
    /// Gets the average quarter-hourly production
    /// </summary>
    public double AverageProduction => Q.ProductionReadings.Count > 0 ? Q.ProductionReadings.Average() : 0;
};

/// <summary>
/// Represents a daily solar system and weather data entry with year information
/// </summary>
public record BarChartDataWithYear(int Year, BarChartData Data)
{
    /// <summary>
    /// Day of year from the underlying data
    /// </summary>
    public int D => Data.D;
    
    /// <summary>
    /// Production from the underlying data
    /// </summary>
    public double P => Data.P;
    
    /// <summary>
    /// Consumption from the underlying data
    /// </summary>
    public double U => Data.U;
    
    /// <summary>
    /// Injection from the underlying data
    /// </summary>
    public double I => Data.I;
    
    /// <summary>
    /// Meteo data from the underlying data
    /// </summary>
    public MeteoStatData MS => Data.MS;
    
    /// <summary>
    /// Anomaly data from the underlying data
    /// </summary>
    public AnomalyData AS => Data.AS;
    
    /// <summary>
    /// Quarter data from the underlying data
    /// </summary>
    public QuarterData Q => Data.Q;
    
    /// <summary>
    /// Sun rise/set data from the underlying data
    /// </summary>
    public SunRiseSet SRS => Data.SRS;
    
    /// <summary>
    /// Calculated properties from the underlying data
    /// </summary>
    public double Efficiency => Data.Efficiency;
    public double EnergyBalance => Data.EnergyBalance;
    public bool IsEnergyPositive => Data.IsEnergyPositive;
    public double PeakProduction => Data.PeakProduction;
    public double AverageProduction => Data.AverageProduction;
    
    /// <summary>
    /// Converts the day-of-year number to a user-friendly date string using the correct year
    /// </summary>
    public string FormattedDate
    {
        get
        {
            try
            {
                var date = new DateTime(Year, 1, 1).AddDays(D - 1);
                return date.ToString("MMM dd, yyyy", System.Globalization.CultureInfo.CurrentCulture);
            }
            catch
            {
                return $"Day {D}";
            }
        }
    }
    
    /// <summary>
    /// Gets the actual DateTime for this entry
    /// </summary>
    public DateTime Date
    {
        get
        {
            try
            {
                return new DateTime(Year, 1, 1).AddDays(D - 1);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
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
public class SolarData : Dictionary<int, List<BarChartData>>
{
    /// <summary>
    /// Gets data for a specific year
    /// </summary>
    public List<BarChartData> GetYearData(int year) => this.ContainsKey(year) ? this[year] : new List<BarChartData>();
    
    /// <summary>
    /// Gets data for the most recent year available
    /// </summary>
    public List<BarChartData> GetLatestYearData() => AvailableYears.Count > 0 ? this[AvailableYears.Last()] : new List<BarChartData>();
    
    /// <summary>
    /// Gets the most recent year available
    /// </summary>
    public int LatestYear => AvailableYears.Count > 0 ? AvailableYears.Last() : DateTime.Now.Year;
    
    /// <summary>
    /// Gets the total number of days with data across all years
    /// </summary>
    public int TotalDays => this.Values.Sum(yearData => yearData.Count);
    
    /// <summary>
    /// Calculates yearly totals for a specific year
    /// </summary>
    public (double Production, double Consumption, double Injection) GetYearlyTotals(int year)
    {
        if (!this.ContainsKey(year))
            return (0, 0, 0);
            
        var yearData = this[year];
        return (
            yearData.Sum(d => d.P),
            yearData.Sum(d => d.U),
            yearData.Sum(d => d.I/1000)
        );
    }
    
    /// <summary>
    /// Calculates totals for all years combined
    /// </summary>
    public (double Production, double Consumption, double Injection) YearlyTotals =>
        (
            this.Values.SelectMany(yearData => yearData).Sum(d => d.P),
            this.Values.SelectMany(yearData => yearData).Sum(d => d.U),
            this.Values.SelectMany(yearData => yearData).Sum(d => d.I/1000)
        );
    
    /// <summary>
    /// Gets available years as a list
    /// </summary>
    public List<int> AvailableYears => this.Keys.OrderBy(y => y).ToList();
    
    /// <summary>
    /// Gets all data across all years with year information included
    /// </summary>
    public List<BarChartDataWithYear> GetAllDataWithYear()
    {
        return this.SelectMany(kvp => kvp.Value.Select(data => new BarChartDataWithYear(kvp.Key, data)))
                   .ToList();
    }
    
    /// <summary>
    /// Gets data for a specific year with year information included
    /// </summary>
    public List<BarChartDataWithYear> GetYearDataWithYear(int year)
    {
        if (!this.ContainsKey(year))
            return new List<BarChartDataWithYear>();
            
        return this[year].Select(data => new BarChartDataWithYear(year, data)).ToList();
    }
    
    /// <summary>
    /// Gets data for the latest year with year information included
    /// </summary>
    public List<BarChartDataWithYear> GetLatestYearDataWithYear()
    {
        return GetYearDataWithYear(LatestYear);
    }
}