using Spectre.Console;
using Spectre.Console.Cli;
using SolarScope.Models;
using SolarScope.Services;
using System.ComponentModel;

namespace SolarScope.Commands;

/// <summary>
/// Weather command implementation (Spectre.Console.Cli)
/// </summary>
public class WeatherCommand : AsyncCommand<WeatherCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("--analysis")]
        [Description("Analysis type: overview, correlation, patterns, or recommendations")]
        [DefaultValue("overview")]
        public string Analysis { get; set; } = "overview";

        [CommandOption("--year")]
        [Description("Year to analyze (optional)")]
        public int? Year { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var dataService = new SolarDataService(settings.DataFile);
            var data = await dataService.LoadDataAsync();

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
            var headerRule = new Rule("[bold blue]🌦 Weather Analysis & Solar Production Correlation 🌦[/]")
            {
                Style = Style.Parse("blue"),
                Justification = Justify.Center
            };
            AnsiConsole.Write(headerRule);

            switch (settings.Analysis.ToLower())
            {
                case "overview":
                    await DisplayWeatherOverview(data, dataService, settings);
                    break;
                case "correlation":
                    await DisplayCorrelationAnalysis(data, dataService, settings);
                    break;
                case "patterns":
                    await DisplayWeatherPatterns(data, settings);
                    break;
                case "recommendations":
                    await DisplayRecommendations(data, dataService, settings);
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Invalid analysis type. Use: overview, correlation, patterns, or recommendations[/]");
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

    private async Task DisplayWeatherOverview(SolarData data, SolarDataService dataService, Settings options)
    {
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
            return;
        }

        var yearData = data[selectedYear];

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Weather Overview - Year {selectedYear}[/]");

        // Weather condition distribution
        var weatherDistribution = yearData
            .GroupBy(d => d.MS.Condition)
            .Select(g => new { Condition = g.Key, Count = g.Count(), Percentage = (double)g.Count() / yearData.Count * 100 })
            .OrderByDescending(wd => wd.Count)
            .ToList();

        var weatherChart = new BarChart()
            .Width(100)
            .Label("[bold]Weather Condition Distribution[/]")
            .CenterLabel();

        foreach (var weather in weatherDistribution)
        {
            var colorEnum = weather.Condition switch
            {
                WeatherCondition.Sunny => Color.Yellow,
                WeatherCondition.PartlyCloudy => Color.Green,
                WeatherCondition.Cloudy => Color.Grey,
                WeatherCondition.Overcast => Color.DarkGreen,
                WeatherCondition.Rainy => Color.Blue,
                _ => Color.White
            };
            
            weatherChart.AddItem($"{weather.Condition}\n({weather.Percentage:F1}%)", weather.Count, colorEnum);
        }

        AnsiConsole.Write(weatherChart);

        // Weather statistics table (use ASCII border for compatibility)
        var statsTable = new Table()
            .Border(TableBorder.Ascii)
            .Title($"[bold]Weather Statistics - Year {selectedYear}[/]")
            .AddColumn("[yellow]Metric[/]")
            .AddColumn("[green]Value[/]");

        var avgTemp = yearData.Average(d => d.MS.AverageTemp);
        var maxTemp = yearData.Max(d => d.MS.MaxTemp);
        var minTemp = yearData.Min(d => d.MS.MinTemp);
        var totalPrecip = yearData.Sum(d => d.MS.Precipitation);
        var totalSunshine = yearData.Sum(d => d.MS.SunshineHours);
        var avgWindSpeed = yearData.Average(d => d.MS.WindSpeed);
        var maxWindSpeed = yearData.Max(d => d.MS.WindSpeed);

        statsTable.AddRow("Average Temperature", $"{avgTemp:F1}°C");
        statsTable.AddRow("Temperature Range", $"{minTemp:F1}°C to {maxTemp:F1}°C");
        statsTable.AddRow("Total Precipitation", $"{totalPrecip:F1}mm");
        statsTable.AddRow("Total Sunshine Hours", $"{totalSunshine:F1} hours");
        statsTable.AddRow("Average Wind Speed", $"{avgWindSpeed:F1} km/h");
        statsTable.AddRow("Max Wind Speed", $"{maxWindSpeed:F1} km/h");
        statsTable.AddRow("Sunny Days", $"{weatherDistribution.FirstOrDefault(w => w.Condition == WeatherCondition.Sunny)?.Count ?? 0} days");

        AnsiConsole.Write(statsTable);

        // Production by weather condition
        var productionByWeather = yearData
            .GroupBy(d => d.MS.Condition)
            .Select(g => new
            {
                Condition = g.Key,
                AvgProduction = g.Average(d => d.P),
                TotalProduction = g.Sum(d => d.P),
                Days = g.Count()
            })
            .OrderByDescending(pw => pw.AvgProduction)
            .ToList();

        var productionTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Production by Weather Condition[/]")
            .AddColumn("[yellow]Condition[/]")
            .AddColumn("[green]Avg Daily Production[/]")
            .AddColumn("[blue]Total Production[/]")
            .AddColumn("[gray]Days[/]");

        foreach (var prod in productionByWeather)
        {
            productionTable.AddRow(
                prod.Condition.ToString(),
                $"{prod.AvgProduction:F2} kWh",
                $"{prod.TotalProduction:F2} kWh",
                prod.Days.ToString()
            );
        }

        AnsiConsole.Write(productionTable);

        // Weather extremes
        var hottestDay = yearData.OrderByDescending(d => d.MS.MaxTemp).First();
        var coldestDay = yearData.OrderBy(d => d.MS.MinTemp).First();
        var rainiestDay = yearData.OrderByDescending(d => d.MS.Precipitation).First();
        var sunniestDay = yearData.OrderByDescending(d => d.MS.SunshineHours).First();

        var extremesPanel = new Panel(new Markup(
            $"[red]🌡 Hottest D: D {hottestDay.D} ({hottestDay.MS.MaxTemp:F1}°C) - {hottestDay.P:F2} kWh[/]\n" +
            $"[blue]❄ Coldest D: D {coldestDay.D} ({coldestDay.MS.MinTemp:F1}°C) - {coldestDay.P:F2} kWh[/]\n" +
            $"[cyan]🌧 Rainiest D: D {rainiestDay.D} ({rainiestDay.MS.Precipitation:F1}mm) - {rainiestDay.P:F2} kWh[/]\n" +
            $"[yellow]☀ Sunniest D: D {sunniestDay.D} ({sunniestDay.MS.SunshineHours:F1}h) - {sunniestDay.P:F2} kWh[/]"))
        {
            Header = new PanelHeader("[bold]Weather Extremes[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(extremesPanel);
    }

    private async Task DisplayCorrelationAnalysis(SolarData data, SolarDataService dataService, Settings options)
    {
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Weather vs Production Correlation Analysis - Year {selectedYear}[/]");

        var correlation = dataService.AnalyzeWeatherCorrelation(data, selectedYear);

        // Correlation strength visualization
        var correlationChart = new BarChart()
            .Width(80)
            .Label("[bold]Weather Factor Correlations with Production[/]")
            .CenterLabel();

        var correlations = new[]
        {
            ("Sunshine", Math.Abs(correlation.SunshineCorrelation)),
            ("Temperature", Math.Abs(correlation.TemperatureCorrelation)),
            ("Precipitation", Math.Abs(correlation.PrecipitationCorrelation)),
            ("Wind Speed", Math.Abs(correlation.WindCorrelation))
        };

        foreach (var (factor, strength) in correlations)
        {
            var color = strength switch
            {
                > 0.7 => Color.Green,
                > 0.5 => Color.Yellow,
                > 0.3 => Color.Orange1,
                _ => Color.Red
            };
            
            correlationChart.AddItem(factor, strength, color);
        }

        AnsiConsole.Write(correlationChart);

        // Detailed correlation table
        var correlationTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Correlation Analysis Details[/]")
            .AddColumn("[yellow]Weather Factor[/]")
            .AddColumn("[green]Correlation[/]")
            .AddColumn("[white]Strength[/]")
            .AddColumn("[blue]Interpretation[/]");

        var correlationData = new[]
        {
            ("Sunshine Hours", correlation.SunshineCorrelation, "Primary driver"),
            ("Temperature", correlation.TemperatureCorrelation, "Secondary factor"), 
            ("Precipitation", correlation.PrecipitationCorrelation, "Negative impact"),
            ("Wind Speed", correlation.WindCorrelation, "Cooling effect")
        };

        foreach (var (factor, corr, interpretation) in correlationData)
        {
            var strength = Math.Abs(corr) switch
            {
                > 0.7 => "Very Strong",
                > 0.5 => "Strong", 
                > 0.3 => "Moderate",
                > 0.1 => "Weak",
                _ => "Very Weak"
            };

            var corrColor = corr switch
            {
                > 0.5 => "green",
                > 0 => "yellow",
                > -0.5 => "orange",
                _ => "red"
            };

            correlationTable.AddRow(
                factor,
                $"[{corrColor}]{corr:F3}[/]",
                strength,
                interpretation
            );
        }

        AnsiConsole.Write(correlationTable);

        // Best and worst correlation days
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for detailed analysis.[/]");
            return;
        }

        var yearData = data[selectedYear].Where(d => d.P > 0).ToList();
        
        var bestSunDays = yearData.OrderByDescending(d => d.MS.SunshineHours).Take(5);
        var worstSunDays = yearData.OrderBy(d => d.MS.SunshineHours).Take(5);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]☀ Best Sunshine Days[/]");
        foreach (var day in bestSunDays)
        {
            AnsiConsole.MarkupLine($"  D {day.D}: [yellow]{day.MS.SunshineHours:F1}h sun[/] → [green]{day.P:F2} kWh[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold red]☁ Worst Sunshine Days[/]");
        foreach (var day in worstSunDays)
        {
            AnsiConsole.MarkupLine($"  D {day.D}: [gray]{day.MS.SunshineHours:F1}h sun[/] → [red]{day.P:F2} kWh[/]");
        }

        // Correlation insights
        var insightsPanel = new Panel(new Markup(
            $"[green]🔗 Strongest Positive Correlation: {correlations.OrderByDescending(c => c.Item2).First().Item1}[/]\n" +
            $"[yellow]📊 Overall Weather Impact: {(correlations.Average(c => c.Item2) * 100):F1}% predictive power[/]\n" +
            $"[blue]💡 Key Insight: {GetCorrelationInsight(correlation)}[/]"))
        {
            Header = new PanelHeader("[bold]Correlation Insights[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(insightsPanel);
    }

    private string GetCorrelationInsight(WeatherCorrelationAnalysis correlation)
    {
        if (correlation.SunshineCorrelation > 0.7)
            return "Sunshine hours are the primary predictor of solar production";
        else if (correlation.TemperatureCorrelation > 0.5)
            return "Temperature plays a significant role in solar efficiency";
        else if (Math.Abs(correlation.PrecipitationCorrelation) > 0.5)
            return "Precipitation significantly impacts solar production";
        else
            return "Weather patterns show complex interactions with solar production";
    }

    private async Task DisplayWeatherPatterns(SolarData data, Settings options)
    {
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
            return;
        }

        var yearData = data[selectedYear];

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Weather Pattern Analysis - Year {selectedYear}[/]");

        // Monthly weather patterns
        var monthlyWeather = yearData
            .GroupBy(d => (d.D - 1) / 30 + 1) // Rough month approximation
            .Select(g => new
            {
                Month = g.Key,
                AvgTemp = g.Average(d => d.MS.AverageTemp),
                TotalPrecip = g.Sum(d => d.MS.Precipitation),
                TotalSunshine = g.Sum(d => d.MS.SunshineHours),
                AvgProduction = g.Average(d => d.P),
                Days = g.Count()
            })
            .OrderBy(mw => mw.Month)
            .ToList();

        // Monthly patterns chart
        var monthlyChart = new BarChart()
            .Width(100)
            .Label("[bold]Monthly Average Temperature (°C)[/]")
            .CenterLabel();

        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        foreach (var month in monthlyWeather)
        {
            var colorEnum = month.AvgTemp switch
            {
                > 25 => Color.Red,
                > 15 => Color.Yellow,
                > 5 => Color.Green,
                _ => Color.Blue
            };
            
            monthlyChart.AddItem(monthNames[Math.Min(month.Month, 12)], month.AvgTemp, colorEnum);
        }

        AnsiConsole.Write(monthlyChart);

        // Monthly details table
        var monthlyTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Monthly Weather Patterns[/]")
            .AddColumn("[yellow]Month[/]")
            .AddColumn("[red]Avg Temp[/]")
            .AddColumn("[blue]Precipitation[/]")
            .AddColumn("[orange1]Sunshine[/]")
            .AddColumn("[green]Avg Production[/]");

        foreach (var month in monthlyWeather)
        {
            monthlyTable.AddRow(
                monthNames[Math.Min(month.Month, 12)],
                $"{month.AvgTemp:F1}°C",
                $"{month.TotalPrecip:F1}mm",
                $"{month.TotalSunshine:F1}h",
                $"{month.AvgProduction:F2} kWh"
            );
        }

        AnsiConsole.Write(monthlyTable);

        // Weather pattern insights
        var bestMonth = monthlyWeather.OrderByDescending(m => m.AvgProduction).First();
        var worstMonth = monthlyWeather.OrderBy(m => m.AvgProduction).First();
        var hottestMonth = monthlyWeather.OrderByDescending(m => m.AvgTemp).First();
        var rainiest = monthlyWeather.OrderByDescending(m => m.TotalPrecip).First();

        var patternsPanel = new Panel(new Markup(
            $"[green]🏆 Best Production Month: {monthNames[Math.Min(bestMonth.Month, 12)]} ({bestMonth.AvgProduction:F2} kWh avg)[/]\n" +
            $"[red]❗ Worst Production Month: {monthNames[Math.Min(worstMonth.Month, 12)]} ({worstMonth.AvgProduction:F2} kWh avg)[/]\n" +
            $"[yellow]🌡 Hottest Month: {monthNames[Math.Min(hottestMonth.Month, 12)]} ({hottestMonth.AvgTemp:F1}°C)[/]\n" +
            $"[blue]🌧 Rainiest Month: {monthNames[Math.Min(rainiest.Month, 12)]} ({rainiest.TotalPrecip:F1}mm)[/]"))
        {
            Header = new PanelHeader("[bold]Weather Pattern Highlights[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(patternsPanel);

        // Seasonal analysis
        var seasons = new[]
        {
            ("Spring", monthlyWeather.Where(m => m.Month >= 3 && m.Month <= 5)),
            ("Summer", monthlyWeather.Where(m => m.Month >= 6 && m.Month <= 8)),
            ("Autumn", monthlyWeather.Where(m => m.Month >= 9 && m.Month <= 11)),
            ("Winter", monthlyWeather.Where(m => m.Month == 12 || m.Month <= 2))
        };

        var seasonalTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Seasonal Weather Summary[/]")
            .AddColumn("[yellow]Season[/]")
            .AddColumn("[red]Avg Temp[/]")
            .AddColumn("[blue]Total Precip[/]")
            .AddColumn("[orange1]Total Sunshine[/]")
            .AddColumn("[green]Avg Production[/]");

        foreach (var (season, months) in seasons)
        {
            if (months.Any())
            {
                seasonalTable.AddRow(
                    season,
                    $"{months.Average(m => m.AvgTemp):F1}°C",
                    $"{months.Sum(m => m.TotalPrecip):F1}mm",
                    $"{months.Sum(m => m.TotalSunshine):F1}h",
                    $"{months.Average(m => m.AvgProduction):F2} kWh"
                );
            }
        }

        AnsiConsole.Write(seasonalTable);
    }

    private async Task DisplayRecommendations(SolarData data, SolarDataService dataService, Settings options)
    {
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        var correlation = dataService.AnalyzeWeatherCorrelation(data, selectedYear);
        
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
            return;
        }

        var yearData = data[selectedYear];

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]🎯 Weather-Based Solar Optimization Recommendations[/]");

        // Generate recommendations based on correlation analysis
        var recommendations = GenerateRecommendations(correlation, yearData);

        var recommendationsPanel = new Panel(new Markup(string.Join("\n\n", recommendations)))
        {
            Header = new PanelHeader("[bold]Optimization Recommendations[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("green")
        };

        AnsiConsole.Write(recommendationsPanel);

        // Weather-based maintenance suggestions
        var maintenancePanel = new Panel(new Markup(
            "[yellow]🔧 Seasonal Maintenance Recommendations:[/]\n\n" +
            "[green]Spring:[/] Clean panels after winter, check for damage from storms\n" +
            "[red]Summer:[/] Monitor for overheating, ensure adequate ventilation\n" +
            "[orange1]Autumn:[/] Clear debris and leaves, prepare for reduced daylight\n" +
            "[blue]Winter:[/] Remove snow accumulation, check for ice damage"))
        {
            Header = new PanelHeader("[bold]Seasonal Maintenance Guide[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("yellow")
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(maintenancePanel);

        // Performance prediction tips
        var predictionPanel = new Panel(new Markup(
            "[cyan]📊 Production Prediction Tips:[/]\n\n" +
            $"[white]• Sunny days typically produce: {yearData.Where(d => d.MS.Condition == WeatherCondition.Sunny).Average(d => d.P):F1} kWh[/]\n" +
            $"[gray]• Cloudy days typically produce: {yearData.Where(d => d.MS.Condition == WeatherCondition.Cloudy).Average(d => d.P):F1} kWh[/]\n" +
            $"[blue]• Rainy days typically produce: {yearData.Where(d => d.MS.Condition == WeatherCondition.Rainy).Average(d => d.P):F1} kWh[/]\n\n" +
            "[yellow]💡 Use weather forecasts to predict daily production and plan energy usage accordingly![/]"))
        {
            Header = new PanelHeader("[bold]Production Forecasting[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(predictionPanel);
    }

    private List<string> GenerateRecommendations(WeatherCorrelationAnalysis correlation, List<BarChartData> yearData)
    {
        var recommendations = new List<string>();

        // Sunshine correlation recommendations
        if (correlation.SunshineCorrelation > 0.7)
        {
            recommendations.Add("[green]☀ Strong sunshine correlation detected![/]\n" +
                              "   • Optimize panel positioning for maximum sun exposure\n" +
                              "   • Consider tracking systems for better sun following\n" +
                              "   • Clear any obstructions that cast shadows");
        }
        else if (correlation.SunshineCorrelation < 0.5)
        {
            recommendations.Add("[orange1]☁ Low sunshine correlation suggests potential issues[/]\n" +
                              "   • Check for panel soiling or degradation\n" +
                              "   • Verify inverter performance and efficiency\n" +
                              "   • Consider professional system inspection");
        }

        // Temperature correlation recommendations
        if (correlation.TemperatureCorrelation < -0.3)
        {
            recommendations.Add("[blue]🌡 Negative temperature correlation detected[/]\n" +
                              "   • High temperatures reduce panel efficiency\n" +
                              "   • Improve ventilation around panels\n" +
                              "   • Consider cooling systems for extreme heat");
        }
        else if (correlation.TemperatureCorrelation > 0.5)
        {
            recommendations.Add("[red]🔥 Positive temperature correlation is unusual[/]\n" +
                              "   • Review system performance data\n" +
                              "   • Check for measurement errors or data issues\n" +
                              "   • Consider system inspection");
        }

        // Precipitation recommendations
        if (Math.Abs(correlation.PrecipitationCorrelation) > 0.5)
        {
            recommendations.Add("[cyan]🌧 Significant precipitation impact detected[/]\n" +
                              "   • Rain can clean panels naturally\n" +
                              "   • Ensure proper drainage around installation\n" +
                              "   • Monitor for water damage or corrosion");
        }

        // Wind speed recommendations
        if (correlation.WindCorrelation > 0.3)
        {
            recommendations.Add("[gray]💨 Wind provides beneficial cooling effect[/]\n" +
                              "   • Maintain clear airflow around panels\n" +
                              "   • Ensure mounting systems can handle wind loads\n" +
                              "   • Consider wind as a positive factor for efficiency");
        }

        // General system recommendations
        var avgProduction = yearData.Average(d => d.P);
        var efficiency = yearData.Average(d => d.Efficiency);

        if (avgProduction < 15)
        {
            recommendations.Add("[red]⚡ Below-average production detected[/]\n" +
                              "   • Schedule comprehensive system evaluation\n" +
                              "   • Check for component failures or degradation\n" +
                              "   • Consider system upgrades or expansion");
        }

        if (efficiency < 80)
        {
            recommendations.Add("[orange1]📉 System efficiency could be improved[/]\n" +
                              "   • Optimize energy consumption patterns\n" +
                              "   • Consider battery storage for excess production\n" +
                              "   • Review appliance usage during peak production");
        }

        return recommendations;
    }
}
