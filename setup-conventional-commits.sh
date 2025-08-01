#!/bin/bash
# Setup Conventional Commits for SolarScope CLI

echo "🚀 Setting up Conventional Commits for SolarScope CLI"
echo ""

# Check if we're in a git repository
if [ ! -d ".git" ]; then
    echo "❌ This script must be run from the root of a git repository"
    exit 1
fi

# 1. Configure git commit template
echo "📝 Configuring git commit message template..."
git config commit.template .gitmessage
echo "✅ Git commit template configured"

# 2. Set up commit message hook
echo "🔧 Setting up commit message validation hook..."
chmod +x .githooks/commit-msg

# Convert to Unix line endings if dos2unix is available (for WSL compatibility)
if command -v dos2unix &> /dev/null; then
    dos2unix .githooks/commit-msg 2>/dev/null || true
    echo "   ℹ️  Converted hook to Unix line endings for WSL compatibility"
fi

git config core.hooksPath .githooks
echo "✅ Commit message hook configured"

# 3. Install optional tools (if Node.js is available)
if command -v npm >/dev/null 2>&1; then
    echo "📦 Installing optional conventional commit tools..."
    
    # Check if running globally or locally
    read -p "Install commitizen globally? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        npm install -g commitizen cz-conventional-changelog
        echo '{ "path": "cz-conventional-changelog" }' > ~/.czrc
        echo "✅ Commitizen installed globally"
        echo "Usage: git add . && cz"
    else
        npm install --save-dev commitizen cz-conventional-changelog
        echo '{ "path": "cz-conventional-changelog" }' > .czrc
        echo "✅ Commitizen installed locally"
        echo "Usage: git add . && npx cz"
    fi
    
    # Ask about conventional-changelog-cli
    read -p "Install conventional-changelog-cli? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        npm install -g conventional-changelog-cli
        echo "✅ Conventional changelog CLI installed"
        echo "Usage: conventional-changelog -p angular -i CHANGELOG.md -s"
    fi
else
    echo "⚠️  Node.js not found. Skipping optional tools installation."
    echo "   To install commitizen and conventional-changelog-cli:"
    echo "   npm install -g commitizen cz-conventional-changelog conventional-changelog-cli"
fi

echo ""
echo "🎉 Conventional Commits setup complete!"
echo ""
echo "📋 Next steps:"
echo "   1. Start using conventional commit format:"
echo "      feat(dashboard): add dark mode theme"
echo "      fix(analyze): resolve memory leak"
echo ""
echo "   2. Use the commit template (already configured):"
echo "      git commit"
echo ""
echo "   3. Or use commitizen for guided commits:"
echo "      git add . && cz"
echo ""
echo "   4. Test the validation hook:"
echo "      git commit -m 'invalid message format'"
echo ""
echo "💡 See docs/CONVENTIONAL_COMMITS.md for detailed examples and guidelines"
echo ""
echo "🔄 Your commits will now be validated automatically and used for:"
echo "   • Automatic changelog generation"
echo "   • Semantic version detection"
echo "   • Release note creation"
echo "   • Better project history"
echo ""
echo "📝 Note: .gitattributes file ensures consistent line endings across platforms"
