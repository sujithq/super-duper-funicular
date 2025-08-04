# Quick setup script for SolarScope CLI environment (PowerShell)

Write-Host "🌞 SolarScope CLI Environment Setup" -ForegroundColor Yellow
Write-Host "===================================" -ForegroundColor Yellow

# Check if .env already exists
if (Test-Path ".env") {
    Write-Host "⚠️  .env file already exists" -ForegroundColor Yellow
    $overwrite = Read-Host "Do you want to overwrite it? (y/N)"
    if ($overwrite -ne "y" -and $overwrite -ne "Y") {
        Write-Host "❌ Setup cancelled" -ForegroundColor Red
        exit 0
    }
}

# Copy example file
Copy-Item .env.example .env
Write-Host "✅ Created .env file from template" -ForegroundColor Green

Write-Host ""
Write-Host "📝 Please edit the .env file and add your GitHub Personal Access Token:" -ForegroundColor Cyan
Write-Host "   1. Open .env in your favorite editor" -ForegroundColor White
Write-Host "   2. Replace 'your_github_token_here' with your actual token" -ForegroundColor White
Write-Host "   3. Save the file" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Create a GitHub token at: https://github.com/settings/tokens" -ForegroundColor Blue
Write-Host "   Make sure to enable 'models:read' permission" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Once configured, test with:" -ForegroundColor Magenta
Write-Host "   .\run-with-env.ps1 ai 'What commands are available?'" -ForegroundColor White
