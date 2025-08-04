#!/bin/bash
# Load environment variables from .env file for Bash/WSL

# Check if .env file exists
if [ ! -f ".env" ]; then
    echo "❌ .env file not found!"
    echo "💡 Copy .env.example to .env and fill in your GitHub token"
    echo "   cp .env.example .env"
    exit 1
fi

# Load environment variables from .env file
echo "🔧 Loading environment variables from .env..."

# Read and export variables, handling whitespace properly
while IFS='=' read -r key value; do
    # Skip comments and empty lines
    if [[ $key =~ ^[[:space:]]*# ]] || [[ -z $key ]]; then
        continue
    fi
    
    # Trim whitespace from key and value
    key=$(echo "$key" | xargs)
    value=$(echo "$value" | xargs)
    
    # Remove quotes if present
    if [[ $value =~ ^\".*\"$ ]]; then
        value="${value:1:-1}"
    fi
    
    # Export the variable
    if [[ -n $key && -n $value ]]; then
        export "$key"="$value"
        echo "   ✓ Set $key"
    fi
done < .env

# Verify GITHUB_TOKEN is set
if [ -z "$GITHUB_TOKEN" ]; then
    echo "❌ GITHUB_TOKEN not set in .env file!"
    echo "💡 Edit .env and set your GitHub Personal Access Token"
    exit 1
fi

echo "✅ Environment variables loaded successfully!"
echo "🤖 You can now use: solarscope ai 'your question here'"

# Run the command if arguments provided
if [ $# -gt 0 ]; then
    echo "🚀 Running: solarscope $*"
    cd src && dotnet run -- "$@"
fi
