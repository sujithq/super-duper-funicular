@echo off
rem Update Project Summary Script (Windows)
rem This script helps maintain the PROJECT_SUMMARY.md file with current status

echo 🔄 Updating PROJECT_SUMMARY.md...

rem Update the last updated date
for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (
    set month=%%a
    set day=%%b
    set year=%%c
)

rem Convert month number to name (simplified)
if "%month%"=="01" set monthname=January
if "%month%"=="02" set monthname=February
if "%month%"=="03" set monthname=March
if "%month%"=="04" set monthname=April
if "%month%"=="05" set monthname=May
if "%month%"=="06" set monthname=June
if "%month%"=="07" set monthname=July
if "%month%"=="08" set monthname=August
if "%month%"=="09" set monthname=September
if "%month%"=="10" set monthname=October
if "%month%"=="11" set monthname=November
if "%month%"=="12" set monthname=December

set today=%monthname% %day%, %year%
echo 📅 Current date: %today%

rem Show current file stats if summary exists
if exist "PROJECT_SUMMARY.md" (
    for %%i in (PROJECT_SUMMARY.md) do (
        echo 📄 Summary file size: %%~zi bytes
    )
)

rem Check if git is available
git --version >nul 2>&1
if %errorlevel%==0 (
    echo 📊 Checking git status...
    git log -1 --oneline 2>nul
    if %errorlevel%==0 (
        echo ✅ Git repository detected
    )
)

echo.
echo ✅ Project summary check complete!
echo.
echo 💡 To manually update the summary:
echo    - Edit PROJECT_SUMMARY.md
echo    - Update the "Last Updated" date at the top
echo    - Update status sections with new progress
echo    - Add new features to the feature lists
echo    - Update completion percentages
echo    - Add lessons learned or insights
echo    - Update future roadmap items
echo.
pause
