using Spectre.Console;
using SolarScope.Models;
using SolarScope.Services;

namespace SolarScope.Commands;

/// <summary>
/// Report command implementation
/// </summary>
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

        AnsiConsole.Clear();
        
        // Display header
        var headerRule = new Rule($"[bold yellow]📊 Solar System Report - {options.Period.ToUpper()} 📊[/]")
        {
            Style = Style.Parse("yellow"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(headerRule);

        switch (options.Period.ToLower())
        {
            case "daily":
                await DisplayDailyReport(data, options);
                break;
            case "weekly":
                await DisplayWeeklyReport(data, options);
                break;
            case "monthly":
                await DisplayMonthlyReport(data, options);
                break;
            case "yearly":
                await DisplayYearlyReport(data, options);
                break;
            default:
                AnsiConsole.MarkupLine("[red]Invalid period. Use: daily, weekly, monthly, or yearly[/]");
                break;
        }
    }

    private async Task DisplayDailyReport(SolarData data, ReportOptions options)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Daily Production Report[/]");
        
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
            return;
        }

        var yearData = data[selectedYear];
        var startDay = options.StartDay ?? 1;
        var endDay = options.EndDay ?? Math.Min(startDay + 30, yearData.Max(d => d.D));
        
        var filteredData = yearData
            .Where(d => d.D >= startDay && d.D <= endDay)
            .OrderBy(d => d.D)
            .ToList();

        if (!filteredData.Any())
        {
            AnsiConsole.MarkupLine("[red]No data available for the specified day range.[/]");
            return;
        }

        // Create production chart
        var productionChart = new BarChart()
            .Width(100)
            .Label("[bold]Daily Production (kWh)[/]")
            .CenterLabel();

        foreach (var day in filteredData.Take(20)) // Limit to 20 days for readability
        {
            var color = day.P switch
            {
                > 30 => Color.Green,
                > 20 => Color.Yellow,
                > 10 => Color.Orange1,
                _ => Color.Red
            };
            
            productionChart.AddItem($"D {day.D}", day.P, color);
        }

        AnsiConsole.Write(productionChart);

        // Daily statistics table
        var statsTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Daily Statistics Summary[/]")
            .AddColumn("[yellow]Metric[/]")
            .AddColumn("[green]Value[/]");

        var totalProduction = filteredData.Sum(d => d.P);
        var totalConsumption = filteredData.Sum(d => d.U);
        var totalInjection = filteredData.Sum(d => d.I);
        var avgDaily = filteredData.Average(d => d.P);
        var maxDaily = filteredData.Max(d => d.P);
        var minDaily = filteredData.Min(d => d.P);

        statsTable.AddRow("Total Production", $"{totalProduction:F2} kWh");
        statsTable.AddRow("Total Consumption", $"{totalConsumption:F2} kWh");
        statsTable.AddRow("Total Grid Injection", $"{totalInjection:F2} kWh");
        statsTable.AddRow("Average Daily Production", $"{avgDaily:F2} kWh");
        statsTable.AddRow("Max Daily Production", $"{maxDaily:F2} kWh");
        statsTable.AddRow("Min Daily Production", $"{minDaily:F2} kWh");
        statsTable.AddRow("Energy Efficiency", $"{(totalConsumption / totalProduction * 100):F1}%");

        AnsiConsole.Write(statsTable);

        // Top and bottom performing days
        var topDays = filteredData.OrderByDescending(d => d.P).Take(3);
        var bottomDays = filteredData.OrderBy(d => d.P).Take(3);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]🏆 Best Performing Days[/]");
        foreach (var day in topDays)
        {
            AnsiConsole.MarkupLine($"  D {day.D}: [green]{day.P:F2} kWh[/] " +
                                 $"(Weather: {day.MS.Condition}, {day.MS.SunshineHours:F1}h sun)");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold red]⚠️ Lowest Performing Days[/]");
        foreach (var day in bottomDays)
        {
            AnsiConsole.MarkupLine($"  D {day.D}: [red]{day.P:F2} kWh[/] " +
                                 $"(Weather: {day.MS.Condition}, {day.MS.SunshineHours:F1}h sun)");
        }
    }

    private async Task DisplayWeeklyReport(SolarData data, ReportOptions options)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Weekly Aggregated Report[/]");
        
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
            return;
        }

        var yearData = data[selectedYear];
        
        // Group by weeks (7-day periods)
        var weeklyData = yearData
            .GroupBy(d => (d.D - 1) / 7 + 1)
            .Select(g => new
            {
                Week = g.Key,
                Days = g.ToList(),
                TotalProduction = g.Sum(d => d.P),
                TotalConsumption = g.Sum(d => d.U),
                TotalInjection = g.Sum(d => d.I),
                AvgProduction = g.Average(d => d.P),
                DayCount = g.Count()
            })
            .OrderBy(w => w.Week)
            .ToList();

        // Weekly chart
        var weeklyChart = new BarChart()
            .Width(100)
            .Label("[bold]Weekly Production (kWh)[/]")
            .CenterLabel();

        foreach (var week in weeklyData.Take(15)) // Show first 15 weeks
        {
            var color = week.TotalProduction switch
            {
                > 150 => Color.Green,
                > 100 => Color.Yellow,
                > 50 => Color.Orange1,
                _ => Color.Red
            };
            
            weeklyChart.AddItem($"W{week.Week}", week.TotalProduction, color);
        }

        AnsiConsole.Write(weeklyChart);

        // Weekly statistics table
        var weeklyTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Weekly Performance Summary[/]")
            .AddColumn("[yellow]Week[/]")
            .AddColumn("[green]Production[/]")
            .AddColumn("[blue]Consumption[/]")
            .AddColumn("[orange]Injection[/]")
            .AddColumn("[white]Avg/D[/]")
            .AddColumn("[gray]Days[/]");

        foreach (var week in weeklyData.Take(10))
        {
            weeklyTable.AddRow(
                $"Week {week.Week}",
                $"{week.TotalProduction:F1} kWh",
                $"{week.TotalConsumption:F1} kWh",
                $"{week.TotalInjection:F1} kWh",
                $"{week.AvgProduction:F1} kWh",
                week.DayCount.ToString()
            );
        }

        AnsiConsole.Write(weeklyTable);

        // Best and worst weeks
        var bestWeek = weeklyData.OrderByDescending(w => w.TotalProduction).First();
        var worstWeek = weeklyData.OrderBy(w => w.TotalProduction).First();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]🏆 Best Week: Week {bestWeek.Week}[/] - {bestWeek.TotalProduction:F2} kWh");
        AnsiConsole.MarkupLine($"[bold red]⚠️ Worst Week: Week {worstWeek.Week}[/] - {worstWeek.TotalProduction:F2} kWh");
    }

    private async Task DisplayMonthlyReport(SolarData data, ReportOptions options)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Monthly Aggregated Report[/]");
        
        var selectedYear = options.Year ?? data.AvailableYears.FirstOrDefault();
        if (!data.ContainsKey(selectedYear))
        {
            AnsiConsole.MarkupLine("[red]No data available for the selected year.[/]");
            return;
        }

        var dataService = new SolarDataService(options.DataFile);
        var monthlyStats = dataService.GetMonthlyStatistics(data, selectedYear);

        if (!monthlyStats.Any())
        {
            AnsiConsole.MarkupLine("[red]No monthly data available.[/]");
            return;
        }

        // Monthly chart
        var monthlyChart = new BarChart()
            .Width(100)
            .Label("[bold]Monthly Production (kWh)[/]")
            .CenterLabel();

        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        foreach (var month in monthlyStats.OrderBy(m => m.Key))
        {
            var color = month.Value.TotalProduction switch
            {
                > 400 => Color.Green,
                > 300 => Color.Yellow,
                > 200 => Color.Orange1,
                _ => Color.Red
            };
            
            monthlyChart.AddItem(monthNames[month.Key], month.Value.TotalProduction, color);
        }

        AnsiConsole.Write(monthlyChart);

        // Monthly table
        var monthlyTable = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]Monthly Statistics - Year {selectedYear}[/]")
            .AddColumn("[yellow]Month[/]")
            .AddColumn("[green]Production[/]")
            .AddColumn("[blue]Consumption[/]")
            .AddColumn("[orange]Injection[/]")
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

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]🏆 Best Month: {monthNames[bestMonth.Key]}[/] - {bestMonth.Value.TotalProduction:F2} kWh");
        AnsiConsole.MarkupLine($"[bold red]⚠️ Worst Month: {monthNames[worstMonth.Key]}[/] - {worstMonth.Value.TotalProduction:F2} kWh");
    }

    private async Task DisplayYearlyReport(SolarData data, ReportOptions options)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Yearly Overview Report[/]");

        if (!data.AvailableYears.Any())
        {
            AnsiConsole.MarkupLine("[red]No yearly data available.[/]");
            return;
        }

        // Yearly comparison chart
        var yearlyChart = new BarChart()
            .Width(100)
            .Label("[bold]Yearly Production Comparison (kWh)[/]")
            .CenterLabel();

        foreach (var year in data.AvailableYears)
        {
            var (production, consumption, injection) = data.GetYearlyTotals(year);
            var color = production switch
            {
                > 8000 => Color.Green,
                > 6000 => Color.Yellow,
                > 4000 => Color.Orange1,
                _ => Color.Red
            };
            
            yearlyChart.AddItem(year.ToString(), production, color);
        }

        AnsiConsole.Write(yearlyChart);

        // Yearly statistics table
        var yearlyTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Multi-Year Performance Comparison[/]")
            .AddColumn("[yellow]Year[/]")
            .AddColumn("[green]Production[/]")
            .AddColumn("[blue]Consumption[/]")
            .AddColumn("[orange]Injection[/]")
            .AddColumn("[white]Efficiency[/]")
            .AddColumn("[gray]Days[/]");

        foreach (var year in data.AvailableYears)
        {
            var (production, consumption, injection) = data.GetYearlyTotals(year);
            var efficiency = production > 0 ? (consumption / production * 100) : 0;
            var dayCount = data[year].Count;

            yearlyTable.AddRow(
                year.ToString(),
                $"{production:F1} kWh",
                $"{consumption:F1} kWh",
                $"{injection:F1} kWh",
                $"{efficiency:F1}%",
                dayCount.ToString()
            );
        }

        AnsiConsole.Write(yearlyTable);

        // Overall system performance
        var (totalProd, totalCons, totalInj) = data.YearlyTotals;
        var overallEfficiency = totalProd > 0 ? (totalCons / totalProd * 100) : 0;

        var summaryPanel = new Panel(new Markup(
            $"[green]Total System Production: {totalProd:F2} kWh[/]\n" +
            $"[blue]Total System Consumption: {totalCons:F2} kWh[/]\n" +
            $"[orange]Total Grid Injection: {totalInj:F2} kWh[/]\n" +
            $"[white]Overall System Efficiency: {overallEfficiency:F1}%[/]\n" +
            $"[yellow]Total Data Points: {data.TotalDays} days[/]\n" +
            $"[gray]Years Covered: {string.Join(", ", data.AvailableYears)}[/]"))
        {
            Header = new PanelHeader("[bold]Overall System Performance[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(summaryPanel);
    }
}
