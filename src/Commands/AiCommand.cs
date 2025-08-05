using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
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

        // Debug logging for AI response
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine("[dim]🐛 DEBUG: AI Response:[/]");
            AnsiConsole.WriteLine(aiResponse);
            AnsiConsole.MarkupLine("[dim]🐛 DEBUG: Response length: {0} characters[/]", aiResponse.Length);
            AnsiConsole.WriteLine();
        }

        // Check if response looks like a valid solarscope command
        var isCommand = IsValidSolarScopeCommand(aiResponse.Trim());

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

            var ops = CollectOps(aiResponse);
            
            var items = new List<IRenderable>();
            foreach (var op in ops)
            {
                switch (op)
                {
                    case MarkupLineOp m:
                        items.Add(new Markup(m.Text));
                        break;
                    case WritePanelOp p:
                        items.Add(new Panel(p.Text).RoundedBorder());
                        break;
                    case WriteRuleOp r:
                        items.Add(new Rule(r.Text));
                        break;
                }
            }
            var panel = new Panel(new Rows(items))
            {
                Header = new PanelHeader("[bold]Help & Information[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("blue")
            };
            AnsiConsole.Write(panel);
        }

        return 0;
    }

    /// <summary>
    /// Validates if the given response is a valid SolarScope CLI command
    /// </summary>
    /// <param name="response">The response to validate</param>
    /// <returns>True if it's a valid SolarScope command, false otherwise</returns>
    private static bool IsValidSolarScopeCommand(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return false;

        // Split the response into parts
        var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Must have at least 2 parts: "solarscope" and a command
        if (parts.Length < 2)
            return false;

        // First part must be "solarscope"
        if (!parts[0].Equals("solarscope", StringComparison.OrdinalIgnoreCase))
            return false;

        // Valid SolarScope commands
        var validCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ai",
            "analyze", 
            "dashboard",
            "anomalies",
            "report",
            "weather",
            "demo",
            "explore"
        };

        // Second part must be a valid command
        return validCommands.Contains(parts[1]);
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

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("SolarScope.README.md");
        using var reader = new StreamReader(stream!);
        string readmeContent = reader.ReadToEnd();


        var systemPrompt = @"STRICT RULE: Only use the commands and options listed below. Never invent new commands, options, or values. If a user asks for something unsupported, explain the limitation and suggest the closest valid command.

You are an AI assistant for SolarScope CLI, a .NET tool for analyzing solar production data with weather correlations and anomaly detection.

Supported commands and their exact options/values:

• analyze [--type|-t production|weather|anomalies|correlation] [--count|-c N] [--data|-d PATH] [--verbose|-v]
• dashboard [--animated|-a] [--full|-f] [--data|-d PATH] [--verbose|-v]
• anomalies [--year|-y YYYY] [--severity|-s Low|Medium|High] [--interactive|-i] [--data|-d PATH] [--verbose|-v]
• report [--period daily|weekly|monthly|yearly] [--year YYYY] [--start-day N] [--end-day N] [--data|-d PATH] [--verbose|-v]
• weather [--analysis overview|correlation|patterns|recommendations] [--year YYYY] [--data|-d PATH] [--verbose|-v]
• demo [--theme|-t solar|matrix|rainbow] [--speed|-s slow|normal|fast] [--data|-d PATH] [--verbose|-v]
• explore [--mode quick|guided|full] [--year YYYY] [--data|-d PATH] [--verbose|-v]
• ai <prompt> [--model|-m github/gpt-4o|github/o4-mini] [--execute|-x] [--data|-d PATH] [--verbose|-v]

Common options (all commands): --data/-d, --verbose/-v

DO NOT:
- Suggest commands, flags, or values not listed above
- Suggest positional arguments that do not exist
- Suggest subcommands or features not present in the list

How to Respond:
- For command requests: Output only the exact CLI command, nothing else
- For help: Provide a concise explanation and a valid example
- For unsupported requests: Say 'That is not supported. Here are the closest valid commands/options: ...'

Examples:
Input: Show me the full animated dashboard
Output: solarscope dashboard --full --animated

Input: Find high severity anomalies for 2024 in interactive mode
Output: solarscope anomalies --year 2024 --severity High --interactive

Input: List all available commands
Output: SolarScope CLI has these main commands: ai, analyze, dashboard, anomalies, report, weather, demo, explore. Each command supports the --help flag for detailed usage information.

STRICT RULE: Only use the commands and options listed above. Never invent new commands, options, or values.

You can also answer questions about the project documentation";

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
                    new { role = "system", content = "Project documentation:\n" + readmeContent },
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

#if DEBUG
            // Log the raw response to file for debugging
            var debugLogPath = Path.Combine(Path.GetTempPath(), "solarscope-ai-debug.log");
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] User Prompt: {userPrompt}\n" +
                              $"Raw API Response: {content}\n" +
                              new string('=', 80) + "\n";
                await File.AppendAllTextAsync(debugLogPath, logEntry);
            }
            catch { /* Ignore logging errors */ }
#endif
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


    public abstract record RenderOp;

    public record MarkupLineOp(string Text) : RenderOp;
    public record WritePanelOp(string Text) : RenderOp;
    public record WriteRuleOp(string Text) : RenderOp;

    private static List<RenderOp> CollectOps(string markdown)
    {
        var ops = new List<RenderOp>();
        var pipeline = new MarkdownPipelineBuilder().Build();
        var document = Markdown.Parse(markdown, pipeline);

        foreach (var block in document)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    var text = ProcessInline(heading.Inline ?? new ContainerInline());
                    if (heading.Level == 1)
                        ops.Add(new WriteRuleOp($"[bold yellow]{text}[/]"));
                    else
                        ops.Add(new MarkupLineOp($"[bold underline]{text}[/]"));
                    break;

                case ListBlock list:
                    CollectListOps(list, 0, ops);
                    break;

                case ParagraphBlock para:
                    var pText = ProcessInline(para.Inline ?? new ContainerInline());
                    ops.Add(new MarkupLineOp(pText));
                    break;

                case FencedCodeBlock code:
                    var codeContent = string.Join('\n', code.Lines);
                    ops.Add(new WritePanelOp($"[grey]{codeContent}[/]"));
                    break;
            }
            ops.Add(new MarkupLineOp("")); // blank line for spacing
        }
        return ops;
    }

    static void CollectListOps(ListBlock list, int indent, List<RenderOp> ops)
    {
        int number = 1;
        if (list.IsOrdered && int.TryParse(list.OrderedStart, out int start))
            number = start;

        foreach (ListItemBlock item in list)
        {
            foreach (var subBlock in item)
            {
                if (subBlock is ParagraphBlock para)
                {
                    var itemText = ProcessInline(para.Inline ?? new ContainerInline());
                    var prefix = new string(' ', indent * 2);
                    if (list.IsOrdered)
                        ops.Add(new MarkupLineOp($"{prefix}{number++}. {itemText}"));
                    else
                        ops.Add(new MarkupLineOp($"{prefix}- {itemText}"));
                }
                else if (subBlock is ListBlock nestedList)
                {
                    CollectListOps(nestedList, indent + 1, ops);
                }
            }
        }
    }

    // Convert Markdig inline objects into Spectre.Console markup
    static string ProcessInline(ContainerInline inline)
    {
        var sb = new StringBuilder();
        foreach (var child in inline)
        {
            switch (child)
            {
                case LiteralInline lit:
                    sb.Append(lit.Content.Text.Substring(lit.Content.Start, lit.Content.Length));
                    break;
                case EmphasisInline emph:
                    var inner = ProcessInline(emph);
                    // 1 '*' = italic, 2 '*' = bold
                    if (emph.DelimiterCount == 2)
                        sb.Append($"[bold]{inner}[/]");
                    else
                        sb.Append($"[italic]{inner}[/]");
                    break;
                case CodeInline code:
                    sb.Append($"[grey]{code.Content}[/]");
                    break;
                    // Add more as needed (links, etc)
            }
        }
        return sb.ToString();
    }

}
