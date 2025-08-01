---
mode: agent
description: Create a new command for SolarScope CLI
---

# Create New CLI Command (Spectre.Console.Cli)

Create a new command for the SolarScope CLI using Spectre.Console.Cli, following established patterns, conventions, and project UI/UX standards.

## Step 1: Gather Command Requirements

If not already provided, ask the user:
- What should the command be called?
- What functionality should it provide?
- What arguments/options should it accept?
- How should it display results to the user?

## Step 2: Create Command Class (Spectre.Console.Cli)

1. Create a new file in the `src/Commands/` directory
2. Follow the naming convention: `[CommandName]Command.cs`
3. Inherit from `AsyncCommand<TSettings>` (or `Command<TSettings>` for sync)
4. Define a nested `Settings : CommandSettings` class for CLI options
5. Use `[CommandOption]` attributes for all arguments
6. Add `[Description]` and `[DefaultValue]` as needed
7. Implement error handling and validation in `ExecuteAsync`

## Step 3: Implement Command Logic

1. Implement the `ExecuteAsync(CommandContext, Settings)` method as the entry point
2. Integrate with `SolarDataService` or other services as needed
3. Use Spectre.Console for terminal output (charts, tables, color, emoji)
4. Include progress indicators for long operations
5. Follow project color schemes and joyful UI/UX

## Step 4: Register and Document the Command

1. Register the new command in the main program using Spectre.Console.Cli's command app builder
2. Ensure the command appears in CLI help output
3. Update documentation and help text to describe the new command

## Step 5: Testing and Validation

1. Add or update unit tests for the command in `src/Tests/`
2. Test the command with various input scenarios and edge cases
3. Verify error handling and output formatting
4. Ensure cross-platform compatibility

Reference the existing commands in `src/Commands/` for patterns and examples. Follow the repository's coding standards and UI/UX guidelines. Ensure all new commands are discoverable via CLI help and follow the SolarScope CLI's joyful, educational, and accessible design principles.
