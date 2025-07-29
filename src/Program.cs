using CommandLine;
using Spectre.Console;
using SolarScope.Commands;
using SolarScope.Services;
using System.Reflection;

namespace SolarScope;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        // Display welcome banner
        DisplayWelcomeBanner();

        // Parse command line arguments
        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<
            DashboardOptions,
            AnalyzeOptions,
            ReportOptions,
            AnomaliesOptions,
            WeatherOptions,
            ExploreOptions,
            DemoOptions>(args);

        return await parserResult.MapResult(
            (DashboardOptions opts) => ExecuteCommand(opts, async () => await new DashboardCommand().ExecuteAsync(opts)),
            (AnalyzeOptions opts) => ExecuteCommand(opts, async () => await new AnalyzeCommand().ExecuteAsync(opts)),
            (ReportOptions opts) => ExecuteCommand(opts, async () => await new ReportCommand().ExecuteAsync(opts)),
            (AnomaliesOptions opts) => ExecuteCommand(opts, async () => await new AnomaliesCommand().ExecuteAsync(opts)),
            (WeatherOptions opts) => ExecuteCommand(opts, async () => await new WeatherCommand().ExecuteAsync(opts)),
            (ExploreOptions opts) => ExecuteCommand(opts, async () => await new ExploreCommand().ExecuteAsync(opts)),
            (DemoOptions opts) => ExecuteCommand(opts, async () => await new DemoCommand().ExecuteAsync(opts)),
            errs => Task.FromResult(DisplayHelpAndExit(parserResult, errs))
        );
    }

    private static void DisplayWelcomeBanner()
    {
        var rule = new Rule("[yellow]🌞 SolarScope CLI - Your Personal Solar System Command Center 🌞[/]")
        {
            Style = Style.Parse("yellow"),
            Justification = Justify.Center
        };
        
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        
        var panel = new Panel(new Markup(
            "[bold cyan]Welcome to SolarScope![/]\n\n" +
            "[green]Your comprehensive solar system monitoring and analysis tool.[/]\n" +
            "[dim]Built for the GitHub 'For the Love of Code 2025' hackathon[/]\n\n" +
            "[yellow]💡 Use --help with any command for detailed information[/]"))
        {
            Header = new PanelHeader("[bold]Getting Started[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static async Task<int> ExecuteCommand(BaseOptions options, Func<Task> commandExecutor)
    {
        try
        {
            if (options.Verbose)
            {
                AnsiConsole.MarkupLine("[dim]Verbose mode enabled[/]");
                AnsiConsole.MarkupLine($"[dim]Data file: {options.DataFile}[/]");
                AnsiConsole.WriteLine();
            }

            // Validate data file exists
            if (!File.Exists(options.DataFile))
            {
                AnsiConsole.MarkupLine($"[red]Error: Data file not found: {options.DataFile}[/]");
                AnsiConsole.MarkupLine("[yellow]Tip: Use -d or --data to specify the correct path[/]");
                return 1;
            }

            await commandExecutor();
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static int DisplayHelpAndExit<T>(ParserResult<T> parserResult, IEnumerable<Error> errs)
    {
        var helpText = CommandLine.Text.HelpText.AutoBuild(parserResult, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "SolarScope CLI v1.0.0";
            h.Copyright = "For the Love of Code 2025 - Built with ❤️ by Sujith Quintelier";
            h.AddDashesToOption = true;
            h.AddEnumValuesToHelpText = true;
            return h;
        }, e => e);

        // Display custom help with Spectre.Console formatting
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Available Commands[/]"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .AddColumn("[bold]Command[/]")
            .AddColumn("[bold]Description[/]")
            .AddColumn("[bold]Example[/]");

        table.AddRow("[cyan]dashboard[/]", "Interactive solar system dashboard", "[dim]solarscope dashboard --animated[/]");
        table.AddRow("[cyan]analyze[/]", "Detailed data analysis", "[dim]solarscope analyze --type production[/]");
        table.AddRow("[cyan]report[/]", "Generate comprehensive reports", "[dim]solarscope report --type monthly[/]");
        table.AddRow("[cyan]anomalies[/]", "Detect system anomalies", "[dim]solarscope anomalies --interactive[/]");
        table.AddRow("[cyan]weather[/]", "Weather analysis and correlation", "[dim]solarscope weather --correlation[/]");
        table.AddRow("[cyan]explore[/]", "Interactive data exploration", "[dim]solarscope explore[/]");
        table.AddRow("[cyan]demo[/]", "Fun demo with animations", "[dim]solarscope demo --theme rainbow[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        // Show examples
        var examplesPanel = new Panel(new Markup(
            "[yellow]# Quick start examples[/]\n\n" +
            "[green]solarscope dashboard[/] - View the main dashboard\n" +
            "[green]solarscope analyze --type weather[/] - Analyze weather patterns\n" +
            "[green]solarscope anomalies --severity high[/] - Find serious anomalies\n" +
            "[green]solarscope demo --theme solar[/] - Show animated demo\n\n" +
            "[dim]Add --help to any command for detailed options[/]"))
        {
            Header = new PanelHeader("[bold]Quick Examples[/]"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(examplesPanel);
        
        return 1;
    }
}
