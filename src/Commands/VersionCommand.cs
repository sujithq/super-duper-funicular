using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Reflection;

namespace SolarScope.Commands;

/// <summary>
/// Version command implementation (Spectre.Console.Cli)
/// </summary>
[Description("Display version information")]
public class VersionCommand : Command<VersionCommand.Settings>
{
    public class Settings : CommandSettings
    {
        // No additional settings needed for version command
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        // Clear the console to provide clean version output
        AnsiConsole.Clear();
        
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "Unknown";
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? version;
        
        // Display version information with SolarScope branding
        var rule = new Rule("[yellow]🌞 SolarScope CLI Version Information 🌞[/]")
        {
            Style = Style.Parse("yellow"),
            Justification = Justify.Center
        };

        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var panel = new Panel(new Markup(
            $"[bold cyan]SolarScope CLI[/]\n" +
            $"[green]Version: {informationalVersion}[/]\n" +
            $"[blue]Runtime: {Environment.Version}[/]\n" +
            $"[yellow]Platform: {Environment.OSVersion}[/]\n\n" +
            $"[dim]Built for GitHub's 'For the Love of Code 2025' hackathon[/]\n" +
            $"[dim]A beautiful terminal tool for solar energy monitoring[/]"))
        {
            Header = new PanelHeader("[bold]Version Details[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("green")
        };

        AnsiConsole.Write(panel);

        return 0;
    }
}
