---
mode: agent
description: Create comprehensive unit tests for SolarScope CLI components
---

# Generate Unit Tests

Create comprehensive unit tests for SolarScope CLI components using xUnit framework and following established testing patterns.

## Step 1: Identify Test Scope

If not already specified, determine:
- Which classes/methods need testing?
- What are the critical business logic paths?
- What edge cases should be covered?
- Are there any external dependencies to mock?

## Step 2: Create Test Class Structure

1. Create test file in `src/Tests/` directory
2. Follow naming convention: `[ClassName]Tests.cs`
3. Use descriptive test class and method names
4. Organize tests with nested classes if needed

## Step 3: Implement Test Cases

### For Data Models:
- Test calculated properties with various input values
- Verify JSON serialization/deserialization
- Test edge cases (zero values, null data)
- Validate business rules and constraints

### For Services:
- Test async methods with proper cancellation token handling
- Mock external dependencies (file system, etc.)
- Test error scenarios and exception handling
- Verify data processing and analytics calculations

### For Commands:
- Test command-line argument parsing
- Mock service dependencies
- Test error handling and exit codes
- Verify output formatting (separate from UI testing)

## Step 4: Create Sample Test Data

1. Build realistic test data sets
2. Include edge cases and boundary conditions
3. Create data builders for complex scenarios
4. Store sample JSON files in TestData folder if needed

## Step 5: Test Categories

### Unit Tests (Fast, Isolated)
```csharp
[Fact]
public void EnergyEfficiency_WithValidData_CalculatesCorrectly()
{
    // Arrange
    var dayData = new DayData { Production = 20.0, Consumption = 15.0 };
    
    // Act
    var efficiency = dayData.EnergyEfficiency;
    
    // Assert
    Assert.Equal(75.0, efficiency);
}
```

### Theory Tests (Multiple Scenarios)
```csharp
[Theory]
[InlineData(10.0, 8.0, 80.0)]
[InlineData(0.0, 5.0, 0.0)]
[InlineData(15.0, 15.0, 100.0)]
public void EnergyEfficiency_VariousInputs_ReturnsExpected(
    double production, double consumption, double expected)
```

### Integration Tests
- Test service interactions
- Use real data files for validation
- Test command execution end-to-end

## Step 6: Performance and Property Tests

1. Add benchmark tests for critical paths
2. Consider property-based testing for mathematical functions
3. Test memory usage and disposal patterns
4. Verify async operation behavior

Reference existing test files and follow the testing guidelines in the instructions.
