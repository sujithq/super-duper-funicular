#!/bin/bash

# SolarScope CLI - Tool Installation Script (Bash Version)
# Packages, uninstalls (if needed), and installs SolarScope CLI as a global dotnet tool

set -e

# Parse command line arguments
USE_NUGET=false
while [[ $# -gt 0 ]]; do
    case $1 in
        -n|--nuget)
            USE_NUGET=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--nuget]"
            echo "  --nuget    Install from NuGet.org instead of local development version"
            exit 1
            ;;
    esac
done

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo
echo -e "${YELLOW}🌞 SolarScope CLI - Tool Installation Script 🌞${NC}"
echo -e "${YELLOW}=================================================${NC}"

if [ "$USE_NUGET" = true ]; then
    echo -e "${BLUE}📦 Installing from NuGet.org (latest published version)${NC}"
else
    echo -e "${BLUE}🔧 Installing from local development version${NC}"
fi
echo

# Function to extract PackageVersion from project file
extract_package_version() {
    local project_file="$1"
    
    if [ ! -f "$project_file" ]; then
        echo "Project file not found: $project_file" >&2
        return 1
    fi
    
    # Extract PackageVersion using grep and sed
    local version=$(grep -o '<PackageVersion>[^<]*</PackageVersion>' "$project_file" | sed 's/<PackageVersion>\(.*\)<\/PackageVersion>/\1/')
    
    if [ -z "$version" ]; then
        echo "PackageVersion not found in project file" >&2
        return 1
    fi
    
    echo "$version"
}

# Check if .NET is available
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ Error: .NET SDK not found. Please install .NET 9.0 SDK or later.${NC}"
    exit 1
fi

# Check for existing installation and uninstall
echo -e "${BLUE}🔧 Checking for existing SolarScope installation...${NC}"
if dotnet tool list --global | grep -i "solarscope" > /dev/null 2>&1; then
    EXISTING_TOOL=$(dotnet tool list --global | grep -i "solarscope")
    echo -e "${YELLOW}⚠️  SolarScope tool is already installed globally${NC}"
    echo -e "${YELLOW}Current installation: $EXISTING_TOOL${NC}"
    echo -e "${BLUE}🔧 Uninstalling existing version...${NC}"
    
    # Force uninstall - try both possible package names
    for package_name in "SolarScope" "solarscope"; do
        echo -e "${BLUE}🔧 Attempting to uninstall package: $package_name${NC}"
        dotnet tool uninstall --global "$package_name" > /dev/null 2>&1 || true
    done
    
    # Verify uninstallation
    if dotnet tool list --global | grep -i "solarscope" > /dev/null 2>&1; then
        STILL_EXISTS=$(dotnet tool list --global | grep -i "solarscope")
        echo -e "${YELLOW}⚠️  Tool may still be installed after uninstall attempt${NC}"
        echo -e "${YELLOW}Remaining: $STILL_EXISTS${NC}"
    else
        echo -e "${GREEN}✅ Successfully uninstalled existing version${NC}"
    fi
    
    # Clear NuGet cache for this package to ensure fresh installation
    echo -e "${BLUE}🔧 Clearing NuGet cache for SolarScope package...${NC}"
    dotnet nuget locals all --clear > /dev/null 2>&1
    echo -e "${GREEN}✅ NuGet cache cleared${NC}"
else
    echo -e "${GREEN}✅ No existing installation found${NC}"
fi

# Handle NuGet.org installation
if [ "$USE_NUGET" = true ]; then
    echo -e "${BLUE}🔧 Installing SolarScope CLI from NuGet.org...${NC}"
    dotnet tool install --global SolarScope
    echo -e "${GREEN}✅ SolarScope CLI installed successfully from NuGet.org!${NC}"
else
    # Local development installation
    echo -e "${BLUE}🔧 Using local development version...${NC}"
    
    # Get script directory and project file path
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    PROJECT_FILE="$SCRIPT_DIR/src/SolarScope.csproj"
    
    # Extract base version from project file
    echo -e "${BLUE}🔧 Extracting version from project file...${NC}"
    BASE_VERSION=$(extract_package_version "$PROJECT_FILE")
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to extract version from project file${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ Found project version: $BASE_VERSION${NC}"
    
    # Generate a unique development version to force package refresh
    DEV_VERSION="$BASE_VERSION-dev-$(date +%Y%m%d%H%M%S)"
    echo -e "${BLUE}🔧 Using development version: $DEV_VERSION${NC}"
    
    # Navigate to project directory
    PROJECT_DIR="$SCRIPT_DIR/src"
    
    cd "$PROJECT_DIR"
    
    # Clean previous builds
    echo -e "${BLUE}🔧 Cleaning previous build artifacts...${NC}"
    rm -rf nupkg
    
    # Skip dotnet clean to avoid hanging issues in some environments
    echo -e "${GREEN}✅ Cleaned build artifacts${NC}"
    
    # Build and package
    echo -e "${BLUE}🔧 Building and packaging SolarScope CLI...${NC}"
    dotnet pack --configuration Release --output "./nupkg" -p:PackageVersion="$DEV_VERSION"
    echo -e "${GREEN}✅ Package created successfully${NC}"
    
    # Find the generated package
    LATEST_PACKAGE=$(ls -t nupkg/*.nupkg | head -n1)
    echo -e "${GREEN}✅ Found package: $(basename "$LATEST_PACKAGE")${NC}"
    
    # Install the tool globally from local package
    echo -e "${BLUE}🔧 Installing SolarScope CLI as global dotnet tool from local package...${NC}"
    echo -e "${BLUE}Package: $(basename "$LATEST_PACKAGE")${NC}"
    echo -e "${BLUE}Version: $DEV_VERSION${NC}"
    echo -e "${BLUE}🔧 Running: dotnet tool install --global --add-source ./nupkg --version $DEV_VERSION SolarScope${NC}"
    dotnet tool install --global --add-source ./nupkg --version "$DEV_VERSION" SolarScope
    echo -e "${GREEN}✅ SolarScope CLI installed successfully from local development version!${NC}"
fi

# Verify installation
echo -e "${BLUE}🔧 Verifying installation...${NC}"
VERIFY_OUTPUT=$(dotnet tool list --global | grep -i "solarscope")
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Installation verified: $VERIFY_OUTPUT${NC}"
    
    # Additional verification - check the actual version
    INSTALLED_VERSION=$(echo "$VERIFY_OUTPUT" | awk '{print $2}')
    if [ "$USE_NUGET" = true ]; then
        echo -e "${BLUE}Installed NuGet.org version: $INSTALLED_VERSION${NC}"
    else
        if [ "$INSTALLED_VERSION" = "$DEV_VERSION" ]; then
            echo -e "${GREEN}✅ Correct development version installed: $INSTALLED_VERSION${NC}"
        else
            echo -e "${YELLOW}⚠️  Version mismatch! Expected: $DEV_VERSION, Got: $INSTALLED_VERSION${NC}"
        fi
    fi
else
    echo -e "${YELLOW}⚠️  Could not verify installation in global tool list${NC}"
fi

# Test the tool
echo -e "${BLUE}🔧 Testing tool execution...${NC}"
VERSION_OUTPUT=$(solarscope version 2>&1)
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Tool test successful!${NC}"
    echo -e "${BLUE}Version output: $VERSION_OUTPUT${NC}"
    
    # Test a quick command to verify functionality
    echo -e "${BLUE}🔧 Testing report command...${NC}"
    if solarscope report --help > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Report command test successful!${NC}"
    else
        echo -e "${YELLOW}⚠️  Report command test failed${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  Tool test failed${NC}"
    echo -e "${RED}Output: $VERSION_OUTPUT${NC}"
fi

# Success message
echo
echo -e "${GREEN}🎉 Installation Complete! 🎉${NC}"
echo -e "${GREEN}=========================${NC}"
echo

if [ "$USE_NUGET" = true ]; then
    echo -e "${GREEN}SolarScope CLI (NuGet.org version) is now available globally!${NC}"
    echo
    echo -e "${YELLOW}Note: This is the published version from NuGet.org${NC}"
    echo -e "${YELLOW}Use ./install-tool.sh (without --nuget) for local development version${NC}"
else
    echo -e "${GREEN}SolarScope CLI (local development version) is now available globally!${NC}"
    echo
    echo -e "${YELLOW}Note: This is your local development version: $DEV_VERSION${NC}"
    echo -e "${YELLOW}Use ./install-tool.sh --nuget for the published version${NC}"
    echo
    echo -e "${BLUE}Package location: $LATEST_PACKAGE${NC}"
    echo -e "${BLUE}Development version: $DEV_VERSION${NC}"
fi

echo
echo -e "${BLUE}Try these commands:${NC}"
echo "  solarscope --help"
echo "  solarscope dashboard"
echo "  solarscope analyze"
echo "  solarscope demo --theme solar"
echo
