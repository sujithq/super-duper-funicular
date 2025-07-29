using Spectre.Console;
using SolarScope.Models;
using SolarScope.Services;

namespace SolarScope.Commands;

/// <summary>
/// Dashboard command implementation
/// </summary>
public class DashboardCommand
{
    public async Task ExecuteAsync(DashboardOptions options)
    {
        // Console.WriteLine("DashboardCommand.ExecuteAsync started");
        // Console.Out.Flush();
        
        SolarDataService dataService = null!;
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("yellow"))
            .StartAsync("Loading solar data...", async ctx =>
            {
                dataService = new SolarDataService(options.DataFile);
                
                ctx.Status("Analyzing data...");
                await Task.Delay(500); // Simulated processing for dramatic effect
            });

        var data = await dataService.LoadDataAsync();
        // Console.WriteLine($"Data loaded: {data?.Count ?? 0} years");
        // Console.Out.Flush();
        
        if (data == null)
        {
            // Console.WriteLine("Data is null - showing error");
            // Console.Out.Flush();
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return;
        }

        // Console.WriteLine("About to clear console and display dashboard");
        // Console.Out.Flush();
        
        AnsiConsole.Clear();
        
        if (options.Animated)
        {
            await DisplayAnimatedDashboard(data, options.FullDashboard);
        }
        else
        {
            DisplayStaticDashboard(data, options.FullDashboard);
        }
    }

    private void DisplayStaticDashboard(SolarData data, bool fullDashboard)
    {
        // Header
        var headerRule = new Rule("[bold yellow]🌞 Solar System Dashboard 🌞[/]")
        {
            Style = Style.Parse("yellow"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(headerRule);
        AnsiConsole.WriteLine();

        // Key Statistics
        DisplayKeyStatistics(data);
        
        if (fullDashboard)
        {
            AnsiConsole.WriteLine();
            DisplayProductionChart(data);
            AnsiConsole.WriteLine();
            DisplayWeatherSummary(data);
            AnsiConsole.WriteLine();
            DisplayAnomalySummary(data);
        }
        else
        {
            AnsiConsole.WriteLine();
            DisplayQuickSummary(data);
        }

        AnsiConsole.WriteLine();
        DisplayFooter();
    }

    private async Task DisplayAnimatedDashboard(SolarData data, bool fullDashboard)
    {
        // Animated header
        await DisplayAnimatedHeader();
        
        // Animate key statistics
        await DisplayAnimatedStatistics(data);
        
        if (fullDashboard)
        {
            await DisplayAnimatedCharts(data);
        }
        
        DisplayFooter();
    }

    private async Task DisplayAnimatedHeader()
    {
        var frames = new[]
        {
            "☀️ Solar System Dashboard ☀️",
            "🌞 Solar System Dashboard 🌞",
            "⭐ Solar System Dashboard ⭐",
            "🌟 Solar System Dashboard 🌟",
            "✨ Solar System Dashboard ✨"
        };

        for (int i = 0; i < 8; i++)
        {
            AnsiConsole.Clear();
            var frame = frames[i % frames.Length];
            var rule = new Rule($"[bold yellow]{frame}[/]")
            {
                Style = Style.Parse("yellow"),
                Justification = Justify.Center
            };
            AnsiConsole.Write(rule);
            await Task.Delay(300);
        }
        AnsiConsole.WriteLine();
    }

    private void DisplayKeyStatistics(SolarData data)
    {
        var (totalProduction, totalConsumption, totalInjection) = data.YearlyTotals;
        var latestYearData = data.GetLatestYearData();
        var averageDaily = latestYearData.Average(d => d.P);
        var bestDay = latestYearData.MaxBy(d => d.P);
        var anomalyCount = latestYearData.Count(d => d.AS.HasAnomaly);

        var statsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow);

        statsTable.AddColumn(new TableColumn("[bold]Metric[/]").Centered());
        statsTable.AddColumn(new TableColumn("[bold]Value[/]").Centered());
        statsTable.AddColumn(new TableColumn("[bold]Status[/]").Centered());

        // Add rows with color coding
        statsTable.AddRow(
            "Total Production", 
            $"[green]{totalProduction:F2} kWh[/]", 
            GetStatusEmoji(totalProduction, 1000)
        );
        
        statsTable.AddRow(
            "Total Consumption", 
            $"[blue]{totalConsumption:F2} kWh[/]", 
            GetStatusEmoji(totalConsumption, 800)
        );
        
        statsTable.AddRow(
            "Grid Injection", 
            $"[cyan]{totalInjection:F2} kWh[/]", 
            GetStatusEmoji(totalInjection, 200)
        );
        
        statsTable.AddRow(
            "Average Daily", 
            $"[yellow]{averageDaily:F2} kWh[/]", 
            GetStatusEmoji(averageDaily, 10)
        );
        
        statsTable.AddRow(
            "Best Production D", 
            $"[green]D {bestDay?.D} ({bestDay?.P:F2} kWh)[/]", 
            "🏆"
        );
        
        statsTable.AddRow(
            "System Anomalies", 
            $"[red]{anomalyCount} detected[/]", 
            anomalyCount == 0 ? "✅" : "⚠️"
        );

        var panel = new Panel(statsTable)
        {
            Header = new PanelHeader("[bold]Key Statistics[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.Write(panel);
    }

    private async Task DisplayAnimatedStatistics(SolarData data)
    {
        var (totalProduction, totalConsumption, totalInjection) = data.YearlyTotals;
        
        // Animate counters
        await AnsiConsole.Live(new Panel("Starting..."))
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .StartAsync(async ctx =>
            {
                for (int i = 0; i <= 100; i += 5)
                {
                    var currentProduction = totalProduction * i / 100;
                    var currentConsumption = totalConsumption * i / 100;
                    
                    var content = new Markup($"[green]Production: {currentProduction:F0} kWh[/]\n" +
                                           $"[blue]Consumption: {currentConsumption:F0} kWh[/]\n" +
                                           $"[yellow]Progress: {i}%[/]");
                    
                    ctx.UpdateTarget(new Panel(content)
                    {
                        Header = new PanelHeader("[bold]Loading Statistics...[/]"),
                        Border = BoxBorder.Rounded
                    });
                    
                    await Task.Delay(50);
                }
            });

        // Display final statistics
        DisplayKeyStatistics(data);
    }

    private void DisplayProductionChart(SolarData data)
    {
        var chart = new BarChart()
            .Width(80)
            .Label("[green bold]Daily Production (Last 20 Days)[/]")
            .CenterLabel();

        // Get last 20 days for chart
        var recentDays = data.GetLatestYearData().TakeLast(20).ToList();
        
        foreach (var day in recentDays)
        {
            var color = day.P switch
            {
                > 15 => Color.Green,
                > 10 => Color.Yellow,
                > 5 => Color.Orange1,
                _ => Color.Red
            };
            
            chart.AddItem($"D {day.D}", day.P, color);
        }

        AnsiConsole.Write(chart);
    }

    private void DisplayWeatherSummary(SolarData data)
    {
        var weatherData = data.GetLatestYearData().Select(d => d.MS).ToList();
        var avgTemp = weatherData.Average(w => w.AverageTemp);
        var totalPrecip = weatherData.Sum(w => w.Precipitation);
        var avgSunshine = weatherData.Average(w => w.SunshineHours);
        
        var weatherTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue);

        weatherTable.AddColumn("[bold]Weather Metric[/]");
        weatherTable.AddColumn("[bold]Value[/]");
        weatherTable.AddColumn("[bold]Trend[/]");

        weatherTable.AddRow("Average Temperature", $"{avgTemp:F1}°C", GetTempTrend(avgTemp));
        weatherTable.AddRow("Total Precipitation", $"{totalPrecip:F1}mm", GetPrecipTrend(totalPrecip));
        weatherTable.AddRow("Average Sunshine", $"{avgSunshine:F1}h", GetSunshineTrend(avgSunshine));

        var weatherPanel = new Panel(weatherTable)
        {
            Header = new PanelHeader("[bold]Weather Summary[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue")
        };

        AnsiConsole.Write(weatherPanel);
    }

    private void DisplayAnomalySummary(SolarData data)
    {
        var anomalies = data.GetLatestYearData().Where(d => d.AS.HasAnomaly).ToList();
        var severityGroups = anomalies.GroupBy(d => d.AS.Severity).ToList();

        var anomalyChart = new BreakdownChart()
            .Width(40)
            .ShowPercentage()
            .UseValueFormatter(value => $"{value} day(s)");

        foreach (var group in severityGroups)
        {
            var color = group.Key switch
            {
                AnomalySeverity.High => Color.Red,
                AnomalySeverity.Medium => Color.Orange1,
                AnomalySeverity.Low => Color.Yellow,
                _ => Color.Green
            };
            
            anomalyChart.AddItem(group.Key.ToString(), group.Count(), color);
        }

        if (anomalies.Count == 0)
        {
            anomalyChart.AddItem("No Anomalies", 1, Color.Green);
        }

        var anomalyPanel = new Panel(anomalyChart)
        {
            Header = new PanelHeader("[bold]Anomaly Breakdown[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("red")
        };

        AnsiConsole.Write(anomalyPanel);
    }

    private async Task DisplayAnimatedCharts(SolarData data)
    {
        AnsiConsole.WriteLine();
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Generating charts...", async ctx =>
            {
                await Task.Delay(1000);
            });

        DisplayProductionChart(data);
        await Task.Delay(500);
        
        AnsiConsole.WriteLine();
        DisplayWeatherSummary(data);
        await Task.Delay(500);
        
        AnsiConsole.WriteLine();
        DisplayAnomalySummary(data);
    }

    private void DisplayQuickSummary(SolarData data)
    {
        var (totalProduction, totalConsumption, _) = data.YearlyTotals;
        var balance = totalProduction - totalConsumption;
        var efficiency = totalConsumption / totalProduction * 100;

        var quickStats = new Panel(new Markup(
            $"[green]📊 Total Production: {totalProduction:F1} kWh[/]\n" +
            $"[blue]⚡ Total Consumption: {totalConsumption:F1} kWh[/]\n" +
            $"[yellow]⚖️ Energy Balance: {(balance >= 0 ? "+" : "")}{balance:F1} kWh[/]\n" +
            $"[cyan]🎯 System Efficiency: {efficiency:F1}%[/]\n\n" +
            $"[dim]💡 Use --full for detailed dashboard[/]"))
        {
            Header = new PanelHeader("[bold]Quick Summary[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("green")
        };

        AnsiConsole.Write(quickStats);
    }

    private void DisplayFooter()
    {
        var rule = new Rule("[dim]Built with ❤️ for GitHub's For the Love of Code 2025[/]")
        {
            Style = Style.Parse("dim"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
    }

    private static string GetStatusEmoji(double value, double threshold)
    {
        return value >= threshold ? "✅" : value >= threshold * 0.7 ? "⚠️" : "❌";
    }

    private static string GetTempTrend(double temp)
    {
        return temp switch
        {
            > 20 => "🌡️ Warm",
            > 10 => "🌤️ Mild",
            _ => "❄️ Cool"
        };
    }

    private static string GetPrecipTrend(double precip)
    {
        return precip switch
        {
            > 100 => "🌧️ Wet",
            > 50 => "🌦️ Moderate",
            _ => "☀️ Dry"
        };
    }

    private static string GetSunshineTrend(double sunshine)
    {
        return sunshine switch
        {
            > 8 => "☀️ Excellent",
            > 5 => "⛅ Good",
            _ => "☁️ Limited"
        };
    }
}
