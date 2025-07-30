using Spectre.Console;
using Spectre.Console.Cli;
using SolarScope.Models;
using SolarScope.Services;
using System.ComponentModel;

namespace SolarScope.Commands;

/// <summary>
/// Demo command with fun animations and themes (Spectre.Console.Cli)
/// </summary>
public class DemoCommand : AsyncCommand<DemoCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("--theme|-t")]
        [Description("Demo theme: solar, matrix, rainbow")]
        public string Theme { get; set; } = "solar";

        [CommandOption("--speed|-s")]
        [Description("Animation speed: slow, normal, fast")]
        public string Speed { get; set; } = "normal";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var dataService = new SolarDataService(settings.DataFile);
        var data = await dataService.LoadDataAsync();

        if (data == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load solar data![/]");
            return 1;
        }

        var speed = GetAnimationSpeed(settings.Speed);

        switch (settings.Theme.ToLower())
        {
            case "solar":
                await SolarThemeDemo(data, speed);
                break;
            case "matrix":
                await MatrixThemeDemo(data, speed);
                break;
            case "rainbow":
                await RainbowThemeDemo(data, speed);
                break;
            default:
                await SolarThemeDemo(data, speed);
                break;
        }
        return 0;
    }

    private async Task SolarThemeDemo(SolarData data, int speed)
    {
        AnsiConsole.Clear();
        
        // Animated solar system ASCII art
        await DisplayAnimatedSolarSystem(speed);
        
        // Welcome animation
        await TypewriterEffect("🌞 Welcome to SolarScope - Your Solar Journey Begins! 🌞", speed);
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        // Animated data loading
        await SimulateDataLoading(data, speed);
        
        // Solar production animation
        await AnimatedProductionVisualization(data, speed);
        
        // Weather effects animation
        await WeatherEffectsDemo(data, speed);
        
        // Finale
        await SolarFinale(speed);
    }

    private async Task MatrixThemeDemo(SolarData data, int speed)
    {
        AnsiConsole.Clear();
        
        // Matrix-style intro
        await MatrixRainIntro(speed);
        
        await TypewriterEffect("[green]SOLAR MATRIX INITIALIZED...[/]", speed);
        await TypewriterEffect("[green]ACCESSING ENERGY GRID...[/]", speed);
        await TypewriterEffect("[green]LOADING POWER MATRIX...[/]", speed);
        AnsiConsole.WriteLine();

        // Matrix-style data display
        await MatrixDataVisualization(data, speed);
        
        // Glitch effects
        await MatrixGlitchEffect(speed);
        
        await TypewriterEffect("[green]SOLAR MATRIX COMPLETE. SYSTEM OPTIMAL.[/]", speed);
    }

    private async Task RainbowThemeDemo(SolarData data, int speed)
    {
        AnsiConsole.Clear();
        
        // Rainbow intro
        await RainbowIntro(speed);
        
        // Colorful data presentation
        await RainbowDataVisualization(data, speed);
        
        // Dancing charts
        await RainbowChartAnimation(data, speed);
        
        // Rainbow finale
        await RainbowFinale(speed);
    }

    private async Task DisplayAnimatedSolarSystem(int speed)
    {
        var frames = new[]
        {
            "        ☀️        \n      🌍    🔋   \n    ⚡  📊  ⚡   ",
            "        🌞        \n      🌍    🔋   \n    ⚡  📈  ⚡   ",
            "        ⭐        \n      🌍    🔋   \n    ⚡  📊  ⚡   ",
            "        ✨        \n      🌍    🔋   \n    ⚡  📈  ⚡   "
        };

        for (int i = 0; i < 12; i++)
        {
            AnsiConsole.Clear();
            var frame = frames[i % frames.Length];
            
            var panel = new Panel(frame)
            {
                Header = new PanelHeader("[bold yellow]Solar System Dashboard[/]"),
                Border = BoxBorder.Double,
                BorderStyle = Style.Parse("yellow"),
                Padding = new Padding(2, 1)
            };
            
            AnsiConsole.Write(Align.Center(panel));
            await Task.Delay(speed * 2);
        }
        
        AnsiConsole.Clear();
    }

    private async Task TypewriterEffect(string text, int speed, bool newLine = true)
    {
        foreach (var word in text.Split(' '))
    {
        // If the word is all letters/numbers, type it char by char
        if (word.All(char.IsLetterOrDigit))
        {
            foreach (char c in word)
            {
                AnsiConsole.Markup(Markup.Escape(c.ToString()));
                await Task.Delay(speed / 2);
            }
            AnsiConsole.Markup(" ");
        }
        else
        {
            // For emoji or non-word, print as a whole
            AnsiConsole.Markup(Markup.Escape(word) + " ");
            await Task.Delay(speed);
        }
    }

        if (newLine) AnsiConsole.WriteLine();
    }

    private async Task SimulateDataLoading(SolarData data, int speed)
    {
        var progressTask = AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(Spinner.Known.Star)
            );

        await progressTask.StartAsync(async ctx =>
        {
            var task1 = ctx.AddTask("[yellow]Loading solar data[/]");
            var task2 = ctx.AddTask("[blue]Analyzing weather patterns[/]");
            var task3 = ctx.AddTask("[green]Processing energy metrics[/]");
            var task4 = ctx.AddTask("[red]Detecting anomalies[/]");

            var tasks = new[] { task1, task2, task3, task4 };
            
            while (!ctx.IsFinished)
            {
                foreach (var task in tasks)
                {
                    task.Increment(Random.Shared.Next(1, 5));
                    await Task.Delay(speed);
                }
            }
        });
        
        AnsiConsole.WriteLine();
        await TypewriterEffect($"✅ Loaded {data.TotalDays} days of solar data successfully!", speed);
        AnsiConsole.WriteLine();
    }

    private async Task AnimatedProductionVisualization(SolarData data, int speed)
    {
        await TypewriterEffect("📊 Visualizing energy production patterns...", speed);
        AnsiConsole.WriteLine();

        var recentDays = data.GetLatestYearData().TakeLast(10).ToList();
        
        await AnsiConsole.Live(new Panel("Initializing..."))
            .StartAsync(async ctx =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    var chart = new BarChart()
                        .Width(60)
                        .Label("[green bold]Daily Production Animation[/]")
                        .CenterLabel();

                    foreach (var day in recentDays.Take(i))
                    {
                        var color = day.P switch
                        {
                            > 15 => Color.Green,
                            > 10 => Color.Yellow,
                            _ => Color.Orange1
                        };
                        
                        chart.AddItem($"D {day.D}", day.P, color);
                    }

                    var panel = new Panel(chart)
                    {
                        Header = new PanelHeader($"[bold]Loading D {i}/10[/]"),
                        Border = BoxBorder.Rounded
                    };

                    ctx.UpdateTarget(panel);
                    await Task.Delay(speed * 3);
                }
            });

        AnsiConsole.WriteLine();
    }

    private async Task WeatherEffectsDemo(SolarData data, int speed)
    {
        await TypewriterEffect("🌤️ Simulating weather effects on solar production...", speed);
        AnsiConsole.WriteLine();

        var weatherEmojis = new Dictionary<WeatherCondition, string>
        {
            [WeatherCondition.Sunny] = "☀️",
            [WeatherCondition.PartlyCloudy] = "⛅",
            [WeatherCondition.Cloudy] = "☁️",
            [WeatherCondition.Overcast] = "🌫️",
            [WeatherCondition.Rainy] = "🌧️"
        };

        var weatherDays = data.GetLatestYearData().Take(7).ToList();
        
        foreach (var day in weatherDays)
        {
            var emoji = weatherEmojis[day.MS.Condition];
            var productionBar = new string('█', (int)(day.P / 2));
            var color = day.P switch
            {
                > 15 => "green",
                > 10 => "yellow",
                _ => "red"
            };

            AnsiConsole.MarkupLine($"{emoji} D {day.D}: [{color}]{productionBar}[/] {day.P:F1} kWh");
            await Task.Delay(speed * 4);
        }
        
        AnsiConsole.WriteLine();
    }

    private async Task SolarFinale(int speed)
    {
        AnsiConsole.WriteLine();
        await TypewriterEffect("🎉 Solar analysis complete! Your system is generating clean energy! 🎉", speed);
        AnsiConsole.WriteLine();

        // Celebration animation
        var celebrationFrames = new[] { "🎉", "🎊", "✨", "🌟", "⭐", "💫" };
        
        for (int i = 0; i < 20; i++)
        {
            var frame = celebrationFrames[i % celebrationFrames.Length];
            AnsiConsole.Markup($"\r{frame} Thank you for using SolarScope! {frame}");
            await Task.Delay(speed * 2);
        }
        
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
        
        var rule = new Rule("[bold green]Demo Complete - Keep Shining! ☀️[/]")
        {
            Style = Style.Parse("green"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
    }

    private async Task MatrixRainIntro(int speed)
    {
        var random = new Random();
        var width = Console.WindowWidth;
        var height = 15;
        
        for (int frame = 0; frame < 30; frame++)
        {
            AnsiConsole.Clear();
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    if (random.Next(100) < 3)
                    {
                        var intensity = random.Next(3);
                        var color = intensity switch
                        {
                            0 => "green3",
                            1 => "green",
                            _ => "lime"
                        };
                        
                        var chars = "01※⚡☀️🔋⚙️";
                        var char1 = chars[random.Next(chars.Length)];
                        AnsiConsole.Markup($"[{color}]{char1}[/]");
                    }
                    else
                    {
                        AnsiConsole.Markup(" ");
                    }
                }
                AnsiConsole.WriteLine();
            }
            
            await Task.Delay(speed);
        }
    }

    private async Task MatrixDataVisualization(SolarData data, int speed)
    {
        var (totalProduction, totalConsumption, totalInjection) = data.YearlyTotals;
        
        var matrixData = new[]
        {
            $"[green]TOTAL_PRODUCTION: {totalProduction:F2} KWH[/]",
            $"[green]TOTAL_CONSUMPTION: {totalConsumption:F2} KWH[/]",
            $"[green]GRID_INJECTION: {totalInjection:F2} KWH[/]",
            $"[green]SYSTEM_STATUS: OPTIMAL[/]",
            $"[green]ANOMALIES_DETECTED: {data.GetLatestYearData().Count(d => d.AS.HasAnomaly)}[/]",
            $"[green]EFFICIENCY_RATING: HIGH[/]"
        };

        foreach (var line in matrixData)
        {
            await TypewriterEffect(line, speed / 2);
            await Task.Delay(speed);
        }
    }

    private async Task MatrixGlitchEffect(int speed)
    {
        AnsiConsole.WriteLine();
        
        var glitchFrames = new[]
        {
            "[red]ERROR: ANOMALY DETECTED[/]",
            "[green]RECALIBRATING...[/]",
            "[yellow]█▓░ PROCESSING ░▓█[/]",
            "[green]SYSTEM RESTORED[/]"
        };

        foreach (var frame in glitchFrames)
        {
            AnsiConsole.MarkupLine(frame);
            await Task.Delay(speed * 3);
        }
    }

    private async Task RainbowIntro(int speed)
    {
        var colors = new[] { "red", "orange1", "yellow", "green", "blue", "purple", "magenta" };
        var text = "🌈 RAINBOW SOLAR SPECTACULAR 🌈";
        
        for (int i = 0; i < 10; i++)
        {
            AnsiConsole.Clear();
            var coloredText = "";
            
            for (int j = 0; j < text.Length; j++)
            {
                var colorIndex = (i + j) % colors.Length;
                coloredText += $"[{colors[colorIndex]}]{text[j]}[/]";
            }
            
            AnsiConsole.Write(Align.Center(new Markup(coloredText)));
            await Task.Delay(speed * 2);
        }
        
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
    }

    private async Task RainbowDataVisualization(SolarData data, int speed)
    {
        var colors = new[] { "red", "orange1", "yellow", "green", "cyan", "blue", "magenta" };
        var (totalProduction, totalConsumption, totalInjection) = data.YearlyTotals;
        
        var dataPoints = new[]
        {
            ("Production", totalProduction, "⚡"),
            ("Consumption", totalConsumption, "🏠"),
            ("Grid Injection", totalInjection, "🔌"),
            ("Efficiency", totalConsumption / totalProduction * 100, "📊"),
            ("Days Monitored", data.TotalDays, "📅")
        };

        for (int i = 0; i < dataPoints.Length; i++)
        {
            var (label, value, emoji) = dataPoints[i];
            var color = colors[i % colors.Length];
            
            await TypewriterEffect($"[{color}]{emoji} {label}: {value:F1}[/]", speed);
            await Task.Delay(speed);
        }
    }

    private async Task RainbowChartAnimation(SolarData data, int speed)
    {
        AnsiConsole.WriteLine();
        await TypewriterEffect("🎨 Creating rainbow energy visualization...", speed);
        AnsiConsole.WriteLine();

        var colors = new[] { Color.Red, Color.Orange1, Color.Yellow, Color.Green, Color.Cyan1, Color.Blue, Color.Magenta1 };
        var recentDays = data.GetLatestYearData().TakeLast(7).ToList();
        
        var chart = new BarChart()
            .Width(70)
            .Label("[bold]🌈 Rainbow Energy Production Chart 🌈[/]")
            .CenterLabel();

        for (int i = 0; i < recentDays.Count; i++)
        {
            var day = recentDays[i];
            var color = colors[i % colors.Length];
            chart.AddItem($"D {day.D}", day.P, color);
        }

        AnsiConsole.Write(chart);
        await Task.Delay(speed * 5);
    }

    private async Task RainbowFinale(int speed)
    {
        AnsiConsole.WriteLine();
        
        var rainbow = "🌈✨🎉🌟💫⭐🎊✨🌈";
        
        for (int i = 0; i < 15; i++)
        {
            AnsiConsole.Clear();
            var rotated = rainbow.Substring(i % rainbow.Length) + rainbow.Substring(0, i % rainbow.Length);
            
            AnsiConsole.Write(Align.Center(new Markup("[bold]{rotated}[/]")));
            AnsiConsole.WriteLine();
            AnsiConsole.Write(Align.Center(new Markup("[bold magenta]SOLAR RAINBOW COMPLETE![/]")));
            AnsiConsole.WriteLine();
            AnsiConsole.Write(Align.Center(new Markup("[bold]{rotated}[/]")));
            
            await Task.Delay(speed * 2);
        }
    }

    private static int GetAnimationSpeed(string speed)
    {
        return speed.ToLower() switch
        {
            "slow" => 200,
            "fast" => 50,
            _ => 100 // normal
        };
    }
}
