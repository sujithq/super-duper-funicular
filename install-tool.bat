@echo off
setlocal enabledelayedexpansion

:: SolarScope CLI - Tool Installation Script (Batch Version)
:: Packages, uninstalls (if needed), and installs SolarScope CLI as a glob# Test the tool
echo 🔧 Testing tool execution...
solarscope version >nul 2>&1dotnet tool

set "USE_NUGET=false"

:: Parse command line arguments
:parse_args
if "%~1"=="-nuget" set "USE_NUGET=true" & shift & goto parse_args
if "%~1"=="--nuget" set "USE_NUGET=true" & shift & goto parse_args
if "%~1"=="/nuget" set "USE_NUGET=true" & shift & goto parse_args
if not "%~1"=="" shift & goto parse_args

echo.
echo 🌞 SolarScope CLI - Tool Installation Script 🌞
echo =================================================

if "%USE_NUGET%"=="true" (
    echo 📦 Installing from NuGet.org (latest published version)
) else (
    echo 🔧 Installing from local development version
)
echo.

:: Check if .NET is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: .NET SDK not found. Please install .NET 9.0 SDK or later.
    exit /b 1
)

:: Function to extract PackageVersion from project file
goto main

:extract_version
set "PROJECT_FILE=%~1"
set "EXTRACTED_VERSION="

if not exist "%PROJECT_FILE%" (
    echo ❌ Project file not found: %PROJECT_FILE%
    exit /b 1
)

:: Extract PackageVersion using findstr and more precise parsing
for /f "tokens=*" %%i in ('findstr "<PackageVersion>" "%PROJECT_FILE%"') do (
    set "LINE=%%i"
    call :parse_version_line
)

if "%EXTRACTED_VERSION%"=="" (
    echo ❌ PackageVersion not found in project file
    exit /b 1
)
exit /b 0

:parse_version_line
:: Remove everything before <PackageVersion>
set "LINE=%LINE:*<PackageVersion>=%"
:: Remove everything after </PackageVersion>
for /f "tokens=1 delims=<" %%j in ("%LINE%") do set "EXTRACTED_VERSION=%%j"
exit /b 0

:main

:: Check for existing installation and uninstall
echo 🔧 Checking for existing SolarScope installation...
set "EXISTING_TOOL="
for /f "delims=" %%i in ('dotnet tool list --global ^| findstr /i "solarscope" 2^>nul') do set "EXISTING_TOOL=%%i"
if not "!EXISTING_TOOL!"=="" (
    echo ⚠️  SolarScope tool is already installed globally
    echo Current installation: !EXISTING_TOOL!
    echo 🔧 Uninstalling existing version...
    
    :: Force uninstall - try both possible package names
    echo 🔧 Attempting to uninstall package: SolarScope
    dotnet tool uninstall --global SolarScope >nul 2>&1
    echo 🔧 Attempting to uninstall package: solarscope  
    dotnet tool uninstall --global solarscope >nul 2>&1
    
    :: Verify uninstallation
    set "STILL_EXISTS="
    for /f "delims=" %%i in ('dotnet tool list --global ^| findstr /i "solarscope" 2^>nul') do set "STILL_EXISTS=%%i"
    if not "!STILL_EXISTS!"=="" (
        echo ⚠️  Tool may still be installed after uninstall attempt
        echo Remaining: !STILL_EXISTS!
    ) else (
        echo ✅ Successfully uninstalled existing version
    )
    
    :: Clear NuGet cache for this package to ensure fresh installation
    echo 🔧 Clearing NuGet cache for SolarScope package...
    dotnet nuget locals all --clear >nul 2>&1
    echo ✅ NuGet cache cleared
) else (
    echo ✅ No existing installation found
)

:: Handle NuGet.org installation
if "%USE_NUGET%"=="true" (
    echo 🔧 Installing SolarScope CLI from NuGet.org...
    dotnet tool install --global SolarScope
    if errorlevel 1 (
        echo ❌ Tool installation from NuGet.org failed
        exit /b 1
    )
    echo ✅ SolarScope CLI installed successfully from NuGet.org!
    goto verify_installation
)

:: Local development installation
echo 🔧 Using local development version...

:: Extract base version from project file
echo 🔧 Extracting version from project file...
set "PROJECT_FILE=%~dp0src\SolarScope.csproj"
call :extract_version "%PROJECT_FILE%"
if errorlevel 1 (
    echo ❌ Failed to extract version from project file
    exit /b 1
)
set "BASE_VERSION=%EXTRACTED_VERSION%"
echo ✅ Found project version: %BASE_VERSION%

:: Generate a unique development version to force package refresh
:: Simple timestamp using current time (HH-MM-SS format)
set "TIMESTAMP=%time:~0,2%%time:~3,2%%time:~6,2%"
set "TIMESTAMP=%TIMESTAMP: =0%"
:: Add date in simple format
for /f "tokens=1-3 delims=/" %%a in ("%date%") do set "DATEPART=%%c%%a%%b"
set "DATEPART=%DATEPART: =%"
set "DEV_VERSION=%BASE_VERSION%-dev-%DATEPART%%TIMESTAMP%"
echo 🔧 Using development version: %DEV_VERSION%

:: Navigate to project directory
cd /d "%~dp0src"

:: Clean previous builds
echo 🔧 Cleaning previous build artifacts...
if exist "nupkg" rmdir /s /q "nupkg"
:: Run dotnet clean (ignore errors if it hangs or fails)
dotnet clean --configuration Release >nul 2>&1
echo ✅ Cleaned build artifacts

:: Build and package
echo 🔧 Building and packaging SolarScope CLI...
dotnet pack --configuration Release --output "./nupkg" -p:PackageVersion=%DEV_VERSION%
if errorlevel 1 (
    echo ❌ Package creation failed
    exit /b 1
)
echo ✅ Package created successfully

:: Find the generated package
for %%f in (nupkg\*.nupkg) do set "LATEST_PACKAGE=%%f"
echo ✅ Found package: %LATEST_PACKAGE%

:: Install the tool globally from local package
echo 🔧 Installing SolarScope CLI as global dotnet tool from local package...
echo Package: %LATEST_PACKAGE%
echo Version: %DEV_VERSION%
echo 🔧 Running: dotnet tool install --global --add-source ./nupkg --version %DEV_VERSION% SolarScope
dotnet tool install --global --add-source ./nupkg --version %DEV_VERSION% SolarScope
if errorlevel 1 (
    echo ❌ Tool installation failed
    exit /b 1
)

echo ✅ SolarScope CLI installed successfully from local development version!

:verify_installation
:: Verify installation
echo 🔧 Verifying installation...
set "VERIFY_OUTPUT="
for /f "delims=" %%i in ('dotnet tool list --global ^| findstr /i "solarscope" 2^>nul') do set "VERIFY_OUTPUT=%%i"
if not "!VERIFY_OUTPUT!"=="" (
    echo ✅ Installation verified: !VERIFY_OUTPUT!
    
    :: Additional verification - check the actual version
    for /f "tokens=2" %%v in ("!VERIFY_OUTPUT!") do set "INSTALLED_VERSION=%%v"
    if "%USE_NUGET%"=="true" (
        echo Installed NuGet.org version: !INSTALLED_VERSION!
    ) else (
        if "!INSTALLED_VERSION!"=="%DEV_VERSION%" (
            echo ✅ Correct development version installed: !INSTALLED_VERSION!
        ) else (
            echo ⚠️  Version mismatch! Expected: %DEV_VERSION%, Got: !INSTALLED_VERSION!
        )
    )
) else (
    echo ⚠️  Could not verify installation in global tool list
)

:: Test the tool
echo 🔧 Testing tool execution...
solarscope version >nul 2>&1
if not errorlevel 1 (
    echo ✅ Tool test successful!
    
    :: Test a quick command to verify functionality
    echo 🔧 Testing report command...
    solarscope report --help >nul 2>&1
    if not errorlevel 1 (
        echo ✅ Report command test successful!
    ) else (
        echo ⚠️  Report command test failed
    )
) else (
    echo ⚠️  Tool test failed
)

:: Success message
echo.
echo 🎉 Installation Complete! 🎉
echo =========================
echo.

if "%USE_NUGET%"=="true" (
    echo SolarScope CLI (NuGet.org version) is now available globally!
    echo.
    echo Note: This is the published version from NuGet.org
    echo Use install-tool.bat (without -nuget) for local development version
) else (
    echo SolarScope CLI (local development version) is now available globally!
    echo.
    echo Note: This is your local development version: %DEV_VERSION%
    echo Use install-tool.bat -nuget for the published version
    echo.
    echo Package location: %LATEST_PACKAGE%
    echo Development version: %DEV_VERSION%
)

echo.
echo Try these commands:
echo   solarscope --help
echo   solarscope dashboard
echo   solarscope analyze
echo   solarscope demo --theme solar
echo.

pause
