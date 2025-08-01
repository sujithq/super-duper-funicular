using SolarScope.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SolarScope
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Task.Run(() =>
        {
            // Display welcome banner
            DisplayWelcomeBanner();

            var app = new CommandApp();
            app.Configure(config =>
            {
                config.AddCommand<AnalyzeCommand>("analyze");
                config.AddCommand<AnomaliesCommand>("anomalies");
                config.AddCommand<DashboardCommand>("dashboard");
                config.AddCommand<DemoCommand>("demo");
                config.AddCommand<ExploreCommand>("explore");
                config.AddCommand<ReportCommand>("report");
                config.AddCommand<WeatherCommand>("weather");

            });
            return app.Run(args);
        });
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

        //private static async Task<int> ExecuteCommand(BaseCommandSettings options, Func<Task> commandExecutor)
        //{
        //    try
        //    {
        //        // Console.WriteLine("ExecuteCommand called");
        //        // Console.Out.Flush();

        //        if (options.Verbose)
        //        {
        //            AnsiConsole.MarkupLine("[dim]Verbose mode enabled[/]");
        //            AnsiConsole.MarkupLine($"[dim]Data file: {options.DataFile}[/]");
        //            AnsiConsole.WriteLine();
        //        }

        //        // Console.WriteLine("About to execute command");
        //        // Console.Out.Flush();

        //        await commandExecutor();

        //        // Console.WriteLine("Command execution completed");
        //        // Console.Out.Flush();
        //        return 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Console.WriteLine($"Exception in ExecuteCommand: {ex.Message}");
        //        // Console.Out.Flush();
        //        AnsiConsole.WriteException(ex);
        //        return 1;
        //    }
        //}

        //private static int DisplayHelpAndExit<T>(ParserResult<T> parserResult, IEnumerable<Error> errs)
        //{
        //    var helpText = CommandLine.Text.HelpText.AutoBuild(parserResult, h =>
        //    {
        //        h.AdditionalNewLineAfterOption = false;
        //        h.Heading = "SolarScope CLI v1.0.0";
        //        h.Copyright = "For the Love of Code 2025 - Built with ❤  by Sujith Quintelier";
        //        h.AddDashesToOption = true;
        //        h.AddEnumValuesToHelpText = true;
        //        return h;
        //    }, e => e);

        //    // Display custom help with Spectre.Console formatting
        //    AnsiConsole.WriteLine();
        //    AnsiConsole.Write(new Rule("[bold]Available Commands[/]"));
        //    AnsiConsole.WriteLine();

        //    var table = new Table()
        //        .AddColumn("[bold]Command[/]")
        //        .AddColumn("[bold]Description[/]")
        //        .AddColumn("[bold]Example[/]");

        //    table.AddRow("[cyan]dashboard[/]", "Interactive solar system dashboard", "[dim]solarscope dashboard --animated[/]");
        //    table.AddRow("[cyan]analyze[/]", "Detailed data analysis", "[dim]solarscope analyze --type production[/]");
        //    table.AddRow("[cyan]report[/]", "Generate comprehensive reports", "[dim]solarscope report --type monthly[/]");
        //    table.AddRow("[cyan]anomalies[/]", "Detect system anomalies", "[dim]solarscope anomalies --interactive[/]");
        //    table.AddRow("[cyan]weather[/]", "Weather analysis and correlation", "[dim]solarscope weather --correlation[/]");
        //    table.AddRow("[cyan]explore[/]", "Interactive data exploration", "[dim]solarscope explore[/]");
        //    table.AddRow("[cyan]demo[/]", "Fun demo with animations", "[dim]solarscope demo --theme rainbow[/]");

        //    AnsiConsole.Write(table);
        //    AnsiConsole.WriteLine();

        //    // Show examples
        //    var examplesPanel = new Panel(new Markup(
        //        "[yellow]# Quick start examples[/]\n\n" +
        //        "[green]solarscope dashboard[/] - View the main dashboard\n" +
        //        "[green]solarscope analyze --type weather[/] - Analyze weather patterns\n" +
        //        "[green]solarscope anomalies --severity high[/] - Find serious anomalies\n" +
        //        "[green]solarscope demo --theme solar[/] - Show animated demo\n\n" +
        //        "[dim]Add --help to any command for detailed options[/]"))
        //    {
        //        Header = new PanelHeader("[bold]Quick Examples[/]"),
        //        Border = BoxBorder.Rounded
        //    };

        //    AnsiConsole.Write(examplesPanel);

        //    return 1;
        //}
    }

}

