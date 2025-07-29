using Spectre.Console;
using SolarScope.Models;
using SolarScope.Services;

namespace SolarScope.Commands;

public class ReportCommand
{
    public async Task ExecuteAsync(ReportOptions options)
    {
        var dataService = new SolarDataService(options.DataFile);
        var data = await dataService.LoadDataAsync();
        
        if (data == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return;
        }

        AnsiConsole.MarkupLine($"[cyan]📋 Generating {options.ReportType} report in {options.Format} format...[/]");
        AnsiConsole.WriteLine();

        switch (options.ReportType.ToLower())
        {
            case "monthly":
                await GenerateMonthlyReport(dataService, data, options);
                break;
            case "yearly":
                await GenerateYearlyReport(data, options);
                break;
            default:
                AnsiConsole.MarkupLine($"[yellow]Report type '{options.ReportType}' not yet implemented[/]");
                break;
        }
    }

    private async Task GenerateMonthlyReport(SolarDataService dataService, SolarData data, ReportOptions options)
    {
        var monthlyStats = dataService.GetMonthlyStatistics(data);
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue);

        table.AddColumn("[bold]Month[/]");
        table.AddColumn("[bold]Days[/]");
        table.AddColumn("[bold]Total Production[/]");
        table.AddColumn("[bold]Avg Daily Production[/]");
        table.AddColumn("[bold]Energy Balance[/]");

        foreach (var kvp in monthlyStats.OrderBy(x => x.Key))
        {
            var stats = kvp.Value;
            var balanceColor = stats.EnergyBalance >= 0 ? "green" : "red";
            
            table.AddRow(
                $"Month {kvp.Key}",
                stats.DaysWithData.ToString(),
                $"{stats.TotalProduction:F1} kWh",
                $"{stats.AverageProductionPerDay:F1} kWh",
                $"[{balanceColor}]{stats.EnergyBalance:+0.0;-0.0} kWh[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    private async Task GenerateYearlyReport(SolarData data, ReportOptions options)
    {
        var (totalProduction, totalConsumption, totalInjection) = data.YearlyTotals;
        
        var yearlyPanel = new Panel(new Markup(
            $"[green]📊 Total Production: {totalProduction:F2} kWh[/]\n" +
            $"[blue]⚡ Total Consumption: {totalConsumption:F2} kWh[/]\n" +
            $"[cyan]🔌 Grid Injection: {totalInjection:F2} kWh[/]\n" +
            $"[yellow]⚖️ Energy Balance: {(totalProduction - totalConsumption):+0.0;-0.0} kWh[/]\n" +
            $"[magenta]📅 Days Monitored: {data.TotalDays}[/]"))
        {
            Header = new PanelHeader("[bold]2023 Annual Solar Report[/]"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(yearlyPanel);
    }
}

public class AnomaliesCommand
{
    public async Task ExecuteAsync(AnomaliesOptions options)
    {
        var dataService = new SolarDataService(options.DataFile);
        var data = await dataService.LoadDataAsync();
        
        if (data == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return;
        }

        var severity = ParseSeverity(options.Severity);
        var anomalousData = dataService.GetAnomalousData(data, severity);

        if (anomalousData.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]✅ No anomalies detected at the specified severity level![/]");
            return;
        }

        AnsiConsole.MarkupLine($"[red]⚠️ Found {anomalousData.Count} anomalies with {options.Severity} or higher severity[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Red);

        table.AddColumn("[bold]Day[/]");
        table.AddColumn("[bold]Severity[/]");
        table.AddColumn("[bold]Anomaly Score[/]");
        table.AddColumn("[bold]Description[/]");

        foreach (var day in anomalousData.Take(10))
        {
            var severityColor = day.AnomalyStats.Severity switch
            {
                AnomalySeverity.High => "red",
                AnomalySeverity.Medium => "orange",
                _ => "yellow"
            };

            table.AddRow(
                $"Day {day.Day}",
                $"[{severityColor}]{day.AnomalyStats.Severity}[/]",
                $"{day.AnomalyStats.TotalAnomalyScore:F2}",
                GetAnomalyDescription(day)
            );
        }

        AnsiConsole.Write(table);
    }

    private AnomalySeverity ParseSeverity(string severity)
    {
        return severity.ToLower() switch
        {
            "high" => AnomalySeverity.High,
            "medium" => AnomalySeverity.Medium,
            "low" => AnomalySeverity.Low,
            _ => AnomalySeverity.None
        };
    }

    private string GetAnomalyDescription(BarChartData day)
    {
        if (day.AnomalyStats.ProductionAnomaly < -5) return "Low production anomaly";
        if (day.AnomalyStats.ConsumptionAnomaly > 5) return "High consumption anomaly";
        if (day.WeatherStats.Condition == WeatherCondition.Rainy) return "Weather-related";
        return "System irregularity";
    }
}

public class WeatherCommand
{
    public async Task ExecuteAsync(WeatherOptions options)
    {
        var dataService = new SolarDataService(options.DataFile);
        var data = await dataService.LoadDataAsync();
        
        if (data == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return;
        }

        AnsiConsole.MarkupLine("[blue]🌤️ Weather Analysis Report[/]");
        AnsiConsole.WriteLine();

        if (options.ShowCorrelation)
        {
            var correlation = dataService.AnalyzeWeatherCorrelation(data);
            
            AnsiConsole.MarkupLine($"[cyan]☀️ Sunshine correlation: {correlation.SunshineCorrelation:F3}[/]");
            AnsiConsole.MarkupLine($"[yellow]🌡️ Temperature correlation: {correlation.TemperatureCorrelation:F3}[/]");
            AnsiConsole.MarkupLine($"[blue]🌧️ Precipitation correlation: {correlation.PrecipitationCorrelation:F3}[/]");
            AnsiConsole.MarkupLine($"[green]💨 Wind correlation: {correlation.WindCorrelation:F3}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]🎯 Strongest factor: {correlation.GetStrongestCorrelation()}[/]");
        }
        else
        {
            var weatherStats = data.Year2023.Select(d => d.WeatherStats).ToList();
            var avgTemp = weatherStats.Average(w => w.AverageTemp);
            var totalPrecip = weatherStats.Sum(w => w.Precipitation);
            
            AnsiConsole.MarkupLine($"[yellow]🌡️ Average Temperature: {avgTemp:F1}°C[/]");
            AnsiConsole.MarkupLine($"[blue]🌧️ Total Precipitation: {totalPrecip:F1}mm[/]");
            AnsiConsole.MarkupLine($"[orange]☀️ Total Sunshine: {weatherStats.Sum(w => w.SunshineHours):F1} hours[/]");
        }
    }
}

public class ExploreCommand
{
    public async Task ExecuteAsync(ExploreOptions options)
    {
        var dataService = new SolarDataService(options.DataFile);
        var data = await dataService.LoadDataAsync();
        
        if (data == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return;
        }

        AnsiConsole.MarkupLine("[cyan]🔍 Interactive Solar Data Explorer[/]");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[dim]Note: Full interactive features coming soon![/]");
        AnsiConsole.MarkupLine("[yellow]For now, try: solarscope dashboard --full[/]");
        AnsiConsole.MarkupLine("[yellow]Or: solarscope analyze --type production[/]");
    }
}
