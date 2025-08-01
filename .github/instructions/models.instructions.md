---
applyTo: "src/Models/**/*.cs"
---

# Data Model Implementation Guidelines

Data models in SolarScope CLI use C# records for immutable data structures with calculated properties.

## Record Design Principles

### Basic Structure
- Use `record` keyword for immutable data types
- Add `JsonPropertyName` attributes for JSON serialization
- Include XML documentation for public properties
- Use nullable types for optional data (`string?`, `int?`)

### Property Naming
- Use descriptive property names that match domain terminology
- Follow PascalCase convention for public properties
- Map JSON property names using `JsonPropertyName` attribute
- Include units in property names where helpful (e.g., `TemperatureCelsius`)

### Calculated Properties
- Add computed properties for derived values
- Use `get` only properties for calculations
- Include appropriate null checks for calculations
- Document calculation formulas in XML comments

## Solar Data Model Examples

### Daily Data Record
```csharp
/// <summary>
/// Represents daily solar system data with production, consumption, and weather information.
/// </summary>
public record DayData
{
    [JsonPropertyName("D")]
    public int DayOfYear { get; init; }

    [JsonPropertyName("P")]
    public double Production { get; init; }

    [JsonPropertyName("U")]
    public double Consumption { get; init; }

    [JsonPropertyName("I")]
    public double GridInjection { get; init; }

    /// <summary>
    /// Energy efficiency as percentage (Consumption / Production * 100)
    /// </summary>
    public double EnergyEfficiency => Production > 0 ? (Consumption / Production) * 100 : 0;

    /// <summary>
    /// Energy balance (Production - Consumption). Positive = surplus, Negative = deficit
    /// </summary>
    public double EnergyBalance => Production - Consumption;
}
```

### Weather Data Record
- Include temperature, precipitation, wind, and pressure data
- Add weather condition classification properties
- Implement wind speed categorization
- Calculate derived weather metrics

### Anomaly Data Record
- Define severity levels (None, Low, Medium, High)
- Include anomaly scores for different metrics
- Add human-readable anomaly descriptions
- Implement severity threshold logic

## JSON Serialization Guidelines

### Attributes
- Use `JsonPropertyName` for all properties that map to JSON
- Add `JsonIgnore` for calculated properties not in JSON
- Use `JsonConverter` for custom data transformations
- Handle null values appropriately with nullable types

### Data Validation
- Validate ranges in property setters when using classes
- Add validation methods for complex business rules
- Check for required fields after deserialization
- Handle malformed or missing data gracefully

## Performance Considerations

### Memory Efficiency
- Use records for immutable data (reduces memory allocations)
- Consider using `ReadOnlySpan<T>` for large array processing
- Minimize object creation in calculated properties
- Use appropriate data types (avoid oversized types)

### Calculation Optimization
- Cache expensive calculated properties when appropriate
- Use lazy initialization for complex calculations
- Consider pre-computing values during deserialization
- Optimize LINQ queries in calculated properties

## Domain-Specific Guidelines

### Energy Calculations
- Use `double` for energy values (kWh precision)
- Handle zero/negative production scenarios
- Include proper units in property documentation
- Round results to appropriate decimal places

### Time Handling
- Use `DateTime` or `DateTimeOffset` for timestamps
- Handle time zone conversions appropriately
- Calculate daylight hours from sunrise/sunset times
- Support different date formats in JSON

### Weather Integration
- Map weather conditions to standardized categories
- Calculate comfort indices and classifications
- Handle missing weather data gracefully
- Include weather impact on production calculations
