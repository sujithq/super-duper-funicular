# Load environment variables from .env file for PowerShell

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "❌ .env file not found!" -ForegroundColor Red
    Write-Host "💡 Copy .env.example to .env and fill in your GitHub token" -ForegroundColor Yellow
    Write-Host "   Copy-Item .env.example .env" -ForegroundColor Cyan
    exit 1
}

# Load environment variables from .env file
Write-Host "🔧 Loading environment variables from .env..." -ForegroundColor Blue

Get-Content ".env" | Where-Object { $_ -match "^[^#].*=" } | ForEach-Object {
    $name, $value = $_.Split('=', 2)
    $name = $name.Trim()
    $value = $value.Trim()
    
    # Remove quotes if present
    if ($value.StartsWith('"') -and $value.EndsWith('"')) {
        $value = $value.Substring(1, $value.Length - 2)
    }
    
    [Environment]::SetEnvironmentVariable($name, $value, "Process")
    Write-Host "   Set $name" -ForegroundColor Green
}

# Verify GITHUB_TOKEN is set
$token = [Environment]::GetEnvironmentVariable("GITHUB_TOKEN")
if ([string]::IsNullOrEmpty($token) -or $token -eq "your_github_token_here") {
    Write-Host "❌ GITHUB_TOKEN not properly set in .env file!" -ForegroundColor Red
    Write-Host "💡 Edit .env and set your GitHub Personal Access Token" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Environment variables loaded successfully!" -ForegroundColor Green
Write-Host "🤖 You can now use: solarscope ai 'your question here'" -ForegroundColor Cyan

# Run the command if arguments provided
if ($args.Count -gt 0) {
    $command = $args -join ' '
    Write-Host "🚀 Running: solarscope $command" -ForegroundColor Magenta
    Set-Location src
    dotnet run -- $args
}
