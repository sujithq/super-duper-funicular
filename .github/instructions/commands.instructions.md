---
applyTo: "src/Commands/**/*.cs"
---

# Command Implementation Guidelines (Spectre.Console.Cli)

Commands in SolarScope CLI follow the Command Pattern and use Spectre.Console.Cli for argument handling and command structure.

## Command Structure Requirements

### Class Definition
- Inherit from `AsyncCommand<TSettings>` (or `Command<TSettings>` for sync commands), where `TSettings` is a nested class inheriting from `CommandSettings`.
- Use descriptive class names ending with "Command" (e.g., `DashboardCommand`, `AnalyzeCommand`).
- Implement proper error handling and validation in the command logic.

### Settings Class
- Define a nested `public class Settings : CommandSettings` inside each command.
- Use `[CommandOption]` attributes for all command-line options.
- Add `[Description]` and `[DefaultValue]` attributes as needed for clarity and help text.

### Method Patterns
- Implement `public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)` as the main entry point.
- Return 0 for success, non-zero for errors.
- Validate input parameters before processing.
- Use try-catch for error handling and display helpful error messages.

### Spectre.Console Integration
- Use `AnsiConsole` for all output operations.
- Implement consistent color schemes:
  - Green: `Color.Green` for production/success
  - Blue: `Color.Blue` for consumption/info
  - Yellow: `Color.Yellow` for warnings
  - Red: `Color.Red` for errors
- Add progress indicators for operations > 1 second
- Use tables, charts, and panels for data presentation

### Animation Guidelines
- Provide speed options: slow (2000ms), normal (1000ms), fast (500ms)
- Use `await Task.Delay()` for timing control
- Include cancellation support for animations
- Show progress with spinners or progress bars

### Error Handling
- Wrap operations in try-catch blocks
- Provide helpful error messages with suggested solutions
- Log errors with structured information
- Return appropriate exit codes

## Example Command Structure (Spectre.Console.Cli)

```csharp
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

public class ExampleCommand : AsyncCommand<ExampleCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--option <OPTION>")]
        [Description("Option description")]
        public string? Option { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            // Validate inputs
            // Show progress indicator
            // Process data using services
            // Display results with Spectre.Console
            return 0; // Success
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1; // Error
        }
    }
}
```

## Service Integration
- Inject or instantiate `SolarDataService` for data operations
- Use service methods for all business logic
- Handle service exceptions appropriately
- Cache service results when beneficial

## User Experience
- Always provide feedback for user actions
- Use emojis consistently: 🌞 ⚡ 📊 🏆 ❗ ❌
- Show helpful examples in command help text
- Support both quick overview and detailed modes
