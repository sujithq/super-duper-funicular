# Setup script for SolarScope CLI development environment (PowerShell)

Write-Host "🌞 SolarScope CLI - Development Environment Setup" -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Yellow

# Function to setup launch settings
function Setup-LaunchSettings {
    Write-Host ""
    Write-Host "📝 Setting up VS Code launch settings with environment variables..." -ForegroundColor Blue
    
    if (Test-Path "src/Properties/launchSettings.local.json") {
        Write-Host "⚠️  launchSettings.local.json already exists" -ForegroundColor Yellow
        $overwrite = Read-Host "Do you want to overwrite it? (y/N)"
        if ($overwrite -ne "y" -and $overwrite -ne "Y") {
            Write-Host "❌ Launch settings setup skipped" -ForegroundColor Red
            return
        }
    }
    
    # Create launch settings content
    $launchSettings = @'
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
'@
    
    $launchSettings | Out-File -FilePath "src/Properties/launchSettings.local.json" -Encoding UTF8
    Write-Host "✅ Created src/Properties/launchSettings.local.json" -ForegroundColor Green
    Write-Host "📝 Please edit this file and replace 'your_github_token_here' with your actual GitHub token" -ForegroundColor Cyan
}

# Function to setup .env file
function Setup-EnvFile {
    Write-Host ""
    Write-Host "📁 Setting up .env file..." -ForegroundColor Blue
    
    if (Test-Path ".env") {
        Write-Host "⚠️  .env file already exists" -ForegroundColor Yellow
        $overwrite = Read-Host "Do you want to overwrite it? (y/N)"
        if ($overwrite -ne "y" -and $overwrite -ne "Y") {
            Write-Host "❌ .env setup skipped" -ForegroundColor Red
            return
        }
    }
    
    Copy-Item .env.example .env
    Write-Host "✅ Created .env file from template" -ForegroundColor Green
    Write-Host "📝 Please edit .env and add your GitHub token" -ForegroundColor Cyan
}

# Main setup
Write-Host ""
Write-Host "🔧 What would you like to set up?" -ForegroundColor Magenta
Write-Host "1) .env file for command-line usage" -ForegroundColor White
Write-Host "2) VS Code launch settings for debugging" -ForegroundColor White
Write-Host "3) Both" -ForegroundColor White
Write-Host ""
$choice = Read-Host "Enter your choice (1-3)"

switch ($choice) {
    "1" {
        Setup-EnvFile
    }
    "2" {
        Setup-LaunchSettings
    }
    "3" {
        Setup-EnvFile
        Setup-LaunchSettings
    }
    default {
        Write-Host "❌ Invalid choice" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "🔗 Next steps:" -ForegroundColor Cyan
Write-Host "1. Create a GitHub Personal Access Token at: https://github.com/settings/tokens" -ForegroundColor White
Write-Host "2. Enable 'models:read' permission" -ForegroundColor White
Write-Host "3. Add your token to the files you just created" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Test your setup:" -ForegroundColor Magenta
Write-Host "   .\run-with-env.ps1 ai 'what commands are available?'  # For .env" -ForegroundColor White
Write-Host "   Press F5 in VS Code                                   # For launch settings" -ForegroundColor White
Write-Host ""
Write-Host "✅ Setup complete!" -ForegroundColor Green
