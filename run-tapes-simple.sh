#!/bin/bash

# Simple VHS Tape Runner - Run all tapes in the tapes directory
# For SolarScope CLI - For the Love of Code 2025

echo "🎬 Running all VHS tapes..."
echo ""

# Check if VHS is installed
if ! command -v vhs &> /dev/null; then
    echo "❌ VHS not found. Install with: go install github.com/charmbracelet/vhs@latest"
    exit 1
fi

# Check if tapes directory exists
if [ ! -d "tapes" ]; then
    echo "❌ Please run from project root (tapes directory not found)"
    exit 1
fi

cd tapes

# Count total tapes
total=$(ls -1 *.tape 2>/dev/null | wc -l)
if [ $total -eq 0 ]; then
    echo "❌ No tape files found in tapes directory"
    exit 1
fi

echo "📼 Found $total tape files"
echo ""

current=0
failed=0

# Run all tapes
for tape in *.tape; do
    current=$((current + 1))
    gif="${tape%.tape}.gif"
    
    echo "[$current/$total] 🎥 $tape → $gif"
    
    if vhs "$tape" > /dev/null 2>&1; then
        echo "         ✅ Success"
    else
        echo "         ❌ Failed"
        failed=$((failed + 1))
    fi
    echo ""
done

echo "🎉 Complete!"
echo "✅ Success: $((current - failed))/$current"
echo "❌ Failed: $failed/$current"

if [ $failed -eq 0 ]; then
    echo "🏆 All tapes processed successfully!"
else
    echo "⚠️  Some tapes failed - check error messages above"
fi
