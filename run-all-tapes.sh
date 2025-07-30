#!/bin/bash

# VHS Tape Runner Script for SolarScope CLI
# Generates all demo GIFs from VHS tape files
# For the Love of Code 2025

echo "🎬 SolarScope CLI - VHS Tape Runner"
echo "=================================="
echo ""

# Check if VHS is installed
if ! command -v vhs &> /dev/null; then
    echo "❌ VHS not found. Please install VHS first:"
    echo "   go install github.com/charmbracelet/vhs@latest"
    echo "   or visit: https://github.com/charmbracelet/vhs"
    exit 1
fi

# Check if we're in the right directory
if [ ! -d "tapes" ]; then
    echo "❌ Please run this script from the project root directory"
    echo "   The 'tapes' directory should be present"
    exit 1
fi

cd tapes

echo "✅ VHS found, starting tape processing..."
echo "📁 Processing $(ls -1 *.tape | wc -l) tape files..."
echo ""

# Function to run VHS with progress indication
run_vhs() {
    local tape_file="$1"
    local gif_file="${tape_file%.tape}.gif"
    
    echo "🎥 Processing: $tape_file → $gif_file"
    
    # Run VHS and capture output
    if vhs "$tape_file" > /dev/null 2>&1; then
        echo "✅ Success: $gif_file created"
    else
        echo "❌ Failed: $tape_file"
        return 1
    fi
    echo ""
}

# Counter for progress tracking
total_tapes=$(ls -1 *.tape | wc -l)
current=0
failed=0

echo "🚀 Starting VHS processing..."
echo ""

# Dashboard Commands
echo "🏠 Dashboard Commands"
echo "--------------------"
for tape in dashboard.tape dashboard-animated.tape dashboard-full.tape dashboard-animated-full.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Analyze Commands
echo "📊 Analyze Commands"
echo "-------------------"
for tape in analyze.tape analyze-production.tape analyze-weather.tape analyze-anomalies.tape analyze-correlation.tape analyze-production-15.tape analyze-weather-20.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Anomalies Commands
echo "⚠️ Anomalies Commands"
echo "--------------------"
for tape in anomalies.tape anomalies-interactive.tape anomalies-high.tape anomalies-medium.tape anomalies-year-2024.tape anomalies-2025.tape anomalies-high-2023.tape anomalies-high-2025.tape anomalies-interactive-2024.tape anomalies-interactive-2025.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Report Commands
echo "📋 Report Commands"
echo "------------------"
for tape in report.tape report-monthly.tape report-yearly.tape report-daily.tape report-daily-range.tape report-monthly-2024.tape report-monthly-2025.tape report-yearly-2023.tape report-yearly-2025.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Weather Commands
echo "🌦️ Weather Commands"
echo "-------------------"
for tape in weather.tape weather-overview.tape weather-correlation.tape weather-patterns.tape weather-recommendations.tape weather-correlation-2024.tape weather-correlation-2025.tape weather-overview-2025.tape weather-patterns-2023.tape weather-patterns-2025.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Explore Commands
echo "🔍 Explore Commands"
echo "-------------------"
for tape in explore.tape explore-quick.tape explore-guided.tape explore-full.tape explore-guided-2023.tape explore-guided-2025.tape explore-full-2024.tape explore-full-2025.tape explore-quick-2025.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Demo Commands
echo "🎮 Demo Commands"
echo "----------------"
for tape in demo.tape demo-solar-slow.tape demo-solar-normal.tape demo-solar-fast.tape demo-matrix-slow.tape demo-matrix-normal.tape demo-matrix-fast.tape demo-rainbow-slow.tape demo-rainbow-normal.tape demo-rainbow-fast.tape; do
    if [ -f "$tape" ]; then
        current=$((current + 1))
        echo "[$current/$total_tapes]"
        run_vhs "$tape" || failed=$((failed + 1))
    fi
done

# Process any remaining tapes not covered above
echo "📦 Additional Tapes"
echo "-------------------"
for tape in *.tape; do
    # Skip if already processed
    case "$tape" in
        dashboard*.tape|analyze*.tape|anomalies*.tape|report*.tape|weather*.tape|explore*.tape|demo*.tape)
            continue
            ;;
        *)
            current=$((current + 1))
            echo "[$current/$total_tapes]"
            run_vhs "$tape" || failed=$((failed + 1))
            ;;
    esac
done

echo ""
echo "🎉 VHS Processing Complete!"
echo "=========================="
echo "✅ Processed: $current tape files"
echo "❌ Failed: $failed tape files"
echo "📊 Success Rate: $(( (current - failed) * 100 / current ))%"
echo ""

# List generated GIF files
gif_count=$(ls -1 *.gif 2>/dev/null | wc -l)
echo "🎬 Generated GIF files: $gif_count"
if [ $gif_count -gt 0 ]; then
    echo ""
    echo "📂 Generated files:"
    ls -1 *.gif | head -10
    if [ $gif_count -gt 10 ]; then
        echo "   ... and $((gif_count - 10)) more files"
    fi
fi

echo ""
echo "🌞 SolarScope CLI demo GIFs are ready!"
echo "   Use them for documentation, presentations, or showcasing the CLI"
echo ""

if [ $failed -gt 0 ]; then
    echo "⚠️  Some tapes failed to process. Check the error messages above."
    exit 1
else
    echo "🏆 All tapes processed successfully!"
    exit 0
fi
