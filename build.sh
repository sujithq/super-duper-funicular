#!/bin/bash

# SolarScope CLI Build Script
# For the Love of Code 2025

echo "🌞 Building SolarScope CLI..."
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9.0 or later."
    echo "   Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Display .NET version
echo "✅ .NET SDK Version:"
dotnet --version
echo ""

# Navigate to source directory
cd src

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Package restore failed!"
    exit 1
fi

# Build project
echo "🔨 Building project..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo ""
echo "✅ Build completed successfully!"
echo ""
echo "🚀 Quick Start:"
echo "   dotnet run -- dashboard"
echo "   dotnet run -- demo --theme solar"
echo "   dotnet run -- analyze --type production"
echo ""
echo "📚 For full help:"
echo "   dotnet run -- --help"
echo ""
echo "🌟 Built with ❤️ for GitHub's For the Love of Code 2025!"
