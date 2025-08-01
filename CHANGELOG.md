## [Unreleased]

#  (2025-08-01)
### Bug Fixes
* add permissions section to GitHub Actions workflows to resolve 403 errors ([881cf74](https://github.com/sujithq/super-duper-funicular/commit/881cf74d0364f2b462d903c1042b44638832ebec))
* Standardize date formatting in GetDateFromDayOfYear method for consistency ([c7ce158](https://github.com/sujithq/super-duper-funicular/commit/c7ce158c8c5e0a941dbdeeb5086417aa496c1e39))
* Update best production day and recent days display to include formatted dates for clarity ([f5438da](https://github.com/sujithq/super-duper-funicular/commit/f5438dab7d117c019c42dfa6b297cd20b25c2204))
* Update formatting for numerical outputs in AnalyzeCommand, DashboardCommand, and ReportCommand for consistency ([8e73b04](https://github.com/sujithq/super-duper-funicular/commit/8e73b04ee0e444c611a3d96ca0473fb78cb05e23))
### Features
* Enhance LoadDataAsync method with verbose logging and fallback file handling ([bf0394d](https://github.com/sujithq/super-duper-funicular/commit/bf0394daf26ae46c1234f067ef4386c52411bab4))
* Implement comprehensive solar data analysis CLI ([8a22da3](https://github.com/sujithq/super-duper-funicular/commit/8a22da34d0e174f36cfe253c4d018b8f6f1e17e3))
* Introduce BaseCommand class and refactor existing commands to inherit from it for improved code reuse and maintainability ([16d6057](https://github.com/sujithq/super-duper-funicular/commit/16d60573a021299a20df74d771ccb9418e5b3708))
* **setup:** add scripts for configuring Conventional Commits ([e9d25f2](https://github.com/sujithq/super-duper-funicular/commit/e9d25f2f95096841929ac1c8cff356a304409e92))
* Upgrade project to .NET 9.0 and update dependencies ([66b0c75](https://github.com/sujithq/super-duper-funicular/commit/66b0c7558d841d3612b3105263b5f9aa065509c9))

# Changelog

All notable changes to SolarScope CLI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-08-01

### Added

- Initial release of SolarScope CLI
- Interactive dashboard with real-time solar system overview
- Advanced analytics for production, weather, and anomaly analysis
- Anomaly detection with severity classification (Low, Medium, High)
- Weather integration and correlation analysis
- Comprehensive reporting (daily, weekly, monthly, yearly)
- Multiple themes (Solar, Matrix, Rainbow) for demo mode
- Beautiful charts and visualizations using Spectre.Console
- Animated displays with customizable speed settings
- Automatic data file management with user profile fallback
- Cross-platform support (.NET 9.0)
- Command-line interface with comprehensive help
- Verbose logging and error handling
- Educational renewable energy content

### Features

- **Commands**: `dashboard`, `analyze`, `anomalies`, `report`, `weather`, `explore`, `demo`
- **Data Formats**: JSON solar system data with weather correlations
- **Visualizations**: Bar charts, breakdown charts, status indicators
- **Animations**: Typewriter effects, rainbow themes, matrix animations
- **Analysis**: Statistical calculations, trend detection, performance metrics

### Technical

- Built with .NET 9.0 and C# 13
- Uses Spectre.Console 0.50.0 for terminal UI
- Uses Spectre.Console.Cli 0.50.0 for command structure
- JSON processing with System.Text.Json 9.0.7
- Cross-platform compatibility (Windows, macOS, Linux)
- Published as .NET Global Tool on NuGet.org

### Documentation

- Comprehensive README.md with usage examples
- Command reference documentation
- Installation and setup guides
- Contributing guidelines
- Project architecture documentation

[Unreleased]: https://github.com/sujithq/super-duper-funicular/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/sujithq/super-duper-funicular/releases/tag/v1.0.0
