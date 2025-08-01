using Spectre.Console;
using Spectre.Console.Cli;
using SolarScope.Models;
using SolarScope.Services;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SolarScope.Commands;

/// <summary>
/// Analysis command implementation (Spectre.Console.Cli)
/// </summary>
public class AnalyzeCommand : AsyncCommand<AnalyzeCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("--type|-t")]
        [Description("Type of analysis: production, weather, anomalies, correlation")]
        public string AnalysisType { get; set; } = "production";

        [CommandOption("--count|-c")]
        [Description("Number of days/records to analyze")]
        [DefaultValue(10)]
        public int Count { get; set; } = 10;
    }

    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "production", "weather", "anomalies", "correlation"
    };

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!ValidTypes.Contains(settings.AnalysisType))
        {
            return ValidationResult.Error($"Invalid analysis type: '{settings.AnalysisType}'. Valid types are: production, weather, anomalies, correlation.");
        }
        return base.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var dataService = new SolarDataService(settings.DataFile);
        var data = await dataService.LoadDataAsync(settings.Verbose);

        if (data == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return 1;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Clock)
            .StartAsync($"Analyzing {settings.AnalysisType} data...", async ctx =>
            {
                await Task.Delay(800); // Dramatic effect
            });

        AnsiConsole.Clear();

        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine("[dim]Verbose mode enabled[/]");
            AnsiConsole.MarkupLine($"[dim]Data file: {settings.DataFile}[/]");
            AnsiConsole.WriteLine();
        }

        var rule = new Rule($"[bold cyan]📊 {settings.AnalysisType.ToUpper()} ANALYSIS 📊[/]")
        {
            Style = Style.Parse("cyan"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        switch (settings.AnalysisType.ToLower())
        {
            case "production":
                await AnalyzeProduction(dataService, data, settings);
                break;
            case "weather":
                await AnalyzeWeather(dataService, data, settings);
                break;
            case "anomalies":
                await AnalyzeAnomalies(dataService, data, settings);
                break;
            case "correlation":
                await AnalyzeCorrelation(dataService, data, settings);
                break;
            default:
                AnsiConsole.MarkupLine($"[red]Unknown analysis type: {settings.AnalysisType}[/]");
                AnsiConsole.MarkupLine("[yellow]Available types: production, weather, anomalies, correlation[/]");
                return 1;
        }
        return 0;
    }

    private async Task AnalyzeProduction(SolarDataService dataService, SolarData data, Settings options)
    {
        await Task.Run(() =>
        {
            var topDays = dataService.GetTopProductionDaysWithYear(data, options.Count);

            // Production trends chart
            var chart = new BarChart()
                .Width(80)
                .Label($"[green bold]Top {options.Count} Production Days[/]")
                .CenterLabel();

            foreach (var day in topDays)
            {
                var color = day.P switch
                {
                    > 20 => Color.Green,
                    > 15 => Color.Lime,
                    > 10 => Color.Yellow,
                    _ => Color.Orange1
                };

                chart.AddItem(day.FormattedDate, day.P, color);
            }

            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();

            // Detailed table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green);

            table.AddColumn("[bold]Date[/]");
            table.AddColumn("[bold]Production (kWh)[/]");
            table.AddColumn("[bold]Consumption (kWh)[/]");
            table.AddColumn("[bold]Balance[/]");
            table.AddColumn("[bold]Efficiency[/]");
            table.AddColumn("[bold]Weather[/]");

            foreach (var day in topDays)
            {
                var balance = day.EnergyBalance;
                var balanceColor = balance >= 0 ? "green" : "red";
                var balanceText = $"[{balanceColor}]{balance:+0.00;-0.00} kWh[/]";

                table.AddRow(
                    day.FormattedDate,
                    $"[green]{day.P:F2}[/]",
                    $"[blue]{day.U:F2}[/]",
                    balanceText,
                    $"[yellow]{day.Efficiency:F2}%[/]",
                    $"[cyan]{GetWeatherEmoji(day.MS.Condition)}[/]"
                );
            }

            var panel = new Panel(table)
            {
                Header = new PanelHeader("[bold]Production Analysis Details[/]"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);

            // Statistics summary
            DisplayProductionStatistics(topDays);
        });

    }

    private async Task AnalyzeWeather(SolarDataService dataService, SolarData data, Settings options)
    {
        await Task.Run(() =>
        {
            var worstWeatherDays = dataService.GetWorstWeatherDaysWithYear(data, options.Count);

            // Weather conditions breakdown
            var weatherBreakdown = new BreakdownChart()
                .Width(50)
                .ShowPercentage();

            var conditionGroups = data.GetLatestYearData()
                .GroupBy(d => d.MS.Condition)
                .OrderByDescending(g => g.Count());

            foreach (var group in conditionGroups)
            {
                var color = group.Key switch
                {
                    WeatherCondition.Sunny => Color.Yellow,
                    WeatherCondition.PartlyCloudy => Color.Orange1,
                    WeatherCondition.Cloudy => Color.Grey,
                    WeatherCondition.Overcast => Color.DarkGreen,
                    WeatherCondition.Rainy => Color.Blue,
                    _ => Color.White
                };

                weatherBreakdown.AddItem(group.Key.ToString(), group.Count(), color);
            }

            var breakdownPanel = new Panel(weatherBreakdown)
            {
                Header = new PanelHeader("[bold]Weather Conditions Distribution[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("blue")
            };

            AnsiConsole.Write(breakdownPanel);
            AnsiConsole.WriteLine();

            // Worst weather days table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue);

            table.AddColumn("[bold]Date[/]");
            table.AddColumn("[bold]Sunshine (h)[/]");
            table.AddColumn("[bold]Precipitation (mm)[/]");
            table.AddColumn("[bold]Temp (°C)[/]");
            table.AddColumn("[bold]Wind (km/h)[/]");
            table.AddColumn("[bold]Production Impact[/]");

            foreach (var day in worstWeatherDays)
            {
                var productionImpact = day.P < 5 ? "[red]High Impact[/]" :
                                     day.P < 10 ? "[yellow]Medium Impact[/]" : "[green]Low Impact[/]";

                table.AddRow(
                    day.FormattedDate,
                    $"{day.MS.SunshineHours:F2}",
                    $"{day.MS.Precipitation:F2}",
                    $"{day.MS.AverageTemp:F2}",
                    $"{day.MS.WindSpeed:F2}",
                    productionImpact
                );
            }

            var weatherPanel = new Panel(table)
            {
                Header = new PanelHeader($"[bold]Challenging Weather Days (Top {options.Count})[/]"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(weatherPanel);

            // Weather statistics
            DisplayWeatherStatistics(data);
        });

    }

    private async Task AnalyzeAnomalies(SolarDataService dataService, SolarData data, Settings options)
    {
        await Task.Run(() =>
        {
            var anomalousData = dataService.GetAnomalousDataWithYear(data, AnomalySeverity.Low);

            if (anomalousData.Count == 0)
            {
                var noAnomaliesPanel = new Panel("[green]🎉 Excellent! No significant anomalies detected in your solar system.[/]")
                {
                    Header = new PanelHeader("[bold]Anomaly Analysis Results[/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("green")
                };
                AnsiConsole.Write(noAnomaliesPanel);
                return;
            }

            // Anomaly severity chart
            var severityChart = new BarChart()
                .Width(60)
                .Label("[red bold]Anomaly Severity Distribution[/]")
                .CenterLabel();

            var severityGroups = anomalousData.GroupBy(d => d.AS.Severity);
            foreach (var group in severityGroups)
            {
                var color = group.Key switch
                {
                    AnomalySeverity.High => Color.Red,
                    AnomalySeverity.Medium => Color.Orange1,
                    AnomalySeverity.Low => Color.Yellow,
                    _ => Color.Green
                };

                severityChart.AddItem(group.Key.ToString(), group.Count(), color);
            }

            AnsiConsole.Write(severityChart);
            AnsiConsole.WriteLine();

            // Detailed anomaly table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red);

            table.AddColumn("[bold]Date[/]");
            table.AddColumn("[bold]Severity[/]");
            table.AddColumn("[bold]Production Anomaly[/]");
            table.AddColumn("[bold]Consumption Anomaly[/]");
            table.AddColumn("[bold]Total Score[/]");
            table.AddColumn("[bold]Potential Cause[/]");

            foreach (var day in anomalousData.Take(options.Count))
            {
                var severityColor = day.AS.Severity switch
                {
                    AnomalySeverity.High => "red",
                    AnomalySeverity.Medium => "orange",
                    AnomalySeverity.Low => "yellow",
                    _ => "green"
                };

                table.AddRow(
                    day.FormattedDate,
                    $"[{severityColor}]{day.AS.Severity}[/]",
                    $"{day.AS.ProductionAnomaly:F2}",
                    $"{day.AS.ConsumptionAnomaly:F2}",
                    $"[bold]{day.AS.TotalAnomalyScore:F2}[/]",
                    GetPotentialCause(day)
                );
            }

            var anomalyPanel = new Panel(table)
            {
                Header = new PanelHeader($"[bold]Detected Anomalies (Top {Math.Min(options.Count, anomalousData.Count)})[/]"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(anomalyPanel);

            // Recommendations
            DisplayAnomalyRecommendations(anomalousData);
        });

    }

    private async Task AnalyzeCorrelation(SolarDataService dataService, SolarData data, Settings options)
    {
        await Task.Run(() =>
        {
            var correlation = dataService.AnalyzeWeatherCorrelation(data);

            // Correlation strength chart
            var correlations = new Dictionary<string, double>
            {
                ["Sunshine Hours"] = correlation.SunshineCorrelation,
                ["Temperature"] = correlation.TemperatureCorrelation,
                ["Precipitation"] = correlation.PrecipitationCorrelation,
                ["Wind Speed"] = correlation.WindCorrelation
            };

            var chart = new BarChart()
                .Width(70)
                .Label("[cyan bold]Weather vs Production Correlation[/]")
                .CenterLabel();

            foreach (var kvp in correlations.OrderByDescending(x => Math.Abs(x.Value)))
            {
                var color = Math.Abs(kvp.Value) switch
                {
                    > 0.7 => Color.Green,
                    > 0.5 => Color.Yellow,
                    > 0.3 => Color.Orange1,
                    _ => Color.Red
                };

                chart.AddItem(kvp.Key, Math.Abs(kvp.Value), color);
            }

            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();

            // Detailed correlation table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1);

            table.AddColumn("[bold]Weather Factor[/]");
            table.AddColumn("[bold]Correlation[/]");
            table.AddColumn("[bold]Strength[/]");
            table.AddColumn("[bold]Impact[/]");

            foreach (var kvp in correlations.OrderByDescending(x => Math.Abs(x.Value)))
            {
                var strength = Math.Abs(kvp.Value) switch
                {
                    > 0.7 => "[green]Strong[/]",
                    > 0.5 => "[yellow]Moderate[/]",
                    > 0.3 => "[orange1]Weak[/]",
                    _ => "[red]Very Weak[/]"
                };

                var impact = kvp.Value > 0 ? "[green]Positive[/]" : "[red]Negative[/]";

                table.AddRow(
                    kvp.Key,
                    $"{kvp.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}",
                    strength,
                    impact
                );
            }

            var correlationPanel = new Panel(table)
            {
                Header = new PanelHeader("[bold]Correlation Analysis Results[/]"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(correlationPanel);

            // Insights
            DisplayCorrelationInsights(correlation);
        });

    }

    private void DisplayProductionStatistics(List<BarChartDataWithYear> days)
    {
        AnsiConsole.WriteLine();

        var avgProduction = days.Average(d => d.P);
        var maxProduction = days.Max(d => d.P);
        var avgEfficiency = days.Average(d => d.Efficiency);

        var stats = new Panel(new Markup(
            $"[green]📈 Average Production: {avgProduction:F2} kWh[/]\n" +
            $"[yellow]🎯 Peak Production: {maxProduction:F2} kWh[/]\n" +
            $"[cyan]⚡ Average Efficiency: {avgEfficiency:F2}%[/]\n" +
            $"[blue]📊 Days Analyzed: {days.Count}[/]"))
        {
            Header = new PanelHeader("[bold]Production Statistics[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("green")
        };

        AnsiConsole.Write(stats);
    }

    private void DisplayWeatherStatistics(SolarData data)
    {
        AnsiConsole.WriteLine();

        var latestYearData = data.GetLatestYearData();
        var weatherData = latestYearData.Select(d => d.MS).ToList();
        var avgTemp = weatherData.Average(w => w.AverageTemp);
        var totalPrecip = weatherData.Sum(w => w.Precipitation);
        var totalSunshine = weatherData.Sum(w => w.SunshineHours);
        var sunnyDays = latestYearData.Count(d => d.MS.Condition == WeatherCondition.Sunny);

        var stats = new Panel(new Markup(
            $"[yellow]🌡 Average Temperature: {avgTemp:F2}°C[/]\n" +
            $"[blue]🌧 Total Precipitation: {totalPrecip:F2}mm[/]\n" +
            $"[orange1]☀ Total Sunshine: {totalSunshine:F2} hours[/]\n" +
            $"[green]🌞 Sunny Days: {sunnyDays} ({(double)sunnyDays / latestYearData.Count * 100:F2}%)[/]"))
        {
            Header = new PanelHeader("[bold]Weather Statistics[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue")
        };

        AnsiConsole.Write(stats);
    }

    private void DisplayAnomalyRecommendations(List<BarChartDataWithYear> anomalies)
    {
        AnsiConsole.WriteLine();

        var highSeverityCount = anomalies.Count(a => a.AS.Severity == AnomalySeverity.High);
        var recommendations = new List<string>();

        if (highSeverityCount > 0)
        {
            recommendations.Add("🔧 Schedule professional system inspection");
            recommendations.Add("📊 Monitor high-severity days for patterns");
        }

        recommendations.Add("📈 Consider weather correlation analysis");
        recommendations.Add("⚡ Review energy consumption patterns");
        recommendations.Add("🔍 Implement automated monitoring alerts");

        var recPanel = new Panel(string.Join("\n", recommendations.Select(r => $"• {r}")))
        {
            Header = new PanelHeader("[bold]Recommendations[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("yellow")
        };

        AnsiConsole.Write(recPanel);
    }

    private void DisplayCorrelationInsights(WeatherCorrelationAnalysis correlation)
    {
        AnsiConsole.WriteLine();

        var strongest = correlation.GetStrongestCorrelation();
        var insights = new List<string>
        {
            $"🎯 Strongest correlation: {strongest}",
            $"☀ Sunshine impact: {GetCorrelationDescription(correlation.SunshineCorrelation)}",
            $"🌡 Temperature impact: {GetCorrelationDescription(correlation.TemperatureCorrelation)}",
            $"🌧 Rain impact: {GetCorrelationDescription(correlation.PrecipitationCorrelation)}"
        };

        var insightPanel = new Panel(string.Join("\n", insights.Select(i => $"• {i}")))
        {
            Header = new PanelHeader("[bold]Key Insights[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.Write(insightPanel);
    }

    private static string GetWeatherSummary(MeteoStatData weather)
    {
        return $"{weather.Condition} ({weather.SunshineHours:F2}h ☀)";
    }

    private static string GetPotentialCause(BarChartDataWithYear day)
    {
        if (day.MS.Precipitation > 5) return "Heavy rain";
        if (day.MS.SunshineHours < 2) return "Low sunshine";
        if (day.AS.ConsumptionAnomaly > 5) return "High consumption";
        if (day.AS.ProductionAnomaly < -5) return "Low production";
        return "System issue";
    }

    private static string GetCorrelationDescription(double correlation)
    {
        var strength = Math.Abs(correlation) switch
        {
            > 0.7 => "Strong",
            > 0.5 => "Moderate",
            > 0.3 => "Weak",
            _ => "Very weak"
        };

        var direction = correlation > 0 ? "positive" : "negative";
        return $"{strength} {direction} correlation ({correlation:F3})";
    }

    private static string GetWeatherEmoji(WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "☀",
            WeatherCondition.PartlyCloudy => "⛅",
            WeatherCondition.Cloudy => "☁",
            WeatherCondition.Overcast => "🌫",
            WeatherCondition.Rainy => "🌧",
            _ => "❓"
        };
    }
}
