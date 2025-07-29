---
mode: agent
description: Add analytics features to SolarDataService
---

# Add Analytics Feature

Add a new analytics feature to the SolarDataService following established patterns and solar domain expertise.

## Step 1: Define Analytics Requirements

If not already provided, ask the user:
- What type of analysis should be performed?
- What data points should be analyzed?
- What insights should be generated?
- How should results be structured and presented?

## Step 2: Design Data Models

1. Create or extend data models in `src/Models/models.cs`
2. Use C# records for immutable result types
3. Include calculated properties for derived metrics
4. Add appropriate JSON serialization attributes if needed

## Step 3: Implement Analytics Method

1. Add the new method to `SolarDataService`
2. Use async/await patterns for data processing
3. Include proper error handling and validation
4. Implement efficient LINQ queries for data analysis
5. Add statistical calculations (mean, median, correlations)

## Step 4: Create Visualization Support

1. Design appropriate Spectre.Console charts or tables
2. Use consistent color schemes and formatting
3. Include meaningful headers and summaries
4. Add progress indicators for complex calculations

## Step 5: Integration and Testing

1. Add command support or extend existing commands
2. Test with realistic solar data scenarios
3. Validate calculations with edge cases
4. Ensure performance is acceptable for large datasets

## Analytics Categories to Consider

- **Production Analysis**: Peak performance, trends, seasonal patterns
- **Weather Correlation**: Impact of weather on energy production
- **Anomaly Detection**: Unusual patterns in production/consumption
- **Efficiency Metrics**: Energy balance, grid injection patterns
- **Comparative Analysis**: Day-to-day, month-to-month comparisons

Reference existing analytics methods in `SolarDataService` for patterns and calculation approaches.
