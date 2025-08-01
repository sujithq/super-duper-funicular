using Spectre.Console;
using Spectre.Console.Cli;
using SolarScope.Models;
using SolarScope.Services;
using System.ComponentModel;

namespace SolarScope.Commands;

/// <summary>
/// Explore command implementation (Spectre.Console.Cli)
/// </summary>
public class ExploreCommand : AsyncCommand<ExploreCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("--mode")]
        [Description("Exploration mode: quick, guided, or full")]
        [DefaultValue("quick")]
        public string Mode { get; set; } = "quick";

        [CommandOption("--year")]
        [Description("Year to explore (optional)")]
        public int? Year { get; set; }
    }

    private static readonly HashSet<string> ValidModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "quick", "guided", "full"
    };

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!ValidModes.Contains(settings.Mode))
        {
            return ValidationResult.Error($"Invalid mode: '{settings.Mode}'. Valid values are: quick, guided, full.");
        }
        return base.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
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
            var headerRule = new Rule("[bold magenta]🔍 Interactive Solar Data Explorer 🔍[/]")
            {
                Style = Style.Parse("magenta"),
                Justification = Justify.Center
            };
            AnsiConsole.Write(headerRule);

            switch (settings.Mode.ToLower())
            {
                case "quick":
                    await QuickExploreMode(data, dataService, settings);
                    break;
                case "guided":
                    await GuidedExploreMode(data, dataService, settings);
                    break;
                case "full":
                    await FullExploreMode(data, dataService, settings);
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Invalid mode. Use: quick, guided, or full[/]");
                    break;
            }
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    private async Task QuickExploreMode(SolarData data, SolarDataService dataService, Settings options)
    {
        await Task.Run(() =>
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold cyan]⚡ Quick Exploration Mode[/]");

            var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();

            if (!data.ContainsKey(selectedYear))
            {
                AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
                return;
            }

            var yearData = data[selectedYear];

            // Quick overview
            var (totalProd, totalCons, totalInj) = data.GetYearlyTotals(selectedYear);
            var avgDaily = yearData.Average(d => d.P);
            var bestDay = yearData.OrderByDescending(d => d.P).First();
            var worstDay = yearData.OrderBy(d => d.P).First();

            var quickPanel = new Panel(new Markup(
                $"[green]📊 Year {selectedYear} Quick Stats[/]\n\n" +
                $"[white]Total Production: {totalProd:F2} kWh[/]\n" +
                $"[blue]Total Consumption: {totalCons:F2} kWh[/]\n" +
                $"[orange1]Grid Injection: {totalInj:F2} kWh[/]\n" +
                $"[yellow]Daily Average: {avgDaily:F2} kWh[/]\n\n" +
                $"[green]🏆 Best D: D {bestDay.D} ({bestDay.P:F2} kWh)[/]\n" +
                $"[red]❗ Worst D: D {worstDay.D} ({worstDay.P:F2} kWh)[/]"))
            {
                Header = new PanelHeader("[bold]Quick Overview[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };

            AnsiConsole.Write(quickPanel);

            // Top 5 and Bottom 5 days
            var topDays = dataService.GetTopProductionDays(data, selectedYear, 5);
            var worstDays = yearData.OrderBy(d => d.P).Take(5).ToList();

            var topTable = new Table()
                .Border(TableBorder.Minimal)
                .Title("[bold green]🏆 Top 5 Production Days[/]")
                .AddColumn("[yellow]D[/]")
                .AddColumn("[green]Production[/]")
                .AddColumn("[cyan]Weather[/]");

            foreach (var day in topDays)
            {
                topTable.AddRow(
                    day.D.ToString(),
                    $"{day.P:F2} kWh",
                    day.MS.Condition.ToString()
                );
            }

            var bottomTable = new Table()
                .Border(TableBorder.Minimal)
                .Title("[bold red]❗ Bottom 5 Production Days[/]")
                .AddColumn("[yellow]D[/]")
                .AddColumn("[red]Production[/]")
                .AddColumn("[cyan]Weather[/]");

            foreach (var day in worstDays)
            {
                bottomTable.AddRow(
                    day.D.ToString(),
                    $"{day.P:F2} kWh",
                    day.MS.Condition.ToString()
                );
            }

            var layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Left").Update(topTable),
                    new Layout("Right").Update(bottomTable)
                );

            AnsiConsole.Write(layout);

            // Anomaly summary
            var anomalies = dataService.GetAnomalousData(data, selectedYear);
            var anomalyPanel = new Panel(new Markup(
                $"[red]❗ Anomalies Detected: {anomalies.Count}[/]\n" +
                $"[orange1]High Severity: {anomalies.Count(a => a.AS.Severity == AnomalySeverity.High)}[/]\n" +
                $"[yellow]Medium Severity: {anomalies.Count(a => a.AS.Severity == AnomalySeverity.Medium)}[/]\n" +
                $"[green]Low Severity: {anomalies.Count(a => a.AS.Severity == AnomalySeverity.Low)}[/]"))
            {
                Header = new PanelHeader("[bold]Anomaly Summary[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("red")
            };

            AnsiConsole.WriteLine();
            AnsiConsole.Write(anomalyPanel);
        });
    }

    private async Task GuidedExploreMode(SolarData data, SolarDataService dataService, Settings options)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]🎯 Guided Exploration Mode[/]");

        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();

        while (true)
        {
            AnsiConsole.Clear();
            var headerRule = new Rule($"[bold magenta]🔍 Guided Explorer - Year {selectedYear} 🔍[/]")
            {
                Style = Style.Parse("magenta"),
                Justification = Justify.Center
            };
            AnsiConsole.Write(headerRule);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to explore?[/]")
                    .AddChoices(new[]
                    {
                        "System Performance Overview",
                        "Production Patterns",
                        "Weather Impact Analysis", 
                        "Anomaly Investigation",
                        "Efficiency Analysis",
                        "Monthly Breakdown",
                        "D Inspector",
                        "Change Year",
                        "Exit"
                    }));

            switch (choice)
            {
                case "System Performance Overview":
                    await ShowSystemPerformance(data, selectedYear);
                    break;

                case "Production Patterns":
                    await ShowProductionPatterns(data, selectedYear);
                    break;

                case "Weather Impact Analysis":
                    await ShowWeatherImpact(data, dataService, selectedYear);
                    break;

                case "Anomaly Investigation":
                    await ShowAnomalyInvestigation(data, dataService, selectedYear);
                    break;

                case "Efficiency Analysis":
                    await ShowEfficiencyAnalysis(data, selectedYear);
                    break;

                case "Monthly Breakdown":
                    await ShowMonthlyBreakdown(data, dataService, selectedYear);
                    break;

                case "D Inspector":
                    await RunDayInspector(data, selectedYear);
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

    private async Task FullExploreMode(SolarData data, SolarDataService dataService, Settings options)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]🚀 Full Interactive Explorer[/]");

        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();

        while (true)
        {
            AnsiConsole.Clear();
            var headerRule = new Rule($"[bold magenta]🚀 Full Explorer - Year {selectedYear} 🚀[/]")
            {
                Style = Style.Parse("magenta"),
                Justification = Justify.Center
            };
            AnsiConsole.Write(headerRule);

            var mainChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Choose exploration category:[/]")
                    .AddChoices(new[]
                    {
                        "📊 Data Analysis",
                        "🌦 Weather Exploration", 
                        "❗ Anomaly Deep Dive",
                        "🔍 Advanced Inspection",
                        "📈 Performance Optimization",
                        "⚙️ System Configuration",
                        "Exit"
                    }));

            switch (mainChoice)
            {
                case "📊 Data Analysis":
                    await DataAnalysisMenu(data, dataService, selectedYear);
                    break;

                case "🌦 Weather Exploration":
                    await WeatherExplorationMenu(data, dataService, selectedYear);
                    break;

                case "❗ Anomaly Deep Dive":
                    await AnomalyDeepDiveMenu(data, dataService, selectedYear);
                    break;

                case "🔍 Advanced Inspection":
                    await AdvancedInspectionMenu(data, dataService, selectedYear);
                    break;

                case "📈 Performance Optimization":
                    await PerformanceOptimizationMenu(data, dataService, selectedYear);
                    break;

                case "⚙️ System Configuration":
                    selectedYear = await SystemConfigurationMenu(data, selectedYear);
                    break;

                case "Exit":
                    return;
            }
        }
    }

    private async Task ShowSystemPerformance(SolarData data, int year)
    {
        await Task.Run(() =>
        {
            if (!data.ContainsKey(year))
            {
                AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
                return;
            }

            var yearData = data[year];
            var (totalProd, totalCons, totalInj) = data.GetYearlyTotals(year);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]📊 System Performance - Year {year}[/]");

            // Performance metrics
            var efficiency = totalProd > 0 ? (totalCons / totalProd * 100) : 0;
            var selfConsumption = (totalProd - totalInj) / totalProd * 100;
            var avgDaily = yearData.Average(d => d.P);
            var capacity = yearData.Max(d => d.P);

            var metricsPanel = new Panel(new Markup(
                $"[green]🔋 Total Production: {totalProd:F2} kWh[/]\n" +
                $"[blue]⚡ Total Consumption: {totalCons:F2} kWh[/]\n" +
                $"[orange1]🏠 Grid Injection: {totalInj:F2} kWh[/]\n" +
                $"[yellow]📊 System Efficiency: {efficiency:F1}%[/]\n" +
                $"[white]🏡 Self-Consumption: {selfConsumption:F1}%[/]\n" +
                $"[gray]📅 Daily Average: {avgDaily:F2} kWh[/]\n" +
                $"[purple]⚡ Peak Capacity: {capacity:F2} kWh[/]"))
            {
                Header = new PanelHeader("[bold]Performance Metrics[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green")
            };

            AnsiConsole.Write(metricsPanel);

            // Monthly performance chart
            var monthlyData = yearData
                .GroupBy(d => (d.D - 1) / 30 + 1)
                .Select(g => new { Month = g.Key, Production = g.Sum(d => d.P) })
                .ToList();

            var monthlyChart = new BarChart()
                .Width(80)
                .Label("[bold]Monthly Production (kWh)[/]")
                .CenterLabel();

            var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            foreach (var month in monthlyData)
            {
                var color = month.Production switch
                {
                    > 600 => Color.Green,
                    > 400 => Color.Yellow,
                    > 200 => Color.Orange1,
                    _ => Color.Red
                };

                monthlyChart.AddItem(monthNames[Math.Min(month.Month, 12)], month.Production, color);
            }

            AnsiConsole.Write(monthlyChart);
        });
    }

    private async Task ShowProductionPatterns(SolarData data, int year)
    {
        await Task.Run(() =>
        {
            if (!data.ContainsKey(year))
            {
                AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
                return;
            }

            var yearData = data[year];

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]📈 Production Patterns - Year {year}[/]");

            // Production distribution
            var productionRanges = new[]
            {
            ("0-5 kWh", yearData.Count(d => d.P >= 0 && d.P < 5)),
            ("5-15 kWh", yearData.Count(d => d.P >= 5 && d.P < 15)),
            ("15-25 kWh", yearData.Count(d => d.P >= 15 && d.P < 25)),
            ("25-35 kWh", yearData.Count(d => d.P >= 25 && d.P < 35)),
            ("35+ kWh", yearData.Count(d => d.P >= 35))
        };

            var distributionChart = new BarChart()
                .Width(80)
                .Label("[bold]Production Distribution (Days per Range)[/]")
                .CenterLabel();

            foreach (var (range, count) in productionRanges)
            {
                var color = range switch
                {
                    "35+ kWh" => Color.Green,
                    "25-35 kWh" => Color.Yellow,
                    "15-25 kWh" => Color.Orange1,
                    "5-15 kWh" => Color.Red,
                    _ => Color.DarkRed
                };

                distributionChart.AddItem(range, count, color);
            }

            AnsiConsole.Write(distributionChart);

            // Production trends
            var trendsTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold]Production Trends[/]")
                .AddColumn("[yellow]Metric[/]")
                .AddColumn("[green]Value[/]");

            var maxProduction = yearData.Max(d => d.P);
            var minProduction = yearData.Min(d => d.P);
            var avgProduction = yearData.Average(d => d.P);
            var stdDev = Math.Sqrt(yearData.Average(d => Math.Pow(d.P - avgProduction, 2)));

            trendsTable.AddRow("Maximum Daily Production", $"{maxProduction:F2} kWh");
            trendsTable.AddRow("Minimum Daily Production", $"{minProduction:F2} kWh");
            trendsTable.AddRow("Average Daily Production", $"{avgProduction:F2} kWh");
            trendsTable.AddRow("Standard Deviation", $"{stdDev:F2} kWh");
            trendsTable.AddRow("Production Variability", $"{(stdDev / avgProduction * 100):F1}%");

            AnsiConsole.Write(trendsTable);
        });
    }

    private async Task ShowWeatherImpact(SolarData data, SolarDataService dataService, int year)
    {
        await Task.Run(() =>
        {
            var correlation = dataService.AnalyzeWeatherCorrelation(data, year);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]🌦 Weather Impact Analysis - Year {year}[/]");

            var impactPanel = new Panel(new Markup(
                $"[yellow]☀ Sunshine Correlation: {correlation.SunshineCorrelation:F3}[/]\n" +
                $"[red]🌡 Temperature Correlation: {correlation.TemperatureCorrelation:F3}[/]\n" +
                $"[blue]🌧 Precipitation Correlation: {correlation.PrecipitationCorrelation:F3}[/]\n" +
                $"[gray]💨 Wind Speed Correlation: {correlation.WindCorrelation:F3}[/]"))
            {
                Header = new PanelHeader("[bold]Weather Correlations[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("blue")
            };

            AnsiConsole.Write(impactPanel);

            if (!data.ContainsKey(year)) return;

            var yearData = data[year];
            var weatherBreakdown = yearData
                .GroupBy(d => d.MS.Condition)
                .Select(g => new
                {
                    Condition = g.Key,
                    Days = g.Count(),
                    AvgProduction = g.Average(d => d.P),
                    TotalProduction = g.Sum(d => d.P)
                })
                .OrderByDescending(wb => wb.AvgProduction)
                .ToList();

            var weatherTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold]Production by Weather Condition[/]")
                .AddColumn("[yellow]Condition[/]")
                .AddColumn("[white]Days[/]")
                .AddColumn("[green]Avg Production[/]")
                .AddColumn("[blue]Total Production[/]");

            foreach (var weather in weatherBreakdown)
            {
                weatherTable.AddRow(
                    weather.Condition.ToString(),
                    weather.Days.ToString(),
                    $"{weather.AvgProduction:F2} kWh",
                    $"{weather.TotalProduction:F2} kWh"
                );
            }

            AnsiConsole.Write(weatherTable);
        });
    }

    private async Task ShowAnomalyInvestigation(SolarData data, SolarDataService dataService, int year)
    {
        await Task.Run(() =>
        {
            var anomalies = dataService.GetAnomalousData(data, year);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]❗ Anomaly Investigation - Year {year}[/]");

            if (!anomalies.Any())
            {
                AnsiConsole.MarkupLine("[green]✅ No anomalies detected for this year![/]");
                return;
            }

            var severityBreakdown = anomalies
                .GroupBy(a => a.AS.Severity)
                .ToDictionary(g => g.Key, g => g.Count());

            var anomalyPanel = new Panel(new Markup(
                $"[red]🚨 Total Anomalies: {anomalies.Count}[/]\n" +
                $"[darkred]High Severity: {severityBreakdown.GetValueOrDefault(AnomalySeverity.High, 0)}[/]\n" +
                $"[orange1]Medium Severity: {severityBreakdown.GetValueOrDefault(AnomalySeverity.Medium, 0)}[/]\n" +
                $"[yellow]Low Severity: {severityBreakdown.GetValueOrDefault(AnomalySeverity.Low, 0)}[/]"))
            {
                Header = new PanelHeader("[bold]Anomaly Summary[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("red")
            };

            AnsiConsole.Write(anomalyPanel);

            // Top anomalies
            var topAnomalies = anomalies
                .OrderByDescending(a => a.AS.TotalAnomalyScore)
                .Take(10)
                .ToList();

            var anomalyTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold]Top 10 Anomalies[/]")
                .AddColumn("[yellow]D[/]")
                .AddColumn("[red]Score[/]")
                .AddColumn("[white]Severity[/]")
                .AddColumn("[green]Production[/]")
                .AddColumn("[cyan]Weather[/]");

            foreach (var anomaly in topAnomalies)
            {
                var severityColor = anomaly.AS.Severity switch
                {
                    AnomalySeverity.High => "red",
                    AnomalySeverity.Medium => "orange",
                    AnomalySeverity.Low => "yellow",
                    _ => "green"
                };

                anomalyTable.AddRow(
                    anomaly.D.ToString(),
                    $"{anomaly.AS.TotalAnomalyScore:F2}",
                    $"[{severityColor}]{anomaly.AS.Severity}[/]",
                    $"{anomaly.P:F2} kWh",
                    anomaly.MS.Condition.ToString()
                );
            }

            AnsiConsole.Write(anomalyTable);
        });
    }

    private async Task ShowEfficiencyAnalysis(SolarData data, int year)
    {
        await Task.Run(() =>
        {
            if (!data.ContainsKey(year))
            {
                AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
                return;
            }

            var yearData = data[year];

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]⚡ Efficiency Analysis - Year {year}[/]");

            var efficiencyData = yearData
                .Select(d => new { Day = d.D, Efficiency = d.Efficiency, Production = d.P, Balance = d.EnergyBalance })
                .OrderByDescending(ed => ed.Efficiency)
                .ToList();

            var avgEfficiency = efficiencyData.Average(ed => ed.Efficiency);
            var highEfficiencyDays = efficiencyData.Count(ed => ed.Efficiency > 90);
            var lowEfficiencyDays = efficiencyData.Count(ed => ed.Efficiency < 50);

            var efficiencyPanel = new Panel(new Markup(
                $"[green]📊 Average System Efficiency: {avgEfficiency:F1}%[/]\n" +
                $"[yellow]🔋 High Efficiency Days (>90%): {highEfficiencyDays}[/]\n" +
                $"[red]❗ Low Efficiency Days (<50%): {lowEfficiencyDays}[/]\n" +
                $"[blue]📈 Best Efficiency: {efficiencyData.First().Efficiency:F1}% (D {efficiencyData.First().Day})[/]\n" +
                $"[orange1]📉 Worst Efficiency: {efficiencyData.Last().Efficiency:F1}% (D {efficiencyData.Last().Day})[/]"))
            {
                Header = new PanelHeader("[bold]Efficiency Metrics[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green")
            };

            AnsiConsole.Write(efficiencyPanel);

            // Efficiency ranges
            var efficiencyRanges = new[]
            {
            ("90-100%", efficiencyData.Count(ed => ed.Efficiency >= 90)),
            ("75-90%", efficiencyData.Count(ed => ed.Efficiency >= 75 && ed.Efficiency < 90)),
            ("50-75%", efficiencyData.Count(ed => ed.Efficiency >= 50 && ed.Efficiency < 75)),
            ("25-50%", efficiencyData.Count(ed => ed.Efficiency >= 25 && ed.Efficiency < 50)),
            ("0-25%", efficiencyData.Count(ed => ed.Efficiency < 25))
        };

            var efficiencyChart = new BarChart()
                .Width(80)
                .Label("[bold]Efficiency Distribution (Days per Range)[/]")
                .CenterLabel();

            foreach (var (range, count) in efficiencyRanges)
            {
                var color = range switch
                {
                    "90-100%" => Color.Green,
                    "75-90%" => Color.Yellow,
                    "50-75%" => Color.Orange1,
                    "25-50%" => Color.Red,
                    _ => Color.DarkRed
                };

                efficiencyChart.AddItem(range, count, color);
            }

            AnsiConsole.Write(efficiencyChart);
        });
    }

    private async Task ShowMonthlyBreakdown(SolarData data, SolarDataService dataService, int year)
    {
        await Task.Run(() =>
        {
            var monthlyStats = dataService.GetMonthlyStatistics(data, year);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]📅 Monthly Breakdown - Year {year}[/]");

            if (!monthlyStats.Any())
            {
                AnsiConsole.MarkupLine("[red]No monthly data available.[/]");
                return;
            }

            var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            var monthlyTable = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[bold]Monthly Statistics - Year {year}[/]")
                .AddColumn("[yellow]Month[/]")
                .AddColumn("[green]Production[/]")
                .AddColumn("[blue]Consumption[/]")
                .AddColumn("[orange1]Injection[/]")
                .AddColumn("[white]Avg Temp[/]")
                .AddColumn("[cyan]Sunshine[/]")
                .AddColumn("[red]Anomalies[/]");

            foreach (var month in monthlyStats.OrderBy(m => m.Key))
            {
                var stats = month.Value;
                monthlyTable.AddRow(
                    monthNames[month.Key],
                    $"{stats.TotalProduction:F1} kWh",
                    $"{stats.TotalConsumption:F1} kWh",
                    $"{stats.TotalInjection:F1} kWh",
                    $"{stats.AverageTemperature:F1}°C",
                    $"{stats.TotalSunshineHours:F1}h",
                    stats.AnomalyCount.ToString()
                );
            }

            AnsiConsole.Write(monthlyTable);

            // Best and worst months
            var bestMonth = monthlyStats.OrderByDescending(m => m.Value.TotalProduction).First();
            var worstMonth = monthlyStats.OrderBy(m => m.Value.TotalProduction).First();

            var comparisonPanel = new Panel(new Markup(
                $"[green]🏆 Best Month: {monthNames[bestMonth.Key]} ({bestMonth.Value.TotalProduction:F2} kWh)[/]\n" +
                $"[red]❗ Worst Month: {monthNames[worstMonth.Key]} ({worstMonth.Value.TotalProduction:F2} kWh)[/]\n" +
                $"[yellow]📊 Monthly Average: {monthlyStats.Average(m => m.Value.TotalProduction):F2} kWh[/]"))
            {
                Header = new PanelHeader("[bold]Monthly Comparison[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };

            AnsiConsole.Write(comparisonPanel);
        });
    }

    private async Task RunDayInspector(SolarData data, int year)
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
                    .AddChoices(dayNumbers.Take(50))
                    .PageSize(20));

            var dayData = yearData.FirstOrDefault(d => d.D == selectedDay);
            if (dayData == null)
            {
                AnsiConsole.MarkupLine("[red]No data found for the selected day.[/]");
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold cyan]🔍 D {selectedDay} Inspector[/]");

            // D overview with all metrics
            var overviewPanel = new Panel(new Markup(
                $"[green]⚡ Production: {dayData.P:F2} kWh[/]\n" +
                $"[blue]🏠 Consumption: {dayData.U:F2} kWh[/]\n" +
                $"[orange1]🔌 Grid Injection: {dayData.I:F2} kWh[/]\n" +
                $"[yellow]📊 Efficiency: {dayData.Efficiency:F1}%[/]\n" +
                $"[white]⚖ Energy Balance: {dayData.EnergyBalance:F2} kWh[/]\n" +
                $"[purple]📈 Peak Production: {dayData.PeakProduction:F2} kWh[/]"))
            {
                Header = new PanelHeader("[bold]Production Metrics[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green")
            };

            var weatherPanel = new Panel(new Markup(
                $"[yellow]🌤 Condition: {dayData.MS.Condition}[/]\n" +
                $"[red]🌡 Temperature: {dayData.MS.AverageTemp:F1}°C ({dayData.MS.MinTemp:F1}°C - {dayData.MS.MaxTemp:F1}°C)[/]\n" +
                $"[blue]🌧 Precipitation: {dayData.MS.Precipitation:F1}mm[/]\n" +
                $"[orange1]☀ Sunshine: {dayData.MS.SunshineHours:F1} hours[/]\n" +
                $"[gray]💨 Wind: {dayData.MS.WindSpeed:F1} km/h ({dayData.MS.WindCondition})[/]\n" +
                $"[white]🔽 Pressure: {dayData.MS.Pressure:F1} hPa[/]"))
            {
                Header = new PanelHeader("[bold]Weather Details[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("blue")
            };

            var layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Left").Update(overviewPanel),
                    new Layout("Right").Update(weatherPanel)
                );

            AnsiConsole.Write(layout);

            // Anomaly information if applicable
            if (dayData.AS.HasAnomaly)
            {
                var anomalyPanel = new Panel(new Markup(
                    $"[red]🚨 ANOMALY DETECTED[/]\n" +
                    $"[white]Severity: {dayData.AS.Severity}[/]\n" +
                    $"[yellow]Total Score: {dayData.AS.TotalAnomalyScore:F2}[/]\n" +
                    $"[green]Production Anomaly: {dayData.AS.ProductionAnomaly:F2}[/]\n" +
                    $"[blue]Consumption Anomaly: {dayData.AS.ConsumptionAnomaly:F2}[/]\n" +
                    $"[orange1]Injection Anomaly: {dayData.AS.InjectionAnomaly:F2}[/]"))
                {
                    Header = new PanelHeader("[bold red]Anomaly Analysis[/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("red")
                };

                AnsiConsole.WriteLine();
                AnsiConsole.Write(anomalyPanel);
            }

            // Quarter-hourly data summary
            var quarterlyPanel = new Panel(new Markup(
                $"[cyan]📊 Quarter-hour Readings: {dayData.Q.TotalReadings}[/]\n" +
                $"[white]⏱️ Time Span: {dayData.Q.TimeSpanHours:F1} hours[/]\n" +
                $"[yellow]🔝 Peak Demand Hour: {dayData.Q.PeakDemandHour:F1}[/]\n" +
                $"[green]⚡ Peak Generation Hour: {dayData.Q.PeakGenerationHour:F1}[/]"))
            {
                Header = new PanelHeader("[bold]Detailed Readings[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };

            AnsiConsole.WriteLine();
            AnsiConsole.Write(quarterlyPanel);
        });
    }

    // Menu implementations for full mode
    private async Task DataAnalysisMenu(SolarData data, SolarDataService dataService, int year)
    {
        // Implementation for data analysis submenu
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Data Analysis Options:[/]")
                .AddChoices(new[]
                {
                    "Production Trends",
                    "Consumption Analysis", 
                    "Statistical Overview",
                    "Comparative Analysis",
                    "Back"
                }));

        switch (choice)
        {
            case "Production Trends":
                await ShowProductionPatterns(data, year);
                break;
            case "Consumption Analysis":
                await ShowEfficiencyAnalysis(data, year);
                break;
            case "Statistical Overview":
                await ShowSystemPerformance(data, year);
                break;
            case "Comparative Analysis":
                await ShowMonthlyBreakdown(data, dataService, year);
                break;
        }
    }

    private async Task WeatherExplorationMenu(SolarData data, SolarDataService dataService, int year)
    {
        // Weather exploration submenu
        await ShowWeatherImpact(data, dataService, year);
    }

    private async Task AnomalyDeepDiveMenu(SolarData data, SolarDataService dataService, int year)
    {
        // Anomaly deep dive submenu
        await ShowAnomalyInvestigation(data, dataService, year);
    }

    private async Task AdvancedInspectionMenu(SolarData data, SolarDataService dataService, int year)
    {
        // Advanced inspection submenu
        await RunDayInspector(data, year);
    }

    private async Task PerformanceOptimizationMenu(SolarData data, SolarDataService dataService, int year)
    {
        // Performance optimization suggestions
        await ShowEfficiencyAnalysis(data, year);
    }

    private async Task<int> SystemConfigurationMenu(SolarData data, int currentYear)
    {
        return await Task.Run(() =>
        {
            // System configuration menu
            return AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[yellow]Select year:[/]")
                    .AddChoices(data.AvailableYears));
        });
    }
}
