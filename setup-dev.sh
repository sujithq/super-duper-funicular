#!/bin/bash
# Setup script for SolarScope CLI development environment

echo "🌞 SolarScope CLI - Development Environment Setup"
echo "================================================="

# Function to setup launch settings
setup_launch_settings() {
    echo ""
    echo "📝 Setting up VS Code launch settings with environment variables..."
    
    if [ -f "src/Properties/launchSettings.local.json" ]; then
        echo "⚠️  launchSettings.local.json already exists"
        read -p "Do you want to overwrite it? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "❌ Launch settings setup skipped"
            return
        fi
    fi
    
    # Copy template
    cp src/Properties/launchSettings.local.json.template src/Properties/launchSettings.local.json 2>/dev/null || {
        echo "Creating launch settings template..."
        cat > src/Properties/launchSettings.local.json << 'EOF'
{
  "profiles": {
    "SolarScope": {
      "commandName": "Project",
      "commandLineArgs": "ai \"what commands are available?\"",
      "environmentVariables": {
        "GITHUB_TOKEN": "your_github_token_here"
      }
    },
    "SolarScope Dashboard Animated": {
      "commandName": "Project",
      "commandLineArgs": "dashboard --animated",
      "environmentVariables": {
        "GITHUB_TOKEN": "your_github_token_here"
      }
    },
    "SolarScope Weather": {
      "commandName": "Project",
      "commandLineArgs": "weather --analysis overview",
      "environmentVariables": {
        "GITHUB_TOKEN": "your_github_token_here"
      }
    },
    "SolarScope Help": {
      "commandName": "Project",
      "commandLineArgs": "--help"
    },
    "SolarScope AI Test": {
      "commandName": "Project",
      "commandLineArgs": "ai \"Show me the dashboard\"",
      "environmentVariables": {
        "GITHUB_TOKEN": "your_github_token_here"
      }
    }
  }
}
EOF
    }
    
    echo "✅ Created src/Properties/launchSettings.local.json"
    echo "📝 Please edit this file and replace 'your_github_token_here' with your actual GitHub token"
}

# Function to setup .env file
setup_env_file() {
    echo ""
    echo "📁 Setting up .env file..."
    
    if [ -f ".env" ]; then
        echo "⚠️  .env file already exists"
        read -p "Do you want to overwrite it? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "❌ .env setup skipped"
            return
        fi
    fi
    
    cp .env.example .env
    echo "✅ Created .env file from template"
    echo "📝 Please edit .env and add your GitHub token"
}

# Main setup
echo ""
echo "🔧 What would you like to set up?"
echo "1) .env file for command-line usage"
echo "2) VS Code launch settings for debugging"
echo "3) Both"
echo ""
read -p "Enter your choice (1-3): " -n 1 -r
echo

case $REPLY in
    1)
        setup_env_file
        ;;
    2)
        setup_launch_settings
        ;;
    3)
        setup_env_file
        setup_launch_settings
        ;;
    *)
        echo "❌ Invalid choice"
        exit 1
        ;;
esac

echo ""
echo "🔗 Next steps:"
echo "1. Create a GitHub Personal Access Token at: https://github.com/settings/tokens"
echo "2. Enable 'models:read' permission"
echo "3. Add your token to the files you just created"
echo ""
echo "🚀 Test your setup:"
echo "   ./run-with-env.sh ai \"what commands are available?\"  # For .env"
echo "   Press F5 in VS Code                                    # For launch settings"
echo ""
echo "✅ Setup complete!"
