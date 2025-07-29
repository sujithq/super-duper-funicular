---
applyTo: "src/Commands/**/*.cs"
---

# Command Implementation Guidelines

Commands in SolarScope CLI follow the Command Pattern and use CommandLineParser for argument handling.

## Command Structure Requirements

### Class Definition
- Inherit from appropriate base class or implement command interface
- Use descriptive class names ending with "Command" (e.g., `DashboardCommand`, `AnalyzeCommand`)
- Add `[Verb]` attribute with command name and description
- Implement proper error handling and validation

### Method Patterns
- Use `ExecuteAsync()` as the main entry point for async operations
- Include `CancellationToken` parameters for long-running operations
- Return appropriate exit codes (0 for success, non-zero for errors)
- Validate input parameters before processing

### Spectre.Console Integration
- Use `AnsiConsole` for all output operations
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

## Example Command Structure

```csharp
[Verb("command-name", HelpText = "Description of what this command does")]
public class ExampleCommand
{
    [Option('o', "option", Required = false, HelpText = "Option description")]
    public string? Option { get; set; }

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
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
- Inject `SolarDataService` for data operations
- Use service methods for all business logic
- Handle service exceptions appropriately
- Cache service results when beneficial

## User Experience
- Always provide feedback for user actions
- Use emojis consistently: 🌞 ⚡ 📊 🏆 ⚠️ ❌
- Show helpful examples in command help text
- Support both quick overview and detailed modes
