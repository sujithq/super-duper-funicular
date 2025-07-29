using CommandLine;

namespace SolarScope.Commands;

/// <summary>
/// Base options for all commands
/// </summary>
public class BaseOptions
{
    private const string DefaultDataUrl = "https://raw.githubusercontent.com/sujithq/myenergy/refs/heads/main/src/myenergy/wwwroot/Data/data.json";
    [Option('d', "data", Required = false, Default = DefaultDataUrl, HelpText = "Path or URL to the solar data JSON file")]
    public string DataFile { get; set; } = DefaultDataUrl;

    [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output")]
    public bool Verbose { get; set; }
}

/// <summary>
/// Dashboard command options
/// </summary>
[Verb("dashboard", HelpText = "Display the main solar system dashboard")]
public class DashboardOptions : BaseOptions
{
    [Option('f', "full", Required = false, Default = false, HelpText = "Show full dashboard with all charts")]
    public bool FullDashboard { get; set; }

    [Option('a', "animated", Required = false, Default = false, HelpText = "Enable animated charts")]
    public bool Animated { get; set; }
}

/// <summary>
/// Analysis command options
/// </summary>
[Verb("analyze", HelpText = "Perform detailed analysis of solar data")]
public class AnalyzeOptions : BaseOptions
{
    [Option('t', "type", Required = false, Default = "production", HelpText = "Analysis type: production, weather, anomalies, correlation")]
    public string AnalysisType { get; set; } = "production";

    [Option('s', "start-day", Required = false, HelpText = "Start day for analysis (day of year)")]
    public int? StartDay { get; set; }

    [Option('e', "end-day", Required = false, HelpText = "End day for analysis (day of year)")]
    public int? EndDay { get; set; }

    [Option('c', "count", Required = false, Default = 10, HelpText = "Number of results to show")]
    public int Count { get; set; } = 10;
}

/// <summary>
/// Report command options
/// </summary>
[Verb("report", HelpText = "Generate detailed reports")]
public class ReportOptions : BaseOptions
{
    [Option('p', "period", Required = false, Default = "monthly", HelpText = "Report period: daily, weekly, monthly, yearly")]
    public string Period { get; set; } = "monthly";

    [Option('t', "type", Required = false, Default = "monthly", HelpText = "Report type: daily, weekly, monthly, yearly")]
    public string ReportType { get; set; } = "monthly";

    [Option('y', "year", Required = false, HelpText = "Year to analyze (default: latest available year)")]
    public int? Year { get; set; }

    [Option('s', "start-day", Required = false, HelpText = "Start day for analysis (day of year)")]
    public int? StartDay { get; set; }

    [Option('e', "end-day", Required = false, HelpText = "End day for analysis (day of year)")]
    public int? EndDay { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output file path (optional)")]
    public string? OutputFile { get; set; }

    [Option('f', "format", Required = false, Default = "table", HelpText = "Output format: table, json, csv")]
    public string Format { get; set; } = "table";
}

/// <summary>
/// Anomalies command options
/// </summary>
[Verb("anomalies", HelpText = "Detect and display system anomalies")]
public class AnomaliesOptions : BaseOptions
{
    [Option('s', "severity", Required = false, Default = "low", HelpText = "Minimum severity: none, low, medium, high")]
    public string Severity { get; set; } = "low";

    [Option('i', "interactive", Required = false, Default = false, HelpText = "Interactive anomaly explorer")]
    public bool Interactive { get; set; }

    [Option('y', "year", Required = false, HelpText = "Year to analyze (default: latest available year)")]
    public int? Year { get; set; }
}

/// <summary>
/// Weather command options
/// </summary>
[Verb("weather", HelpText = "Weather analysis and correlation")]
public class WeatherOptions : BaseOptions
{
    [Option('a', "analysis", Required = false, Default = "overview", HelpText = "Analysis type: overview, correlation, patterns, recommendations")]
    public string Analysis { get; set; } = "overview";

    [Option('y', "year", Required = false, HelpText = "Year to analyze (default: latest available year)")]
    public int? Year { get; set; }

    [Option('c', "correlation", Required = false, Default = false, HelpText = "Show weather-production correlation")]
    public bool ShowCorrelation { get; set; }

    [Option('h', "historical", Required = false, Default = false, HelpText = "Show historical weather patterns")]
    public bool Historical { get; set; }
}

/// <summary>
/// Interactive explorer command options
/// </summary>
[Verb("explore", HelpText = "Interactive data exploration")]
public class ExploreOptions : BaseOptions
{
    [Option('m', "mode", Required = false, Default = "full", HelpText = "Exploration mode: full, quick, guided")]
    public string Mode { get; set; } = "full";

    [Option('y', "year", Required = false, HelpText = "Year to analyze (default: latest available year)")]
    public int? Year { get; set; }
}

/// <summary>
/// Fun/demo command options
/// </summary>
[Verb("demo", HelpText = "Show demo with animations and fun features")]
public class DemoOptions : BaseOptions
{
    [Option('s', "speed", Required = false, Default = "normal", HelpText = "Animation speed: slow, normal, fast")]
    public string Speed { get; set; } = "normal";

    [Option('t', "theme", Required = false, Default = "solar", HelpText = "Display theme: solar, matrix, rainbow")]
    public string Theme { get; set; } = "solar";
}
