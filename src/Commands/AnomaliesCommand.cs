using Spectre.Console;
using Spectre.Console.Cli;
using SolarScope.Models;
using SolarScope.Services;
using System.ComponentModel;

namespace SolarScope.Commands;

/// <summary>
/// Anomalies command implementation (Spectre.Console.Cli)
/// </summary>
public class AnomaliesCommand : BaseCommand<AnomaliesCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("--year|-y")]
        [Description("Year to analyze (default: latest year in data)")]
        public int? Year { get; set; }

        [CommandOption("--severity|-s")]
        [Description("Minimum anomaly severity (Low, Medium, High)")]
        public string Severity { get; set; } = "Low";

        [CommandOption("--interactive|-i")]
        [Description("Run in interactive mode")]
        [DefaultValue(false)]
        public bool Interactive { get; set; }
    }

    private static readonly HashSet<string> ValidSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "Low", "Medium", "High"
    };

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!ValidSeverities.Contains(settings.Severity))
        {
            return ValidationResult.Error($"Invalid severity: '{settings.Severity}'. Valid values are: Low, Medium, High.");
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

        AnsiConsole.Clear();

        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine("[dim]Verbose mode enabled[/]");
            AnsiConsole.MarkupLine($"[dim]Data file: {settings.DataFile}[/]");
            AnsiConsole.WriteLine();
        }

        // Display header
        var headerRule = new Rule("[bold red]❗ Anomaly Detection Analysis ❗[/]")
        {
            Style = Style.Parse("red"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(headerRule);

        if (settings.Interactive)
        {
            await RunInteractiveMode(data, dataService, settings);
        }
        else
        {
            await DisplayAnomalyReport(data, dataService, settings);
        }
        return 0;
    }

    private async Task DisplayAnomalyReport(SolarData data, SolarDataService dataService, Settings options)
    {
        await Task.Run(() =>
        {
            var selectedYear = options.Year ?? data.LatestYear;
            var minSeverity = Enum.Parse<AnomalySeverity>(options.Severity, true);

            var anomalies = dataService.GetAnomalousData(data, selectedYear, minSeverity);

            if (!anomalies.Any())
            {
                AnsiConsole.MarkupLine($"[green]✅ No anomalies found with severity {minSeverity} or higher for year {selectedYear}![/]");
                return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Found {anomalies.Count} anomalies with severity {minSeverity} or higher in year {selectedYear}[/]");

            // Anomaly severity distribution
            var severityDistribution = anomalies
                .GroupBy(a => a.AS.Severity)
                .ToDictionary(g => g.Key, g => g.Count());

            var severityChart = new BarChart()
                .Width(80)
                .Label("[bold]Anomaly Severity Distribution[/]")
                .CenterLabel();

            foreach (var severity in Enum.GetValues<AnomalySeverity>())
            {
                if (severityDistribution.ContainsKey(severity))
                {
                    var color = severity switch
                    {
                        AnomalySeverity.High => Color.Red,
                        AnomalySeverity.Medium => Color.Orange1,
                        AnomalySeverity.Low => Color.Yellow,
                        _ => Color.Green
                    };

                    severityChart.AddItem(severity.ToString(), severityDistribution[severity], color);
                }
            }

            AnsiConsole.Write(severityChart);

            // Top anomalies table
            var anomaliesTable = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[bold]Top {Math.Min(20, anomalies.Count)} Anomalies - Year {selectedYear}[/]")
                .AddColumn("[yellow]Date[/]")
                .AddColumn("[red]Severity[/]")
                .AddColumn("[green]Production[/]")
                .AddColumn("[blue]Consumption[/]")
                .AddColumn("[orange1]Injection[/]")
                .AddColumn("[white]Score[/]")
                .AddColumn("[cyan]Weather[/]");

            foreach (var anomaly in anomalies.Take(20))
            {
                var severityColor = anomaly.AS.Severity switch
                {
                    AnomalySeverity.High => "red",
                    AnomalySeverity.Medium => "orange",
                    AnomalySeverity.Low => "yellow",
                    _ => "green"
                };

                anomaliesTable.AddRow(
                    GetDateFromDayOfYear(anomaly.D, selectedYear),
                    $"[{severityColor}]{anomaly.AS.Severity}[/]",
                    $"{anomaly.P:F2} kWh",
                    $"{anomaly.U:F2} kWh",
                    $"{(anomaly.I / 1000.0):F2} kWh",
                    $"{anomaly.AS.TotalAnomalyScore:F2}",
                    anomaly.MS.Condition.ToString()
                );
            }

            AnsiConsole.Write(anomaliesTable);

            // Anomaly analysis
            DisplayAnomalyAnalysis(anomalies);
        });
    }

    private void DisplayAnomalyAnalysis(List<BarChartData> anomalies)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]📊 Anomaly Pattern Analysis[/]");

        // Weather correlation
        var weatherPatterns = anomalies
            .GroupBy(a => a.MS.Condition)
            .Select(g => new { Condition = g.Key, Count = g.Count(), AvgScore = g.Average(a => a.AS.TotalAnomalyScore) })
            .OrderByDescending(wp => wp.Count)
            .ToList();

        var weatherTable = new Table()
            .Border(TableBorder.Minimal)
            .Title("[bold]Weather Pattern Correlation[/]")
            .AddColumn("[yellow]Weather[/]")
            .AddColumn("[white]Count[/]")
            .AddColumn("[red]Avg Score[/]");

        foreach (var pattern in weatherPatterns)
        {
            weatherTable.AddRow(
                pattern.Condition.ToString(),
                pattern.Count.ToString(),
                $"{pattern.AvgScore:F2}"
            );
        }

        AnsiConsole.Write(weatherTable);

        // Production anomaly insights
        var highProdAnomalies = anomalies.Where(a => a.AS.ProductionAnomaly > 0).Count();
        var lowProdAnomalies = anomalies.Where(a => a.AS.ProductionAnomaly < 0).Count();
        var highConsAnomalies = anomalies.Where(a => a.AS.ConsumptionAnomaly > 0).Count();
        var lowConsAnomalies = anomalies.Where(a => a.AS.ConsumptionAnomaly < 0).Count();

        var insightsPanel = new Panel(new Markup(
            $"[green]High Production Anomalies: {highProdAnomalies}[/]\n" +
            $"[red]Low Production Anomalies: {lowProdAnomalies}[/]\n" +
            $"[blue]High Consumption Anomalies: {highConsAnomalies}[/]\n" +
            $"[orange1]Low Consumption Anomalies: {lowConsAnomalies}[/]\n\n" +
            $"[yellow]Most Common Weather During Anomalies: {weatherPatterns.First().Condition}[/]\n" +
            $"[gray]Peak Anomaly Score: {anomalies.Max(a => a.AS.TotalAnomalyScore):F2}[/]"))
        {
            Header = new PanelHeader("[bold]Anomaly Insights[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.Write(insightsPanel);
    }

    private async Task RunInteractiveMode(SolarData data, SolarDataService dataService, Settings options)
    {
        var selectedYear = options.Year ?? data.LatestYear;
        var minSeverity = Enum.Parse<AnomalySeverity>(options.Severity, true);

        while (true)
        {
            AnsiConsole.Clear();
            var headerRule = new Rule($"[bold red]❗ Interactive Anomaly Explorer - Year {selectedYear} ❗[/]")
            {
                Style = Style.Parse("red"),
                Justification = Justify.Center
            };
            AnsiConsole.Write(headerRule);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to explore?[/]")
                    .AddChoices(new[]
                    {
                        "View All Anomalies",
                        "Filter by Severity",
                        "Analyze Weather Patterns",
                        "Inspect Specific D",
                        "Change Year",
                        "Exit"
                    }));

            switch (choice)
            {
                case "View All Anomalies":
                    await DisplayAnomalyReport(data, dataService, options);
                    break;

                case "Filter by Severity":
                    var severityChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<AnomalySeverity>()
                            .Title("[yellow]Select minimum severity level:[/]")
                            .AddChoices(Enum.GetValues<AnomalySeverity>()));
                    
                    var filteredAnomalies = dataService.GetAnomalousData(data, selectedYear, severityChoice);
                    await DisplayFilteredAnomalies(filteredAnomalies, severityChoice);
                    break;

                case "Analyze Weather Patterns":
                    await AnalyzeWeatherPatterns(data, selectedYear);
                    break;

                case "Inspect Specific D":
                    await InspectSpecificDay(data, selectedYear);
                    break;

                case "Change Year":
                    selectedYear = AnsiConsole.Prompt(
                        new SelectionPrompt<int>()
                            .Title("[yellow]Select year:[/]")
                            .AddChoices(data.AvailableYears));
                    continue;

                case "Exit":
                    return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[gray]Press any key to continue...[/]");
            Console.ReadKey();
        }
    }

    private async Task DisplayFilteredAnomalies(List<BarChartData> anomalies, AnomalySeverity minSeverity)
    {
        await Task.Run(() =>
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Showing {anomalies.Count} anomalies with severity {minSeverity} or higher[/]");

            if (!anomalies.Any())
            {
                AnsiConsole.MarkupLine("[green]No anomalies found at this severity level![/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[bold]Filtered Anomalies - Severity: {minSeverity}+[/]")
                .AddColumn("[yellow]D[/]")
                .AddColumn("[red]Severity[/]")
                .AddColumn("[white]Score[/]")
                .AddColumn("[green]Production[/]")
                .AddColumn("[cyan]Weather[/]");

            foreach (var anomaly in anomalies.Take(15))
            {
                var severityColor = anomaly.AS.Severity switch
                {
                    AnomalySeverity.High => "red",
                    AnomalySeverity.Medium => "orange",
                    AnomalySeverity.Low => "yellow",
                    _ => "green"
                };

                table.AddRow(
                    anomaly.D.ToString(),
                    $"[{severityColor}]{anomaly.AS.Severity}[/]",
                    $"{anomaly.AS.TotalAnomalyScore:F2}",
                    $"{anomaly.P:F2} kWh",
                    anomaly.MS.Condition.ToString()
                );
            }

            AnsiConsole.Write(table);
        });
    }

    private async Task AnalyzeWeatherPatterns(SolarData data, int year)
    {
        await Task.Run(() =>
        {
            if (!data.ContainsKey(year))
            {
                AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
                return;
            }

            var yearData = data[year];
            var anomalousData = yearData.Where(d => d.AS.HasAnomaly).ToList();

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold cyan]🌦 Weather Pattern Analysis[/]");

            // Weather condition breakdown
            var weatherBreakdown = anomalousData
                .GroupBy(d => d.MS.Condition)
                .Select(g => new
                {
                    Condition = g.Key,
                    Count = g.Count(),
                    AvgScore = g.Average(d => d.AS.TotalAnomalyScore),
                    AvgProduction = g.Average(d => d.P),
                    AvgTemp = g.Average(d => d.MS.AverageTemp)
                })
                .OrderByDescending(wb => wb.Count)
                .ToList();

            var weatherChart = new BarChart()
                .Width(80)
                .Label("[bold]Anomalies by Weather Condition[/]")
                .CenterLabel();

            foreach (var weather in weatherBreakdown)
            {
                var color = weather.Condition switch
                {
                    WeatherCondition.Rainy => Color.Blue,
                    WeatherCondition.Sunny => Color.Yellow,
                    WeatherCondition.PartlyCloudy => Color.Green,
                    WeatherCondition.Cloudy => Color.Grey,
                    _ => Color.White
                };

                weatherChart.AddItem(weather.Condition.ToString(), weather.Count, color);
            }

            AnsiConsole.Write(weatherChart);

            // Detailed weather analysis table
            var weatherTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold]Weather Pattern Details[/]")
                .AddColumn("[yellow]Condition[/]")
                .AddColumn("[white]Anomalies[/]")
                .AddColumn("[red]Avg Score[/]")
                .AddColumn("[green]Avg Production[/]")
                .AddColumn("[cyan]Avg Temp[/]");

            foreach (var weather in weatherBreakdown)
            {
                weatherTable.AddRow(
                    weather.Condition.ToString(),
                    weather.Count.ToString(),
                    $"{weather.AvgScore:F2}",
                    $"{weather.AvgProduction:F2} kWh",
                    $"{weather.AvgTemp:F1}°C"
                );
            }

            AnsiConsole.Write(weatherTable);

            // Weather insights
            var worstWeather = weatherBreakdown.OrderByDescending(w => w.AvgScore).First();
            var mostCommon = weatherBreakdown.OrderByDescending(w => w.Count).First();

            var insightsPanel = new Panel(new Markup(
                $"[red]Worst Weather for Anomalies: {worstWeather.Condition} (Avg Score: {worstWeather.AvgScore:F2})[/]\n" +
                $"[yellow]Most Common Anomaly Weather: {mostCommon.Condition} ({mostCommon.Count} occurrences)[/]\n" +
                $"[blue]Total Anomalous Days: {anomalousData.Count} out of {yearData.Count} days ({(double)anomalousData.Count / yearData.Count * 100:F1}%)[/]"))
            {
                Header = new PanelHeader("[bold]Weather Insights[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };

            AnsiConsole.Write(insightsPanel);
        });
    }

    private async Task InspectSpecificDay(SolarData data, int year)
    {
        await Task.Run(() =>
        {

            if (!data.ContainsKey(year))
            {
                AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
                return;
            }

            var yearData = data[year];
            var dayNumbers = yearData.Select(d => d.D).OrderBy(d => d).ToList();

            var selectedDay = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[yellow]Select day to inspect:[/]")
                    .AddChoices(dayNumbers.Take(50)) // Limit choices for performance
                    .PageSize(20));

            var dayData = yearData.FirstOrDefault(d => d.D == selectedDay);
            if (dayData == null)
            {
                AnsiConsole.MarkupLine("[red]No data found for the selected day.[/]");
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold cyan]🔍 D {selectedDay} Detailed Inspection[/]");

            // D overview panel
            var isAnomaly = dayData.AS.HasAnomaly;
            var anomalyStatus = isAnomaly ? $"[red]ANOMALY ({dayData.AS.Severity})[/]" : "[green]NORMAL[/]";

            var overviewPanel = new Panel(new Markup(
                $"[yellow]D: {dayData.D}[/]\n" +
                $"[white]Status: {anomalyStatus}[/]\n" +
                $"[green]Production: {dayData.P:F2} kWh[/]\n" +
                $"[blue]Consumption: {dayData.U:F2} kWh[/]\n" +
                $"[orange1]Grid Injection: {dayData.I:F2} kWh[/]\n" +
                $"[gray]Energy Balance: {dayData.EnergyBalance:F2} kWh[/]"))
            {
                Header = new PanelHeader("[bold]D Overview[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };

            AnsiConsole.Write(overviewPanel);

            // Weather details
            var weatherPanel = new Panel(new Markup(
                $"[yellow]Condition: {dayData.MS.Condition}[/]\n" +
                $"[white]Temperature: {dayData.MS.AverageTemp:F1}°C ({dayData.MS.MinTemp:F1}°C - {dayData.MS.MaxTemp:F1}°C)[/]\n" +
                $"[blue]Precipitation: {dayData.MS.Precipitation:F1}mm[/]\n" +
                $"[orange1]Sunshine: {dayData.MS.SunshineHours:F1} hours[/]\n" +
                $"[gray]Wind: {dayData.MS.WindSpeed:F1} km/h ({dayData.MS.WindCondition})[/]"))
            {
                Header = new PanelHeader("[bold]Weather Details[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("blue")
            };

            AnsiConsole.Write(weatherPanel);

            // Anomaly details (if applicable)
            if (isAnomaly)
            {
                var anomalyPanel = new Panel(new Markup(
                    $"[red]Anomaly Score: {dayData.AS.TotalAnomalyScore:F2}[/]\n" +
                    $"[green]Production Anomaly: {dayData.AS.ProductionAnomaly:F2}[/]\n" +
                    $"[blue]Consumption Anomaly: {dayData.AS.ConsumptionAnomaly:F2}[/]\n" +
                    $"[orange1]Injection Anomaly: {dayData.AS.InjectionAnomaly:F2}[/]"))
                {
                    Header = new PanelHeader("[bold red]Anomaly Analysis[/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("red")
                };

                AnsiConsole.Write(anomalyPanel);
            }

            // Performance metrics
            var metricsPanel = new Panel(new Markup(
                $"[green]Efficiency: {dayData.Efficiency:F1}%[/]\n" +
                $"[yellow]Peak Production: {dayData.PeakProduction:F2} kWh[/]\n" +
                $"[white]Average Production: {dayData.AverageProduction:F2} kWh[/]\n" +
                $"[blue]Peak Demand Hour: {dayData.Q.PeakDemandHour:F1}[/]\n" +
                $"[orange1]Peak Generation Hour: {dayData.Q.PeakGenerationHour:F1}[/]"))
            {
                Header = new PanelHeader("[bold]Performance Metrics[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green")
            };

            AnsiConsole.Write(metricsPanel);
        });
    }
}
