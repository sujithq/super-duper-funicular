@echo off
REM Setup Conventional Commits for SolarScope CLI (Windows)

echo 🚀 Setting up Conventional Commits for SolarScope CLI
echo.

REM Check if we're in a git repository
if not exist ".git" (
    echo ❌ This script must be run from the root of a git repository
    exit /b 1
)

REM 1. Configure git commit template
echo 📝 Configuring git commit message template...
git config commit.template .gitmessage
echo ✅ Git commit template configured

REM 2. Set up commit message hook
echo 🔧 Setting up commit message validation hook...
git config core.hooksPath .githooks
echo ✅ Commit message hook configured

REM 3. Install optional tools (if Node.js is available)
where npm >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo 📦 Installing optional conventional commit tools...
    
    set /p install_global="Install commitizen globally? (y/n): "
    if /i "%install_global%"=="y" (
        npm install -g commitizen cz-conventional-changelog
        echo { "path": "cz-conventional-changelog" } > %USERPROFILE%\.czrc
        echo ✅ Commitizen installed globally
        echo Usage: git add . ^&^& cz
    ) else (
        npm install --save-dev commitizen cz-conventional-changelog
        echo { "path": "cz-conventional-changelog" } > .czrc
        echo ✅ Commitizen installed locally
        echo Usage: git add . ^&^& npx cz
    )
    
    set /p install_changelog="Install conventional-changelog-cli? (y/n): "
    if /i "%install_changelog%"=="y" (
        npm install -g conventional-changelog-cli
        echo ✅ Conventional changelog CLI installed
        echo Usage: conventional-changelog -p angular -i CHANGELOG.md -s
    )
) else (
    echo ⚠️  Node.js not found. Skipping optional tools installation.
    echo    To install commitizen and conventional-changelog-cli:
    echo    npm install -g commitizen cz-conventional-changelog conventional-changelog-cli
)

echo.
echo 🎉 Conventional Commits setup complete!
echo.
echo 📋 Next steps:
echo    1. Start using conventional commit format:
echo       feat(dashboard^): add dark mode theme
echo       fix(analyze^): resolve memory leak
echo.
echo    2. Use the commit template (already configured^):
echo       git commit
echo.
echo    3. Or use commitizen for guided commits:
echo       git add . ^&^& cz
echo.
echo    4. Test the validation hook:
echo       git commit -m "invalid message format"
echo.
echo 💡 See docs\CONVENTIONAL_COMMITS.md for detailed examples and guidelines
echo.
echo 🔄 Your commits will now be validated automatically and used for:
echo    • Automatic changelog generation
echo    • Semantic version detection
echo    • Release note creation
echo    • Better project history

pause
