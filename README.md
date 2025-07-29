# 🌞 SolarScope CLI - Your Personal Solar System Command Center

[![For the Love of Code 2025](https://img.shields.io/badge/For%20the%20Love%20of%20Code-2025-orange?style=flat-square)](https://github.blog/open-source/for-the-love-of-code-2025/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Spectre.Console](https://img.shields.io/badge/Spectre.Console-0.49.1-blue?style=flat-square)](https://spectreconsole.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)

**A beautiful, interactive command-line tool for monitoring and analyzing your solar energy system with weather correlations and anomaly detection.**

> Built with ❤️ for GitHub's "For the Love of Code 2025" hackathon - Category 3: Terminal talent

## ✨ Features

### 🎯 Core Functionality
- **Interactive Dashboard** - Real-time solar system overview with beautiful charts
- **Advanced Analytics** - Production analysis, weather correlation, and trend detection  
- **Anomaly Detection** - Intelligent system monitoring with severity classification
- **Weather Integration** - Correlate weather patterns with energy production
- **Comprehensive Reports** - Monthly, yearly, and custom reporting

### 🎨 Visual Excellence
- **Beautiful Charts** - Bar charts, breakdown charts, and data visualizations
- **Animated Displays** - Dynamic loading, real-time updates, and smooth transitions
- **Multiple Themes** - Solar, Matrix, and Rainbow themes for the demo mode
- **Rich Formatting** - Colors, emojis, panels, and progress indicators
- **Responsive Design** - Adapts to your terminal size and capabilities

### 🚀 Interactive Experience
- **Command-line Interface** - Intuitive commands with comprehensive help
- **Progress Indicators** - Visual feedback for all operations
- **Status Updates** - Real-time processing information
- **Error Handling** - Graceful error messages and suggestions
- **Verbose Mode** - Detailed logging for troubleshooting

## 🏗️ Architecture

### Data Models
- **Rich Data Types** - Comprehensive models for solar, weather, and anomaly data
- **Type Safety** - Full C# record types with JSON serialization
- **Calculated Properties** - Automatic efficiency, balance, and trend calculations
- **Enum Classifications** - Weather conditions, anomaly severity, and wind classifications

### Services Layer
- **Data Service** - Efficient JSON parsing and data access
- **Analytics Engine** - Statistical analysis and correlation calculations
- **Anomaly Detection** - Smart pattern recognition and severity assessment
- **Report Generation** - Flexible reporting with multiple output formats

### Command Architecture
- **Modular Commands** - Separate command classes for each feature
- **Shared Options** - Common base options with command-specific extensions
- **Async Processing** - Non-blocking operations with progress indication
- **Error Recovery** - Comprehensive error handling and user guidance

## 🎮 Demo Modes

### Solar Theme (Default)
- Animated solar system ASCII art
- Energy flow visualizations
- Weather effect simulations
- Production timeline animations

### Matrix Theme
- Digital rain intro sequence
- Green terminal aesthetics
- Glitch effects and system messages
- Cyberpunk data visualization

### Rainbow Theme
- Colorful data presentations
- Animated rainbow effects
- Vibrant charts and indicators
- Celebration animations

## 📊 Sample Data Analysis

The tool analyzes your solar system data including:

- **Daily Production Metrics** - Energy generation, consumption, and grid injection
- **Quarter-hourly Readings** - Detailed 15-minute interval measurements
- **Weather Correlations** - Temperature, precipitation, sunshine, and wind analysis
- **System Anomalies** - Automatic detection of unusual patterns or issues
- **Performance Trends** - Historical analysis and efficiency calculations

## 🛠️ Installation & Setup

### Prerequisites
- .NET 8.0 SDK or later
- Windows, macOS, or Linux
- Terminal with Unicode support (recommended)

### Quick Start

1. **Clone the repository**
```bash
git clone https://github.com/sujithq/super-duper-funicular.git
cd super-duper-funicular
```

2. **Build the project**
```bash
cd src
dotnet build
```

3. **Run the application**
```bash
dotnet run -- dashboard
```

### Package Installation (Alternative)
```bash
dotnet pack
dotnet tool install --global --add-source ./nupkg SolarScope
solarscope dashboard
```

## 🎯 Usage Examples

### Dashboard Commands
```bash
# Quick dashboard overview
solarscope dashboard

# Full dashboard with all charts
solarscope dashboard --full

# Animated dashboard experience  
solarscope dashboard --animated --full
```

### Analysis Commands
```bash
# Analyze production patterns
solarscope analyze --type production --count 15

# Weather correlation analysis
solarscope analyze --type correlation

# Anomaly detection
solarscope analyze --type anomalies --count 10

# Custom date range analysis
solarscope analyze --type weather --start-day 100 --end-day 200
```

### Reporting Commands
```bash
# Generate monthly report
solarscope report --type monthly

# Yearly summary report
solarscope report --type yearly --format table

# Export to file
solarscope report --type monthly --output monthly_report.json --format json
```

### Specialized Commands
```bash
# Detect system anomalies
solarscope anomalies --severity medium --interactive

# Weather analysis with correlation
solarscope weather --correlation --historical

# Interactive data exploration
solarscope explore --mode guided
```

### Fun Demo Commands
```bash
# Solar-themed demo
solarscope demo --theme solar --speed normal

# Matrix-style visualization
solarscope demo --theme matrix --speed slow

# Rainbow celebration mode
solarscope demo --theme rainbow --speed fast
```

## 🎨 Visual Examples

### Dashboard Overview
```
🌞 Solar System Dashboard 🌞
┌─────────────────────────────────────────┐
│ ⚡ Total Production: 2,847.5 kWh     ✅ │
│ 🏠 Total Consumption: 2,156.8 kWh   ✅ │  
│ 🔌 Grid Injection: 690.7 kWh        ✅ │
│ 📊 Average Daily: 12.8 kWh          ✅ │
│ 🏆 Best Day: Day 156 (28.4 kWh)     🏆 │
│ ⚠️ System Anomalies: 3 detected     ⚠️ │
└─────────────────────────────────────────┘
```

### Production Analysis Chart
```
Daily Production (Last 20 Days)
▅▆▇██▆▅▇▆▅▆▇██▆▅▇▆▅▆▇
Day 344  Day 348  Day 352  Day 356  Day 360
```

### Anomaly Detection
```
⚠️ Anomaly Detection Results
┌─────────────────────────────────────┐
│ High Severity:    2 occurrences    │
│ Medium Severity:  5 occurrences    │  
│ Low Severity:     8 occurrences    │
│ Total Anomalies:  15 detected      │
└─────────────────────────────────────┘
```

## 🔧 Configuration

### Data Sources
The tool expects JSON data in the following format:
```json
{
  "2023": [
    {
      "D": 13,
      "P": 12.533,
      "U": 10.2,
      "I": 2.333,
      "MS": { "tavg": 8.6, "tmin": 7.5, ... },
      "AS": { "P": 0, "U": 0, "I": 0, "A": false },
      "Q": { "C": [...], "I": [...], "G": [...] },
      ...
    }
  ]
}
```

### Custom Data Files
```bash
# Use custom data file
solarscope dashboard --data /path/to/your/solar-data.json

# Enable verbose logging
solarscope analyze --verbose --type production
```

## 🤝 Contributing

This project was created for the GitHub "For the Love of Code 2025" hackathon. Contributions are welcome!

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation for public APIs
- Include unit tests for new features

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

### Technologies Used
- **[.NET 8.0](https://dotnet.microsoft.com/)** - Modern, cross-platform framework
- **[Spectre.Console](https://spectreconsole.net/)** - Beautiful console applications
- **[CommandLineParser](https://github.com/commandlineparser/commandline)** - Command-line argument parsing
- **[System.Text.Json](https://docs.microsoft.com/en-us/dotnet/api/system.text.json)** - High-performance JSON processing

### Inspiration
- **[Rich (Python)](https://github.com/willmcgugan/rich)** - Inspiration for terminal formatting
- **[GitHub CLI](https://cli.github.com/)** - Command structure and user experience
- **Solar Energy Community** - For the importance of renewable energy monitoring

### Hackathon
Created with ❤️ for **[GitHub's "For the Love of Code 2025"](https://github.blog/open-source/for-the-love-of-code-2025/)** hackathon.

**Category:** Terminal talent  
**Focus:** Joyful, useful, and beautifully crafted command-line experience

## � Project Status

The project maintains a comprehensive `PROJECT_SUMMARY.md` file that tracks:
- Complete implementation status and progress
- Technical architecture and design decisions
- Feature documentation and usage examples
- Performance characteristics and metrics
- Community guidelines and contribution opportunities
- Future roadmap and enhancement plans

To keep the summary updated, run:
```bash
# Linux/macOS
./update-summary.sh

# Windows
update-summary.bat
```

## �📞 Support

- **Issues:** [GitHub Issues](https://github.com/sujithq/super-duper-funicular/issues)
- **Discussions:** [GitHub Discussions](https://github.com/sujithq/super-duper-funicular/discussions)
- **Documentation:** [Project Wiki](https://github.com/sujithq/super-duper-funicular/wiki)

---

**🌟 Star this repository if you find SolarScope useful!**

*Built with passion for clean energy and beautiful software* ⚡🌱💚
