#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packages, uninstalls (if needed), and installs SolarScope CLI as a global dotnet tool.

.DESCRIPTION
    This script automates the process of:
    1. Cleaning previous builds
    2. Creating a NuGet package 
    3. Uninstalling existing global tool installation (if present)
    4. Installing the tool globally from the local package (default) or NuGet.org

.PARAMETER Clean
    Whether to clean build artifacts before packaging (default: true)

.PARAMETER UseNuGet
    Install from NuGet.org instead of local development version (default: false)

.EXAMPLE
    .\install-tool.ps1
    Installs the local development version
    
.EXAMPLE
    .\install-tool.ps1 -UseNuGet
    Installs the latest version from NuGet.org
    
.EXAMPLE
    .\install-tool.ps1 -Clean:$false -Verbose
    Installs local version without cleaning, with verbose output

.NOTES
    Requires .NET 9.0 SDK or later
    The tool will be available globally as 'solarscope' command
    Default behavior: Install local development version for consistency with run-with-env scripts
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [bool]$Clean = $true,
    
    [Parameter(Mandatory = $false)]
    [switch]$UseNuGet
)

# Set error handling
$ErrorActionPreference = "Stop"

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColoredOutput {
    param(
        [string]$Message,
        [string]$Color = $Reset
    )
    Write-Host "${Color}${Message}${Reset}"
}

function Write-Step {
    param([string]$Message)
    Write-ColoredOutput "🔧 $Message" $Blue
}

function Write-Success {
    param([string]$Message)
    Write-ColoredOutput "✅ $Message" $Green
}

function Write-Warning {
    param([string]$Message)
    Write-ColoredOutput "⚠️  $Message" $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-ColoredOutput "❌ $Message" $Red
}

function Get-PackageVersionFromProject {
    param([string]$ProjectFile)
    
    try {
        # Read and parse the project file as XML
        [xml]$ProjectXml = Get-Content $ProjectFile
        
        # Find the PackageVersion element
        $PackageVersion = $ProjectXml.Project.PropertyGroup.PackageVersion
        
        if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
            throw "PackageVersion not found in project file"
        }
        
        return $PackageVersion.Trim()
    }
    catch {
        throw "Failed to extract PackageVersion from project file: $($_.Exception.Message)"
    }
}

try {
    # Get script directory and project paths
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $ProjectDir = Join-Path $ScriptDir "src"
    $ProjectFile = Join-Path $ProjectDir "SolarScope.csproj"
    $NupkgDir = Join-Path $ProjectDir "nupkg"
    
    # Validate project file exists
    if (-not (Test-Path $ProjectFile)) {
        throw "Project file not found: $ProjectFile"
    }
    
    Write-ColoredOutput "`n🌞 SolarScope CLI - Tool Installation Script 🌞" $Yellow
    Write-ColoredOutput "=================================================" $Yellow
    
    if ($UseNuGet) {
        Write-ColoredOutput "📦 Installing from NuGet.org (latest published version)" $Blue
    } else {
        Write-ColoredOutput "🔧 Installing from local development version" $Blue
    }
    Write-Host ""
    
    # Step 1: Check for existing installation first
    Write-Step "Checking for existing SolarScope installation..."
    
    $ExistingTool = dotnet tool list --global | Select-String "solarscope"
    
    if ($ExistingTool) {
        Write-Warning "SolarScope tool is already installed globally"
        Write-ColoredOutput "Current installation: $($ExistingTool.Line.Trim())" $Yellow
        Write-Step "Uninstalling existing version..."
        
        # Force uninstall - try both possible package names
        @("SolarScope", "solarscope") | ForEach-Object {
            $packageName = $_
            Write-Step "Attempting to uninstall package: $packageName"
            dotnet tool uninstall --global $packageName 2>&1 | Out-Null
        }
        
        # Verify uninstallation
        $StillExists = dotnet tool list --global | Select-String "solarscope"
        if ($StillExists) {
            Write-Warning "Tool may still be installed after uninstall attempt"
            Write-ColoredOutput "Remaining: $($StillExists.Line.Trim())" $Yellow
        } else {
            Write-Success "Successfully uninstalled existing version"
        }
        
        # Clear NuGet cache for this package to ensure fresh installation
        Write-Step "Clearing NuGet cache for SolarScope package..."
        dotnet nuget locals all --clear 2>&1 | Out-Null
        Write-Success "NuGet cache cleared"
    } else {
        Write-Success "No existing installation found"
    }
    
    # Handle NuGet.org installation
    if ($UseNuGet) {
        Write-Step "Installing SolarScope CLI from NuGet.org..."
        
        dotnet tool install --global SolarScope
        
        if ($LASTEXITCODE -ne 0) {
            throw "Tool installation from NuGet.org failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "SolarScope CLI installed successfully from NuGet.org!"
    }
    else {
        # Step 2: Local development installation
        # Clean if requested
        if ($Clean) {
            Write-Step "Cleaning previous build artifacts..."
            
            if (Test-Path $NupkgDir) {
                Remove-Item $NupkgDir -Recurse -Force
                Write-Success "Cleaned nupkg directory"
            }
            
            Push-Location $ProjectDir
            try {
                # Try dotnet clean with timeout handling
                try {
                    dotnet clean --configuration Release | Out-Null
                    Write-Success "Cleaned build artifacts"
                } catch {
                    Write-Warning "Clean command failed, continuing without clean: $($_.Exception.Message)"
                }
            }
            finally {
                Pop-Location
            }
        }
        
        # Extract the base version from the project file
        Write-Step "Extracting version from project file..."
        $BaseVersion = Get-PackageVersionFromProject $ProjectFile
        Write-Success "Found project version: $BaseVersion"
        
        # Generate a unique development version to force package refresh
        $DevVersion = "$BaseVersion-dev-$(Get-Date -Format 'yyyyMMddHHmmss')"
        Write-Step "Using development version: $DevVersion"
        
        # Step 3: Build and package
        Write-Step "Building and packaging SolarScope CLI..."
        
        Push-Location $ProjectDir
        try {
            $packArgs = @(
                "pack"
                "--configuration", "Release"
                "--output", "./nupkg"
                "-p:PackageVersion=$DevVersion"
            )
            
            if ($VerbosePreference -eq 'Continue') {
                $packArgs += "--verbosity", "detailed"
            }
            
            & dotnet @packArgs
            
            if ($LASTEXITCODE -ne 0) {
                throw "Package creation failed with exit code $LASTEXITCODE"
            }
            
            Write-Success "Package created successfully"
        }
        finally {
            Pop-Location
        }
        
        # Step 4: Find the generated package
        $PackageFiles = Get-ChildItem -Path $NupkgDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending
        
        if ($PackageFiles.Count -eq 0) {
            throw "No NuGet package found in $NupkgDir"
        }
        
        $LatestPackage = $PackageFiles[0]
        Write-Success "Found package: $($LatestPackage.Name)"
        
        # Step 5: Install the tool globally from local package
        Write-Step "Installing SolarScope CLI as global dotnet tool from local package..."
        Write-ColoredOutput "Package: $($LatestPackage.Name)" $Blue
        Write-ColoredOutput "Version: $DevVersion" $Blue
        
        $installArgs = @(
            "tool", "install"
            "--global"
            "--add-source", $NupkgDir
            "--version", $DevVersion
            "SolarScope"
        )
        
        Write-Step "Running: dotnet $($installArgs -join ' ')"
        & dotnet @installArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Tool installation failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "SolarScope CLI installed successfully from local development version!"
    }
    
    # Step 6: Verify installation
    Write-Step "Verifying installation..."
    
    $VerifyOutput = dotnet tool list --global | Select-String "solarscope"
    
    if ($VerifyOutput) {
        Write-Success "Installation verified: $($VerifyOutput.Line.Trim())"
        
        # Additional verification - check the actual version
        $InstalledVersion = ($VerifyOutput.Line -split '\s+')[1]
        if ($UseNuGet) {
            Write-ColoredOutput "Installed NuGet.org version: $InstalledVersion" $Blue
        } else {
            if ($InstalledVersion -eq $DevVersion) {
                Write-Success "✅ Correct development version installed: $InstalledVersion"
            } else {
                Write-Warning "⚠️ Version mismatch! Expected: $DevVersion, Got: $InstalledVersion"
            }
        }
    } else {
        Write-Warning "Could not verify installation in global tool list"
    }
    
    # Step 7: Test the tool
    Write-Step "Testing tool execution..."
    
    try {
        $VersionOutput = & solarscope version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Tool test successful!"
            Write-ColoredOutput "Version output: $VersionOutput" $Blue
            
            # Test a quick command to verify functionality
            Write-Step "Testing report command..."
            $ReportTest = & solarscope report --help 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Report command test successful!"
            } else {
                Write-Warning "Report command test failed (exit code: $LASTEXITCODE)"
            }
        } else {
            Write-Warning "Tool test returned exit code $LASTEXITCODE"
            Write-ColoredOutput "Output: $VersionOutput" $Red
        }
    }
    catch {
        Write-Warning "Could not test tool execution: $($_.Exception.Message)"
    }
    
    # Success summary
    Write-ColoredOutput "`n🎉 Installation Complete! 🎉" $Green
    Write-ColoredOutput "=========================" $Green
    Write-Host ""
    
    if ($UseNuGet) {
        Write-ColoredOutput "SolarScope CLI (NuGet.org version) is now available globally!" $Green
        Write-Host ""
        Write-ColoredOutput "Note: This is the published version from NuGet.org" $Yellow
        Write-ColoredOutput "Use .\install-tool.ps1 (without -UseNuGet) for local development version" $Yellow
    } else {
        Write-ColoredOutput "SolarScope CLI (local development version) is now available globally!" $Green
        Write-Host ""
        Write-ColoredOutput "Note: This is your local development version: $DevVersion" $Yellow
        Write-ColoredOutput "Use .\install-tool.ps1 -UseNuGet for the published version" $Yellow
        Write-Host ""
        Write-ColoredOutput "Package location: $($LatestPackage.FullName)" $Blue
        Write-ColoredOutput "Development version: $DevVersion" $Blue
    }
    
    Write-Host ""
    Write-ColoredOutput "Try these commands:" $Blue
    Write-Host "  solarscope --help"
    Write-Host "  solarscope dashboard"
    Write-Host "  solarscope analyze"
    Write-Host "  solarscope demo --theme solar"
    Write-Host ""
    
}
catch {
    Write-Error "Installation failed: $($_.Exception.Message)"
    Write-Host ""
    Write-ColoredOutput "Troubleshooting:" $Yellow
    Write-Host "1. Ensure .NET 9.0 SDK is installed"
    Write-Host "2. Check that you have permissions to install global tools"
    Write-Host "3. Try running with -Verbose for more details"
    if ($UseNuGet) {
        Write-Host "4. Check your internet connection for NuGet.org access"
        Write-Host "5. Try: dotnet tool install --global SolarScope"
    } else {
        Write-Host "4. Manually run: dotnet tool install --global --add-source ./src/nupkg SolarScope"
        Write-Host "5. Try with -UseNuGet flag to install from NuGet.org instead"
    }
    
    exit 1
}
