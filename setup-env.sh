#!/bin/bash
# Quick setup script for SolarScope CLI environment

echo "🌞 SolarScope CLI Environment Setup"
echo "==================================="

# Check if .env already exists
if [ -f ".env" ]; then
    echo "⚠️  .env file already exists"
    read -p "Do you want to overwrite it? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Setup cancelled"
        exit 0
    fi
fi

# Copy example file
cp .env.example .env
echo "✅ Created .env file from template"

echo ""
echo "📝 Please edit the .env file and add your GitHub Personal Access Token:"
echo "   1. Open .env in your favorite editor"
echo "   2. Replace 'your_github_token_here' with your actual token"
echo "   3. Save the file"
echo ""
echo "🔗 Create a GitHub token at: https://github.com/settings/tokens"
echo "   Make sure to enable 'models:read' permission"
echo ""
echo "🚀 Once configured, test with:"
echo "   ./run-with-env.sh ai \"What commands are available?\""
