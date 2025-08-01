---
mode: agent
description: Add analytics features to SolarDataService
---

# Add Analytics Feature to CLI

Add a new analytics feature to the SolarScope CLI, following Spectre.Console.Cli patterns, solar domain expertise, and project UI/UX standards.

## Step 1: Define Analytics Requirements

If not already provided, ask the user:
- What type of analysis should be performed?
- What data points should be analyzed?
- What insights should be generated?
- How should results be structured and presented?

## Step 2: Design Data Models

1. Create or extend data models in `src/Models/models.cs` using C# records
2. Include calculated properties for derived metrics
3. Add appropriate JSON serialization attributes if needed
4. Implement data validation as required

## Step 3: Implement Analytics Logic in Service Layer

1. Add or extend analytics methods in `SolarDataService`
2. Use async/await for data processing
3. Include proper error handling and validation
4. Implement efficient LINQ/statistical calculations (mean, median, correlations)

## Step 4: Integrate with CLI Command (Spectre.Console.Cli)

1. Create or extend a command in `src/Commands/` using Spectre.Console.Cli
2. Inherit from `AsyncCommand<TSettings>` and define a nested `Settings : BaseCommandSettings` class
3. Use `[CommandOption]` attributes for CLI arguments
4. Call the analytics method from the command's `ExecuteAsync` method
5. Use Spectre.Console for output: charts, tables, color schemes, emojis, and progress indicators
6. Follow project color conventions (green, blue, yellow, red) and joyful UI/UX

## Step 5: Testing and Validation

1. Add or update unit tests in `src/Tests/` for analytics logic and CLI command
2. Test with realistic and edge-case solar data
3. Validate CLI output formatting and error handling
4. Ensure performance is acceptable for large datasets

## Analytics Categories to Consider

- **Production Analysis**: Peak performance, trends, seasonal patterns
- **Weather Correlation**: Impact of weather on energy production
- **Anomaly Detection**: Unusual patterns in production/consumption
- **Efficiency Metrics**: Energy balance, grid injection patterns
- **Comparative Analysis**: Day-to-day, month-to-month comparisons

Reference existing analytics commands and methods for patterns and calculation approaches. Ensure all new features are discoverable via CLI help and follow the SolarScope CLI's joyful, educational, and accessible design principles.
