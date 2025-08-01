---
applyTo: "src/Tests/**/*.cs"
---

# Testing Implementation Guidelines

Testing in SolarScope CLI uses xUnit framework with focus on data processing and command validation.

## Test Organization

### Test Class Structure
- Use descriptive test class names ending with "Tests" (e.g., `SolarDataServiceTests`)
- Group related tests in the same class
- Use nested classes for organizing test categories
- Follow AAA pattern: Arrange, Act, Assert

### Test Method Naming
- Use descriptive method names: `Method_Scenario_ExpectedBehavior`
- Include context about the test scenario
- Be specific about expected outcomes
- Use meaningful parameter names in theory tests

## Data Testing Patterns

### Sample Data Creation
- Create reusable test data builders
- Use realistic solar data values
- Include edge cases (zero production, missing data)
- Provide both valid and invalid data scenarios

### JSON Deserialization Tests
- Test proper mapping of JSON properties
- Verify calculated properties work correctly
- Handle malformed JSON gracefully
- Test null value handling

## Service Testing Guidelines

### SolarDataService Tests
- Mock file system operations for data loading
- Test analytics calculations with known data
- Verify error handling for invalid data
- Test async operations with cancellation tokens

### Mock Data Patterns
```csharp
private static List<DayData> CreateTestData()
{
    return new List<DayData>
    {
        new() { DayOfYear = 1, Production = 10.5, Consumption = 8.2 },
        new() { DayOfYear = 2, Production = 15.3, Consumption = 12.1 },
        // More test data...
    };
}
```

## Command Testing

### Command Execution Tests
- Test command-line argument parsing
- Verify proper service integration
- Test error scenarios and exit codes
- Mock external dependencies (file system, services)

### UI Testing Considerations
- Test data formatting logic separately from UI output
- Mock Spectre.Console operations where possible
- Focus on testing business logic rather than visual output
- Verify proper error message formatting

## Performance Testing

### Benchmark Tests
- Use BenchmarkDotNet for performance measurement
- Test with realistic data sizes (365 days of data)
- Measure memory allocations and execution time
- Compare different algorithm implementations

### Load Testing
- Test with large datasets (multiple years)
- Verify memory usage stays within bounds
- Test concurrent operations where applicable
- Monitor resource cleanup and disposal

## Test Data Management

### Sample Data Files
- Store test JSON files in `TestData` folder
- Use realistic but anonymized data
- Include various data patterns and edge cases
- Version control test data files

### Data Builders
- Create fluent test data builders
- Support method chaining for complex scenarios
- Include random data generation for property testing
- Provide both minimal and complete data sets

## Example Test Structure

```csharp
public class SolarDataServiceTests
{
    [Fact]
    public async Task LoadDataAsync_ValidFile_ReturnsExpectedData()
    {
        // Arrange
        var testData = CreateTestData();
        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.ReadAllTextAsync(It.IsAny<string>()))
                      .ReturnsAsync(JsonSerializer.Serialize(testData));
        
        var service = new SolarDataService(mockFileService.Object);

        // Act
        var result = await service.LoadDataAsync("test.json");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testData.Count, result.Count);
        Assert.Equal(testData.First().Production, result.First().Production);
    }

    [Theory]
    [InlineData(0, 10, 0)] // No production = 0% efficiency
    [InlineData(10, 5, 50)] // Normal efficiency
    [InlineData(20, 20, 100)] // Perfect efficiency
    public void EnergyEfficiency_CalculatedCorrectly(double production, double consumption, double expected)
    {
        // Arrange
        var dayData = new DayData { Production = production, Consumption = consumption };

        // Act
        var efficiency = dayData.EnergyEfficiency;

        // Assert
        Assert.Equal(expected, efficiency, precision: 2);
    }
}
```

## Test Categories

### Unit Tests
- Test individual methods and calculations
- Mock external dependencies
- Focus on single responsibility
- Fast execution (< 100ms per test)

### Integration Tests
- Test service interactions
- Use real data files where appropriate
- Test command-line parsing end-to-end
- Verify data flow through the application

### Property Tests
- Use random data generation for edge case discovery
- Test mathematical properties (commutativity, associativity)
- Verify invariants across different inputs
- Use FsCheck or similar property testing library
