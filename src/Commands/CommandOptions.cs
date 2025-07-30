using Spectre.Console.Cli;
using System.ComponentModel;

namespace SolarScope.Commands;

public class BaseCommandSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the data file path or URL.
    /// </summary>
    [CommandOption("--data|-d")]
    [Description("Path or URL to the solar data JSON file")]
    public string DataFile { get; set; } = "https://raw.githubusercontent.com/sujithq/myenergy/refs/heads/main/src/myenergy/wwwroot/Data/data.json";


    /// <summary>
    /// Gets or sets a value indicating whether verbose output is enabled.
    /// </summary>
    [CommandOption("--verbose|-v")]
    [Description("Enable verbose output")]
    public bool Verbose { get; set; }
}

    