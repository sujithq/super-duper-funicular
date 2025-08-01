@echo off
REM SolarScope CLI Build Script for Windows
REM For the Love of Code 2025

echo 🌞 Building SolarScope CLI...
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ❌ .NET SDK not found. Please install .NET 9.0 or later.
    echo    Download from: https://dotnet.microsoft.com/download
    exit /b 1
)

REM Display .NET version
echo ✅ .NET SDK Version:
dotnet --version
echo.

REM Navigate to source directory
cd src

REM Restore packages
echo 📦 Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo ❌ Package restore failed!
    exit /b 1
)

REM Build project
echo 🔨 Building project...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed!
    exit /b 1
)

echo.
echo ✅ Build completed successfully!
echo.
echo 🚀 Quick Start:
echo    dotnet run -- dashboard
echo    dotnet run -- demo --theme solar
echo    dotnet run -- analyze --type production
echo.
echo 📚 For full help:
echo    dotnet run -- --help
echo.
echo 🌟 Built with ❤️ for GitHub's For the Love of Code 2025!
