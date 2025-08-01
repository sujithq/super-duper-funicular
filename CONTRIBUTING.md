# 🤝 Contributing to SolarScope CLI

Thank you for your interest in contributing to SolarScope CLI! This project was created for GitHub's "For the Love of Code 2025" hackathon, and we welcome contributions that help make solar energy monitoring more accessible and fun.

---

> **Visual Demos:**
> For live demo GIFs and visual examples of SolarScope CLI, see the [README.md](./README.md#-live-demo-gifs) section. You can also generate your own GIFs using the included VHS tape scripts!

## 🌟 Ways to Contribute

### 🐛 Bug Reports
- Use GitHub Issues to report bugs
- Include steps to reproduce the issue
- Provide your system information (.NET version, OS)
- Include sample data if relevant

### 💡 Feature Requests
- Suggest new analysis features
- Propose UI/UX improvements
- Request new data visualization types
- Ideas for additional themes or animations

### 🔧 Code Contributions
- Fix bugs or implement new features
- Improve performance or code quality
- Add tests for existing functionality
- Enhance documentation

## 🛠️ Development Setup

### Prerequisites
- .NET 9.0 SDK or later
- Git
- Your favorite C# IDE (Visual Studio, VS Code, JetBrains Rider)

### Getting Started
1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your feature
4. Make your changes
5. Test your changes
6. Submit a pull request

```bash
git clone https://github.com/your-username/super-duper-funicular.git
cd super-duper-funicular
git checkout -b feature/your-feature-name
```

### Building the Project
```bash
# Windows
build.bat

# Linux/macOS
chmod +x build.sh
./build.sh
```

### Running the Application
```bash
cd src
dotnet run -- dashboard
dotnet run -- demo --theme rainbow
```

## 📝 Code Guidelines

### C# Coding Standards
- Follow Microsoft C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Use records for data models where appropriate
- Implement proper error handling

### File Organization
```
src/
├── Commands/           # Command implementations
├── Services/          # Business logic and data access
├── Models/            # Data models and types
├── Tests/             # Unit tests (when added)
└── Program.cs         # Application entry point
```

### Naming Conventions
- `PascalCase` for classes, methods, properties
- `camelCase` for local variables and parameters
- `UPPER_CASE` for constants
- Descriptive names over short abbreviations

### Documentation
- Add XML comments for public methods and classes
- Update README.md for new features
- Include code examples in documentation
- Document any breaking changes

## 🎨 UI/UX Guidelines

### Terminal Experience
- Use Spectre.Console for all UI elements
- Maintain consistent color schemes
- Include appropriate emojis for visual appeal
- Ensure responsive design for different terminal sizes

### Animation and Effects
- Keep animations smooth and purposeful
- Provide speed options (slow, normal, fast)
- Allow users to disable animations if needed
- Test on different terminal types

### Accessibility
- Use high contrast colors
- Provide text alternatives for visual elements
- Support keyboard navigation
- Test with screen readers when possible

## 🧪 Testing

### Test Categories
- Unit tests for data models and calculations
- Integration tests for data loading
- Command execution tests
- Performance tests for large datasets

### Testing Tools
- xUnit for unit testing framework
- Mock data for consistent testing
- Benchmark.NET for performance testing

### Test Structure
```csharp
[Fact]
public void Method_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = CreateTestData();
    
    // Act
    var result = MethodUnderTest(input);
    
    // Assert
    Assert.Equal(expected, result);
}
```

## 📊 Data Processing

### Supported Data Formats
- JSON solar system data
- Weather correlation data
- Anomaly detection metrics
- Quarter-hourly measurements

### Adding New Data Types
1. Define data models in `Models/`
2. Update JSON serialization attributes
3. Add processing logic in `Services/`
4. Create visualization in `Commands/`
5. Update documentation

## 🎯 Performance Considerations

### Optimization Guidelines
- Use async/await for I/O operations
- Implement efficient data structures
- Cache calculations when appropriate
- Provide progress indicators for long operations

### Memory Management
- Dispose of resources properly
- Use streaming for large datasets
- Avoid unnecessary object allocations
- Profile memory usage regularly

## 🚀 Release Process

### Version Numbering
- Follow Semantic Versioning (SemVer)
- Major.Minor.Patch format
- Document breaking changes

### Release Checklist
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Performance benchmarks run
- [ ] Cross-platform testing completed
- [ ] Release notes prepared

## 🏆 Recognition

### Contributors
All contributors will be recognized in:
- Repository README
- Release notes
- Project documentation
- Social media announcements

### Code of Conduct
This project follows the [GitHub Community Code of Conduct](https://docs.github.com/en/site-policy/github-terms/github-community-code-of-conduct). Please be respectful and inclusive in all interactions.

## 🎉 Hackathon Context

This project was created for **GitHub's "For the Love of Code 2025"** hackathon:
- **Category:** Terminal talent
- **Focus:** Joyful, useful, and beautifully crafted CLI experience
- **Timeline:** July 16 - September 22, 2025
- **Submission:** Must be in public GitHub repository

## 📞 Getting Help

### Resources
- [GitHub Issues](https://github.com/sujithq/super-duper-funicular/issues) - Bug reports and feature requests
- [GitHub Discussions](https://github.com/sujithq/super-duper-funicular/discussions) - General questions and ideas
- [Project Wiki](https://github.com/sujithq/super-duper-funicular/wiki) - Detailed documentation

### Community
- Be patient and helpful with newcomers
- Share knowledge and best practices
- Collaborate respectfully on solutions
- Celebrate successes together

---

**Thank you for contributing to SolarScope CLI! Together, we're making solar energy monitoring more accessible and enjoyable for everyone.** 🌞⚡💚
