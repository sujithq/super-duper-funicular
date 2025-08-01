---
applyTo: "**/*.md"
---

# Documentation Guidelines

Documentation in SolarScope CLI should be comprehensive, engaging, and educational.

## Markdown Standards

### Structure and Formatting
- Use clear heading hierarchy (H1 for main topics, H2 for sections, etc.)
- Include table of contents for long documents
- Use code blocks with appropriate language syntax highlighting
- Add blank lines around headings, lists, and code blocks for readability

### Visual Elements
- Use appropriate emojis to enhance readability: 🌞 ⚡ 📊 🏆 ❗ 💡 🚀
- Include badges for status, version, and build information
- Add screenshots or ASCII art for visual examples
- Use consistent formatting for commands, file names, and options

### Content Guidelines
- Write for multiple audiences (developers, users, contributors)
- Include practical examples and use cases
- Provide troubleshooting sections for common issues
- Link to relevant external resources and documentation

## README Structure

### Required Sections
1. **Project Title and Description** - Clear, engaging overview
2. **Features** - Bullet-pointed list of key capabilities
3. **Installation** - Step-by-step setup instructions
4. **Usage** - Command examples with expected output
5. **Contributing** - Link to contribution guidelines
6. **License** - License information and attribution

### Solar Domain Context
- Explain renewable energy benefits and importance
- Include educational content about solar monitoring
- Provide context for data analysis and interpretation
- Use terminology that's accessible to non-technical users

## API Documentation

### Code Comments
- Use XML documentation for all public methods and classes
- Include parameter descriptions and return value explanations
- Add usage examples in `<example>` tags
- Document exceptions that may be thrown

### Inline Documentation
```csharp
/// <summary>
/// Calculates energy efficiency as the ratio of consumption to production.
/// </summary>
/// <param name="production">Total energy produced in kWh</param>
/// <param name="consumption">Total energy consumed in kWh</param>
/// <returns>Efficiency percentage (0-100+)</returns>
/// <example>
/// <code>
/// var efficiency = CalculateEfficiency(20.5, 18.2); // Returns ~88.8%
/// </code>
/// </example>
public double CalculateEfficiency(double production, double consumption)
```

## Educational Content

### Learning Objectives
- Explain concepts before diving into technical details
- Provide background on solar energy systems
- Include glossary of terms for domain-specific vocabulary
- Link to external educational resources

### Accessibility
- Use clear, simple language where possible
- Define technical terms when first introduced
- Provide multiple examples for complex concepts
- Include visual aids and diagrams when helpful

## Contribution Documentation

### Setup Instructions
- Provide detailed development environment setup
- Include troubleshooting for common setup issues
- List all required tools and dependencies
- Document build and test procedures

### Coding Standards Reference
- Link to established coding conventions
- Provide examples of preferred patterns
- Document architecture decisions and rationale
- Include templates for common tasks

## Command Documentation

### Help Text Guidelines
- Write concise but descriptive command descriptions
- Include usage examples for each command
- Document all options and their effects
- Provide examples of common use cases

### Error Messages
- Use clear, actionable language
- Suggest specific solutions when possible
- Include relevant context information
- Maintain consistent tone and style

## Release Documentation

### Changelog Format
- Follow semantic versioning principles
- Group changes by type (Added, Changed, Deprecated, Removed, Fixed, Security)
- Include migration instructions for breaking changes
- Link to relevant issues and pull requests

### Version Documentation
- Document new features with examples
- Include performance improvements and benchmarks
- Note compatibility requirements and changes
- Provide upgrade instructions when needed
