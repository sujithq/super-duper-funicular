# SolarScope CLI - Repository Custom Instructions

## Project Overview

SolarScope CLI is a beautiful, interactive command-line tool for monitoring and analyzing solar energy systems with weather correlations and anomaly detection. Built for GitHub's "For the Love of Code 2025" hackathon in the "Terminal talent" category.

**Mission:** Transform boring solar data into an engaging, insightful, and joyful terminal experience that makes renewable energy monitoring both useful and fun.

## Project Structure

- **`src/`** - All source code organized by feature
  - **`Commands/`** - Command implementations using Command Pattern
  - **`Services/`** - Business logic and data processing
  - **`Models/`** - Data models using C# records
  - **`Tests/`** - Unit tests (structure ready but not implemented)
- **`data/`** - Sample solar system data in JSON format
- **`docs/`** - Documentation and guides
- **Build Scripts** - Cross-platform build automation (`build.sh`, `build.bat`)
- **Demo Scripts** - Feature showcase scripts (`demo.sh`, `demo.bat`)

## Technology Stack

- **.NET 8.0** - Modern, cross-platform framework
- **C# 12** - Latest language features with records and pattern matching
- **Spectre.Console 0.49.1** - Beautiful terminal UI with charts and animations
- **CommandLineParser 2.9.1** - Robust command-line argument parsing
- **System.Text.Json 8.0.0** - High-performance JSON processing

## Architecture Principles

- **Command Pattern** - Separate command classes for each feature
- **Service Layer Pattern** - Business logic separated from presentation
- **Record Types** - Immutable data models with computed properties
- **Async/Await** - Non-blocking operations with progress indication
- **Clean Architecture** - Clear separation of concerns and dependencies

## Coding Standards

### General C# Guidelines
- Follow Microsoft C# coding conventions
- Use PascalCase for classes, methods, properties
- Use camelCase for local variables and parameters
- Use meaningful, descriptive names over abbreviations
- Add XML documentation for public APIs
- Implement proper error handling with try-catch blocks
- Use `async/await` for I/O operations

### Data Models
- Use C# records for immutable data structures
- Add `JsonPropertyName` attributes for JSON serialization
- Include calculated properties for derived data
- Implement data validation where appropriate
- Use nullable reference types when data might be missing

### Terminal UI Design
- Use Spectre.Console for all UI elements
- Maintain consistent color schemes:
  - **Green** for production/positive metrics
  - **Blue** for consumption/neutral metrics
  - **Yellow** for warnings/anomalies
  - **Red** for errors/critical issues
- Include appropriate emojis for visual appeal (🌞⚡📊🏆⚠️)
- Provide progress indicators for long-running operations
- Support different animation speeds (slow, normal, fast)

### Performance Guidelines
- Use efficient data structures (List<T>, Dictionary<K,V>)
- Implement caching for expensive calculations
- Use streaming for large datasets
- Provide async operations with CancellationToken support
- Include memory cleanup and resource disposal

## Solar Domain Knowledge

### Data Structure Understanding
- **Daily Data**: Day of year, production (P), consumption (U), injection (I)
- **Weather Data**: Temperature, precipitation, wind, pressure, sunshine
- **Quarter-hourly**: 15-minute interval measurements
- **Anomalies**: Production, consumption, and injection anomalies with severity
- **Time Data**: Sunrise/sunset times for daylight calculations

### Energy Calculations
- **Energy Efficiency** = (Consumption / Production) × 100
- **Energy Balance** = Production - Consumption
- **Grid Injection** = Surplus energy fed back to grid
- **Peak Production** = Maximum quarter-hourly generation
- **Daylight Hours** = Sunset time - Sunrise time

### Weather Classifications
- **Sunny**: High sunshine hours, low precipitation
- **Cloudy**: Low sunshine, no precipitation
- **Rainy**: High precipitation, low sunshine
- **Stormy**: High wind speed + precipitation

## User Experience Principles

- **Joyful Interactions** - Fun animations, themes, and delightful experiences
- **Progressive Disclosure** - Simple overviews expanding to detailed analysis
- **Consistent Visual Language** - Uniform colors, emojis, and formatting
- **Accessibility Conscious** - High contrast colors and clear text
- **Performance Focused** - Fast startup and responsive interactions
- **Educational Value** - Promote renewable energy awareness and understanding

## Error Handling Standards

- Provide helpful error messages with suggested solutions
- Use structured logging for debugging information
- Gracefully handle missing or malformed data
- Offer alternative actions when operations fail
- Include file path and line number information in error messages

## Testing Approach (Future)

- Unit tests for data models and calculations
- Integration tests for data loading and command execution
- Performance tests for large datasets
- Mock data for consistent testing scenarios
- Benchmark tests for critical performance paths

## Documentation Standards

- Include comprehensive XML documentation for public APIs
- Provide usage examples in code comments
- Update README.md for new features and changes
- Document breaking changes in commit messages
- Include performance characteristics for major features

## Hackathon Alignment

This project was created for GitHub's "For the Love of Code 2025" hackathon:
- **Category:** Terminal talent
- **Focus:** Joyful, useful, and beautifully crafted CLI experience
- **Values:** Open source, community-friendly, educational impact
- **Goal:** Showcase the power of beautiful command-line interfaces while promoting renewable energy awareness

## Community Guidelines

- Follow the MIT License for all contributions
- Maintain welcoming and inclusive language
- Provide clear contribution guidelines
- Support cross-platform compatibility (Windows, macOS, Linux)
- Encourage educational use and renewable energy advocacy
