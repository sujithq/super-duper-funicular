#!/bin/bash

# SolarScope CLI Demo Script
# For the Love of Code 2025 - Comprehensive Feature Showcase

echo "🌞======================================🌞"
echo "   SolarScope CLI - Complete Demo"
echo "   For the Love of Code 2025"
echo "🌞======================================🌞"
echo ""

# Check if we're in the right directory
if [ ! -f "src/Program.cs" ]; then
    echo "❌ Please run this script from the project root directory"
    exit 1
fi

cd src

echo "🎯 Starting comprehensive SolarScope CLI demonstration..."
echo ""
echo "Press ENTER after each demo to continue..."
read -p ""

echo "1️⃣ BASIC DASHBOARD - Quick Overview"
echo "----------------------------------------"
echo "Command: dotnet run -- dashboard"
echo ""
dotnet run -- dashboard
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "2️⃣ FULL DASHBOARD - Complete Overview with Charts"
echo "----------------------------------------"
echo "Command: dotnet run -- dashboard --full"
echo ""
dotnet run -- dashboard --full
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "3️⃣ ANIMATED DASHBOARD - Dynamic Experience"
echo "----------------------------------------"
echo "Command: dotnet run -- dashboard --animated --full"
echo ""
dotnet run -- dashboard --animated --full
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "4️⃣ PRODUCTION ANALYSIS - Top Performing Days"
echo "----------------------------------------"
echo "Command: dotnet run -- analyze --type production --count 10"
echo ""
dotnet run -- analyze --type production --count 10
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "5️⃣ WEATHER CORRELATION - Climate Impact Analysis"
echo "----------------------------------------"
echo "Command: dotnet run -- analyze --type correlation"
echo ""
dotnet run -- analyze --type correlation
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "6️⃣ ANOMALY DETECTION - System Issues"
echo "----------------------------------------"
echo "Command: dotnet run -- anomalies --severity low"
echo ""
dotnet run -- anomalies --severity low
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "7️⃣ WEATHER ANALYSIS - Meteorological Insights"
echo "----------------------------------------"
echo "Command: dotnet run -- weather --correlation"
echo ""
dotnet run -- weather --correlation
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "8️⃣ MONTHLY REPORT - Comprehensive Statistics"
echo "----------------------------------------"
echo "Command: dotnet run -- report --type monthly"
echo ""
dotnet run -- report --type monthly
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "9️⃣ SOLAR THEME DEMO - Animated Solar Experience"
echo "----------------------------------------"
echo "Command: dotnet run -- demo --theme solar --speed normal"
echo ""
dotnet run -- demo --theme solar --speed normal
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "🔟 MATRIX THEME DEMO - Cyberpunk Solar Matrix"
echo "----------------------------------------"
echo "Command: dotnet run -- demo --theme matrix --speed fast"
echo ""
dotnet run -- demo --theme matrix --speed fast
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "1️⃣1️⃣ RAINBOW THEME DEMO - Colorful Celebration"
echo "----------------------------------------"
echo "Command: dotnet run -- demo --theme rainbow --speed normal"
echo ""
dotnet run -- demo --theme rainbow --speed normal
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "1️⃣2️⃣ HELP SYSTEM - Complete Command Reference"
echo "----------------------------------------"
echo "Command: dotnet run -- --help"
echo ""
dotnet run -- --help
echo ""
read -p "Press ENTER to continue..."

echo ""
echo "🎉======================================🎉"
echo "   SolarScope CLI Demo Complete!"
echo ""
echo "✨ Features Demonstrated:"
echo "   • Interactive dashboards with animations"
echo "   • Advanced data analytics and correlations"
echo "   • Beautiful charts and visualizations"
echo "   • Anomaly detection and reporting"
echo "   • Weather impact analysis"
echo "   • Multiple themes and visual effects"
echo "   • Comprehensive help system"
echo ""
echo "🌟 Built with:"
echo "   • .NET 8.0 & C# 12"
echo "   • Spectre.Console for beautiful UI"
echo "   • CommandLineParser for CLI parsing"
echo "   • System.Text.Json for data processing"
echo ""
echo "🏆 For GitHub's 'For the Love of Code 2025'"
echo "   Category: Terminal talent"
echo "   Focus: Joyful, useful, beautiful CLI"
echo ""
echo "💚 Making solar energy monitoring fun!"
echo "🎉======================================🎉"
echo ""
echo "🌞 Thank you for trying SolarScope CLI! 🌞"
