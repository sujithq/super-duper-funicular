using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SolarScope.Commands;

/// <summary>
/// AI assistant command that can translate natural language to CLI commands and answer help questions
/// </summary>
public class AiCommand : AsyncCommand<AiCommand.Settings>
{
    /// <summary>
    /// Settings for the ai command.
    /// </summary>
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "<PROMPT>")]
        [Description("Natural language prompt - ask questions or describe what you want to do")]
        public string Prompt { get; set; } = string.Empty;

        [CommandOption("--model|-m")]
        [Description("AI model to use (github/gpt-4o, github/o4-mini)")]
        [DefaultValue("github/gpt-4o")]
        public string Model { get; set; } = "github/gpt-4o";

        [CommandOption("--execute|-x")]
        [Description("Execute the suggested command directly (use with caution)")]
        [DefaultValue(false)]
        public bool Execute { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Prompt))
        {
            AnsiConsole.MarkupLine("[red]❌ You must provide a prompt.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Examples:[/]");
            AnsiConsole.MarkupLine("  [cyan]solarscope ai \"Show me dashboard for last 7 days\"[/]");
            AnsiConsole.MarkupLine("  [cyan]solarscope ai \"What commands are available?\"[/]");
            AnsiConsole.MarkupLine("  [cyan]solarscope ai \"How do I use the analyze command?\"[/]");
            return 1;
        }

        AnsiConsole.Status()
            .Start("🤖 Thinking...", ctx => 
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
            });

        var aiResponse = await GetAiResponseAsync(settings.Prompt, settings.Model);

        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            AnsiConsole.MarkupLine("[red]❌ No response received or an error occurred.[/]");
            return 2;
        }

        // Check if response looks like a command
        var isCommand = aiResponse.Trim().StartsWith("solarscope", StringComparison.OrdinalIgnoreCase);

        if (isCommand)
        {
            AnsiConsole.MarkupLine("[green]🤖 AI suggested command:[/]");
            var panel = new Panel(new Markup($"[bold cyan]{aiResponse}[/]"))
            {
                Header = new PanelHeader("[bold]CLI Command[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green")
            };
            AnsiConsole.Write(panel);

            if (settings.Execute)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[yellow]⚡ Executing command...[/]");
                // TODO: Execute the command (would need command parsing logic)
                AnsiConsole.MarkupLine("[dim]Command execution not yet implemented - copy and run manually[/]");
            }
            else
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]💡 Add --execute flag to run this command directly[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[green]🤖 AI response:[/]");
            var panel = new Panel(new Markup(aiResponse))
            {
                Header = new PanelHeader("[bold]Help & Information[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("blue")
            };
            AnsiConsole.Write(panel);
        }

        return 0;
    }

    private async Task<string?> GetAiResponseAsync(string userPrompt, string model)
    {
        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            AnsiConsole.MarkupLine("[red]❌ Error: GITHUB_TOKEN environment variable not set![/]");
            AnsiConsole.MarkupLine("[yellow]💡 Create a GitHub Personal Access Token with 'models:read' permission[/]");
            AnsiConsole.MarkupLine("[dim]   Set it with: export GITHUB_TOKEN=your_token_here[/]");
            return null;
        }

        var systemPrompt = @"You are an AI assistant for SolarScope CLI, a .NET tool for analyzing solar production data with weather correlations and anomaly detection.

You can:
1. Translate natural language requests into valid SolarScope CLI commands
2. Answer help and usage questions about SolarScope commands, options, and functionality
3. Provide guidance on solar data analysis workflows

Available commands and their exact options:

• analyze [--type|-t production|weather|anomalies|correlation] [--count|-c N] [--data|-d PATH] [--verbose|-v]
  - Analyzes solar production data with various analysis types (default: production, count: 10)
  
• dashboard [--animated|-a] [--full|-f] [--data|-d PATH] [--verbose|-v]
  - Shows interactive visual dashboard of solar system performance
  
• anomalies [--year|-y YYYY] [--severity|-s Low|Medium|High] [--interactive|-i] [--data|-d PATH] [--verbose|-v]
  - Detects and analyzes anomalies in solar production data (default: severity Low, latest year)
  
• report [--period daily|weekly|monthly|yearly] [--year YYYY] [--start-day N] [--end-day N] [--data|-d PATH] [--verbose|-v]
  - Generates comprehensive reports on solar system performance (default: monthly)
  
• weather [--analysis overview|correlation|patterns|recommendations] [--year YYYY] [--data|-d PATH] [--verbose|-v]
  - Analyzes weather data and correlations with solar production (default: overview)
  
• demo [--theme|-t solar|matrix|rainbow] [--speed|-s slow|normal|fast] [--data|-d PATH] [--verbose|-v]
  - Shows animated demonstrations of SolarScope capabilities (default: solar theme, normal speed)
  
• explore [--mode quick|guided|full] [--year YYYY] [--data|-d PATH] [--verbose|-v]
  - Interactive exploration of solar data (default: quick mode)

• ai <prompt> [--model|-m github/gpt-4o|github/o4-mini] [--execute|-x] [--data|-d PATH] [--verbose|-v]
  - AI assistant for natural language commands and help (default: github/gpt-4o)

Common options (available on all commands):
- --data/-d: Path to solar data JSON file (defaults to ~/SolarScopeData.json)
- --verbose/-v: Enable verbose output

Examples:
Input: Show me the full animated dashboard
Output: solarscope dashboard --full --animated

Input: Find high severity anomalies for 2024 in interactive mode
Output: solarscope anomalies --year 2024 --severity High --interactive

Input: Analyze weather correlations for the last 15 records
Output: solarscope analyze --type correlation --count 15

Input: Generate a yearly report for 2023
Output: solarscope report --period yearly --year 2023

Input: Run a matrix demo at fast speed
Output: solarscope demo --theme matrix --speed fast

Input: What does the weather command do?
Output: The 'weather' command analyzes weather data and its correlation with solar production. You can choose from four analysis types: 'overview' (general weather summary), 'correlation' (weather impact on production), 'patterns' (weather trend analysis), or 'recommendations' (optimization suggestions). Use --year to focus on a specific year.

Input: How do I explore data in guided mode?
Output: Use the 'explore' command with --mode guided. For example: solarscope explore --mode guided --year 2024

Input: List all available commands
Output: SolarScope CLI has these main commands: ai (AI assistant), analyze (data analysis), dashboard (visual overview), anomalies (anomaly detection), report (generate reports), weather (weather analysis), demo (feature demonstrations), and explore (interactive data exploration). Each command supports the --help flag for detailed usage information.

Instructions:
- If the user asks for a command or action, provide the exact CLI command with proper flags
- If the user asks questions about functionality, provide helpful explanations
- Always be concise but informative
- For command outputs, only provide the command without additional text
- For help questions, provide clear explanations with examples
- Use the exact flag names and values from the command definitions above";

        try
        {
            var baseUrl = "https://models.github.ai/inference/chat/completions";
            
            // Map the model name to the correct format for GitHub Models
            var githubModel = model switch
            {
                "github/gpt-4o" => "openai/gpt-4o",
                "github/o4-mini" => "openai/gpt-4o-mini",
                _ => "openai/gpt-4o" // Default fallback
            };

            // Build the request body
            var body = new
            {
                model = githubModel,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 200,
                temperature = 0.3
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(body);

            // Send the POST request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
            client.DefaultRequestHeaders.Add("User-Agent", "SolarScopeCLI/1.0");

            var response = await client.PostAsync(
                baseUrl,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[red]❌ AI request failed: {response.StatusCode}[/]");
                AnsiConsole.MarkupLine($"[dim]{errorContent}[/]");
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();

            // Deserialize and extract the message
            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content?.Trim();
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Network error: {ex.Message}[/]");
            return null;
        }
        catch (JsonException ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Failed to parse AI response: {ex.Message}[/]");
            return null;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ AI request failed: {ex.Message}[/]");
            return null;
        }
    }
}
