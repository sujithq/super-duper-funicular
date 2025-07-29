#!/bin/bash
# Update Project Summary Script
# This script helps maintain the PROJECT_SUMMARY.md file with current status

echo "🔄 Updating PROJECT_SUMMARY.md..."

# Update the last updated date
TODAY=$(date +"%B %d, %Y")
sed -i "s/\*\*Last Updated:\*\* .*/\*\*Last Updated:\*\* $TODAY/" PROJECT_SUMMARY.md

echo "📅 Updated date to: $TODAY"

# Add git commit information if in a git repo
if [ -d ".git" ]; then
    COMMIT_COUNT=$(git rev-list --count HEAD 2>/dev/null || echo "0")
    LAST_COMMIT=$(git log -1 --pretty=format:"%h - %s" 2>/dev/null || echo "No commits")
    echo "📊 Git Status: $COMMIT_COUNT commits, Latest: $LAST_COMMIT"
fi

# Show current file size and line count
if [ -f "PROJECT_SUMMARY.md" ]; then
    LINES=$(wc -l < PROJECT_SUMMARY.md)
    SIZE=$(wc -c < PROJECT_SUMMARY.md)
    echo "📄 Summary Stats: $LINES lines, $SIZE characters"
fi

echo "✅ Project summary update complete!"
echo ""
echo "💡 To manually edit the summary:"
echo "   - Update the status section with new progress"
echo "   - Add new features to the feature list"
echo "   - Update completion percentages"
echo "   - Add lessons learned or insights"
echo "   - Update future roadmap items"
