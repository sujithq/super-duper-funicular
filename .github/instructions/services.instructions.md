---
applyTo: "src/Services/**/*.cs"
---

# Service Layer Implementation Guidelines

Services contain business logic and data processing for SolarScope CLI.

## Service Design Principles

### Class Structure
- Use descriptive service names (e.g., `SolarDataService`, `AnalyticsService`)
- Implement interfaces for testability
- Use dependency injection patterns
- Keep services focused on single responsibilities

### Data Processing Patterns
- Use async/await for I/O operations
- Implement proper error handling and validation
- Use LINQ for data transformations
- Cache expensive calculations when appropriate
- Support cancellation tokens for long operations

### JSON Data Handling
- Use `System.Text.Json` for serialization/deserialization
- Handle missing or malformed data gracefully
- Implement data validation after deserialization
- Use nullable types for optional data fields

## SolarDataService Specific Guidelines

### Data Loading
- Load JSON data asynchronously
- Validate data structure after loading
- Handle file not found scenarios
- Support different data sources (file, stream, etc.)

### Analytics Methods
- Return structured data types (records or classes)
- Include statistical calculations (mean, median, max, min)
- Provide correlation analysis between weather and production
- Implement anomaly detection with severity scoring

### Performance Considerations
- Use efficient data structures (Dictionary, List)
- Minimize memory allocations in loops
- Consider streaming for large datasets
- Implement result caching for expensive operations

## Example Service Method

```csharp
public async Task<ProductionAnalysis> AnalyzeProductionAsync(
    int topCount = 10, 
    CancellationToken cancellationToken = default)
{
    try
    {
        var data = await LoadDataAsync(cancellationToken);
        
        var topDays = data
            .Where(d => d.HasMeasurements)
            .OrderByDescending(d => d.Production)
            .Take(topCount)
            .ToList();

        return new ProductionAnalysis
        {
            TopDays = topDays,
            AverageProduction = data.Average(d => d.Production),
            TotalProduction = data.Sum(d => d.Production)
        };
    }
    catch (Exception ex)
    {
        throw new SolarDataException($"Failed to analyze production: {ex.Message}", ex);
    }
}
```

## Error Handling
- Create custom exception types for domain-specific errors
- Include context information in exception messages
- Log errors with structured data
- Provide actionable error messages

## Data Validation
- Validate input parameters at method entry
- Check for null or empty collections
- Verify data ranges and constraints
- Handle edge cases (zero production, missing weather data)

## Mathematical Calculations
- Use appropriate data types (decimal for currency, double for scientific)
- Handle division by zero scenarios
- Round results to appropriate precision
- Include units in calculation results where applicable
