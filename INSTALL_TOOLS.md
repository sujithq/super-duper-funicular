# Tool Installation Scripts

SolarScope includes several automated installation scripts to package and install the CLI as a global .NET tool. **By default, all scripts install the local development version** to ensure consistency with the `run-with-env` scripts.

## Available Scripts

### PowerShell Script (Recommended for Windows)

```powershell
# Install local development version (default)
.\install-tool.ps1

# Install from NuGet.org
.\install-tool.ps1 -UseNuGet
```

**Features:**
- ✅ Cross-platform PowerShell support
- ✅ Colored output and progress indicators  
- ✅ Verbose mode for debugging (`-Verbose`)
- ✅ Optional clean mode (`-Clean:$false`)
- ✅ Choose between local dev version (default) or NuGet.org (`-UseNuGet`)
- ✅ Comprehensive error handling
- ✅ Installation verification

**Usage Examples:**

```powershell
# Standard installation (local development version)
.\install-tool.ps1

# Install from NuGet.org instead
.\install-tool.ps1 -UseNuGet

# Skip cleaning, enable verbose output, use local version
.\install-tool.ps1 -Clean:$false -Verbose

# Get help
Get-Help .\install-tool.ps1 -Detailed
```

### Batch Script (Windows)

```cmd
# Install local development version (default)
install-tool.bat

# Install from NuGet.org
install-tool.bat -nuget
```

Simple Windows batch file with NuGet.org option.

### Shell Script (Linux/macOS)

```bash
# Install local development version (default)
./install-tool.sh

# Install from NuGet.org  
./install-tool.sh --nuget
```

Cross-platform bash script with colored output and NuGet.org option.

## What the Scripts Do

1. **Clean** - Remove previous build artifacts (optional, local dev only)
2. **Package** - Build and create NuGet package (`dotnet pack`, local dev only)  
3. **Uninstall** - Remove existing global tool installation if present
4. **Install** - Install tool globally from local package (default) or NuGet.org (with flags)
5. **Verify** - Test installation and provide usage examples

## Installation Modes

### Local Development Version (Default)
- ✅ **Recommended for development** - matches your current code
- ✅ **Consistent with `run-with-env.ps1`** - same behavior
- ✅ Includes your latest changes and BarChart formatting fixes
- ✅ Perfect for testing and development

### NuGet.org Version (Optional)
- ✅ **Stable published version** - thoroughly tested
- ⚠️ May not include your latest local changes
- ✅ Good for production use or clean environment testing

## Manual Installation

If you prefer manual control, use these commands:

```bash
# Navigate to project
cd src

# Create package
dotnet pack --configuration Release --output "./nupkg"

# Install globally (uninstall first if already installed)
dotnet tool uninstall --global SolarScope
dotnet tool install --global --add-source ./nupkg SolarScope
```

## After Installation

Once installed, SolarScope is available globally:

```bash
solarscope --help
solarscope dashboard
solarscope analyze --year 2025
solarscope demo --theme solar
```

## Troubleshooting

- **Permission errors**: Run as administrator or with `sudo`
- **Tool already installed**: Use `dotnet tool uninstall --global SolarScope` first
- **Package not found**: Ensure the `nupkg` directory exists and contains the package
- **.NET version**: Requires .NET 9.0 SDK or later
