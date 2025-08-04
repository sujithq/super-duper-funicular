# 🔒 Security Guidelines for SolarScope CLI Development

## Environment Variables and Sensitive Data

### ❌ **Never Commit These Files:**
- `.env` - Contains actual tokens and secrets
- `launchSettings.local.json` - VS Code settings with tokens
- Any file with real API keys or tokens

### ✅ **Safe to Commit:**
- `.env.example` - Template with placeholder values
- `launchSettings.json` - Template without sensitive data
- Setup scripts that help developers configure local environment

## Development Setup

### For New Contributors

1. **Clone the repository**
2. **Run the development setup script:**
   ```bash
   # Creates secure local files for development
   ./setup-dev.sh              # Bash/WSL/Linux/macOS
   .\setup-dev.ps1             # PowerShell/Windows
   ```
3. **Add your GitHub token** to the created files
4. **Start developing** with secure environment

### File Structure

```
📁 Project Root
├── .env.example                    ✅ Committed (template)
├── .env                           ❌ Not committed (your local config)
├── setup-dev.sh                   ✅ Committed (setup script)
├── run-with-env.sh                ✅ Committed (helper script)
└── src/Properties/
    ├── launchSettings.json         ✅ Committed (template)
    └── launchSettings.local.json   ❌ Not committed (your local config)
```

## VS Code Debugging

The `launchSettings.local.json` file allows you to:
- Set environment variables for debugging
- Configure different launch profiles
- Test AI commands directly in VS Code with F5

## Security Best Practices

1. **Use local files** for sensitive data (`*.local.json`, `.env`)
2. **Check .gitignore** before committing
3. **Use setup scripts** to help other developers
4. **Review commits** to ensure no secrets are included
5. **Use environment variables** in CI/CD pipelines

## Testing AI Features

```bash
# Command line (uses .env)
./run-with-env.sh ai "what commands are available?"

# VS Code debugging (uses launchSettings.local.json)
Press F5 → Select profile → Debug starts with environment
```

## Token Permissions

Your GitHub Personal Access Token needs:
- ✅ `models:read` permission (for GitHub Models API)
- ❌ No other permissions required

Create at: https://github.com/settings/tokens
