# 🚀 Conventional Commits Guide for SolarScope CLI

## Overview

Conventional Commits is a specification for adding human and machine-readable meaning to commit messages. This enables automatic changelog generation, semantic versioning, and better project history.

## 📋 Commit Message Format

```text
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Examples

```bash
feat: add weather correlation analysis command
fix(dashboard): resolve crash when data file is empty
docs: update installation guide with troubleshooting
feat!: redesign command structure (breaking change)
```

## 🎯 Commit Types

### **feat** - New Features

```bash
feat: add dark mode theme for dashboard
feat(analyze): add correlation analysis type
feat(demo): add matrix animation theme
```

### **fix** - Bug Fixes

```bash
fix: resolve memory leak in anomaly detection
fix(dashboard): handle empty data gracefully
fix(weather): correct temperature unit conversion
```

### **docs** - Documentation

```bash
docs: add API documentation for services
docs(readme): update installation instructions
docs: create troubleshooting guide
```

### **style** - Code Style (no logic changes)

```bash
style: format code with prettier
style(models): add missing whitespace
style: fix indentation in command files
```

### **refactor** - Code Refactoring

```bash
refactor: extract common chart rendering logic
refactor(services): simplify data loading workflow
refactor: optimize anomaly detection algorithm
```

### **test** - Adding Tests

```bash
test: add unit tests for weather service
test(dashboard): add integration tests
test: increase coverage for data models
```

### **chore** - Maintenance

```bash
chore: update dependencies to latest versions
chore(deps): bump Spectre.Console to 0.50.1
chore: configure CI/CD pipeline
```

### **perf** - Performance Improvements

```bash
perf: optimize large dataset processing
perf(dashboard): reduce rendering time by 40%
perf: implement data caching for repeated queries
```

### **ci** - CI/CD Changes

```bash
ci: add automated testing workflow
ci: configure NuGet publishing pipeline
ci(release): improve version bump automation
```

### **build** - Build System Changes

```bash
build: update project to .NET 9.0
build: configure cross-platform packaging
build(deps): add new package references
```

## 🔄 Breaking Changes

Use `!` after type or add `BREAKING CHANGE:` in footer:

```bash
feat!: redesign command line interface
# or
feat: redesign command line interface

BREAKING CHANGE: The --data option is now required for all commands
```

## 📊 Scopes for SolarScope CLI

### **Common Scopes**

- `dashboard` - Dashboard command and UI
- `analyze` - Analysis commands and algorithms
- `anomalies` - Anomaly detection features
- `weather` - Weather-related functionality
- `data` - Data loading and processing
- `models` - Data models and types
- `services` - Business logic services
- `cli` - Command-line interface
- `demo` - Demo and animation features
- `docs` - Documentation
- `ci` - Continuous integration
- `deps` - Dependencies

### **Examples with Scopes**

```bash
feat(dashboard): add real-time data refresh
fix(analyze): correct production calculation
docs(api): add service layer documentation
perf(data): optimize JSON parsing performance
test(models): add validation tests
```

## 🤖 Automated Changelog Generation

With conventional commits, we can automatically generate changelog entries:

### **Commit → Changelog Mapping**

```bash
feat: add weather correlation analysis
→ ### Added
  - Weather correlation analysis

fix(dashboard): resolve crash on empty data
→ ### Fixed
  - Dashboard crash when data file is empty

feat!: redesign command structure
→ ### Changed
  - **BREAKING**: Redesigned command structure

perf: optimize anomaly detection by 40%
→ ### Changed
  - Improved anomaly detection performance by 40%
```

## 🛠️ Tooling and Validation

### **1. Commitizen Setup**

Install commitizen for interactive commit creation:

```bash
npm install -g commitizen cz-conventional-changelog
echo '{ "path": "cz-conventional-changelog" }' > ~/.czrc
```

Usage:

```bash
git add .
cz  # Interactive commit creation
```

### **2. Commit Message Validation**

Create `.gitmessage` template:

```txt
# <type>[optional scope]: <description>
# 
# [optional body]
#
# [optional footer(s)]
#
# Types: feat, fix, docs, style, refactor, test, chore, perf, ci, build
# Scopes: dashboard, analyze, anomalies, weather, data, models, services, cli, demo
# 
# Examples:
# feat(dashboard): add dark mode theme
# fix: resolve memory leak in data processing
# docs: update API documentation
# feat!: redesign command structure (breaking change)
```

### **3. Pre-commit Hook**

Create `.githooks/commit-msg`:

```bash
#!/bin/sh
# Validate conventional commit format

commit_regex='^(feat|fix|docs|style|refactor|test|chore|perf|ci|build)(\(.+\))?(!)?: .{1,50}'

if ! grep -qE "$commit_regex" "$1"; then
    echo "❌ Invalid commit message format!"
    echo "Format: <type>[scope]: <description>"
    echo "Example: feat(dashboard): add dark mode theme"
    echo ""
    echo "Types: feat, fix, docs, style, refactor, test, chore, perf, ci, build"
    exit 1
fi

echo "✅ Commit message format is valid"
```

## 🔄 GitHub Workflow Integration

### **Enhanced Release Workflow**

Update the release workflow to use conventional commits for automatic changelog generation:

```yaml
- name: Generate changelog from conventional commits
  run: |
    # Install conventional-changelog-cli
    npm install -g conventional-changelog-cli
    
    # Generate changelog for current version
    conventional-changelog -p angular -i temp_changelog.md -s
    
    # Update CHANGELOG.md with new entries
    if [ -s temp_changelog.md ]; then
      # Replace [Unreleased] section with generated content
      sed -i '/## \[Unreleased\]/r temp_changelog.md' CHANGELOG.md
      sed -i '/## \[Unreleased\]/d' CHANGELOG.md
    fi
```

### **Auto-labeling PRs**

Based on conventional commits, automatically label PRs:

```yaml
- name: Label PR based on conventional commits
  run: |
    COMMITS=$(git log --pretty=format:"%s" ${{ github.event.before }}..${{ github.sha }})
    
    if echo "$COMMITS" | grep -q "^feat"; then
      gh pr edit ${{ github.event.number }} --add-label "enhancement"
    fi
    
    if echo "$COMMITS" | grep -q "^fix"; then
      gh pr edit ${{ github.event.number }} --add-label "bug"
    fi
    
    if echo "$COMMITS" | grep -q "!:"; then
      gh pr edit ${{ github.event.number }} --add-label "breaking-change"
    fi
```

## 📈 Semantic Version Detection

Automatically determine version bump based on commits:

```bash
#!/bin/bash
# Determine semantic version bump from conventional commits

COMMITS=$(git log --pretty=format:"%s" v1.0.0..HEAD)

# Check for breaking changes
if echo "$COMMITS" | grep -q "!:" || echo "$COMMITS" | grep -q "BREAKING CHANGE"; then
  echo "major"
# Check for new features
elif echo "$COMMITS" | grep -q "^feat"; then
  echo "minor"
# Check for fixes
elif echo "$COMMITS" | grep -q "^fix"; then
  echo "patch"
else
  echo "patch"  # Default to patch for other changes
fi
```

## 🎯 Benefits for SolarScope CLI

### **1. Automatic Changelog Generation**

- No more manual changelog updates
- Consistent formatting
- Complete change history

### **2. Semantic Versioning Automation**

- Automatic version bump detection
- Release notes generation
- Version tag creation

### **3. Better Collaboration**

- Clear commit history
- Easier code reviews
- Better issue tracking

### **4. Release Automation**

- Automatic release creation
- NuGet publishing based on changes
- GitHub release notes

## 🚀 Migration Strategy

### **Phase 1: Adoption** (Week 1)

1. Create commit message templates
2. Add validation hooks
3. Train team on conventional commits

### **Phase 2: Automation** (Week 2)

1. Implement automatic changelog generation
2. Update CI/CD workflows
3. Test release automation

### **Phase 3: Enhancement** (Week 3)

1. Add PR auto-labeling
2. Implement semantic version detection
3. Full automation testing

## 📚 Examples for SolarScope CLI

### **Feature Development**

```bash
git commit -m "feat(dashboard): add real-time data refresh every 30 seconds"
git commit -m "feat(analyze): add correlation analysis between weather and production"
git commit -m "feat(demo): add solar panel animation theme"
```

### **Bug Fixes**

```bash
git commit -m "fix(dashboard): resolve crash when solar data file is corrupted"
git commit -m "fix(weather): correct precipitation unit conversion from mm to inches"
git commit -m "fix(anomalies): handle edge case with zero production days"
```

### **Documentation**

```bash
git commit -m "docs(readme): add troubleshooting section for common installation issues"
git commit -m "docs(api): document weather service API endpoints"
git commit -m "docs: create user guide for advanced analytics features"
```

### **Breaking Changes**

```bash
git commit -m "feat(cli)!: redesign command structure for better usability

BREAKING CHANGE: 
- The --data option is now required for all commands
- Renamed --type to --analysis-type in analyze command
- Removed deprecated --legacy flag"
```

## 🔗 Resources

- [Conventional Commits Specification](https://www.conventionalcommits.org/)
- [Angular Commit Guidelines](https://github.com/angular/angular/blob/main/CONTRIBUTING.md#commit)
- [Commitizen](https://github.com/commitizen/cz-cli)
- [Conventional Changelog](https://github.com/conventional-changelog/conventional-changelog)

Implementing conventional commits will make SolarScope CLI's development process more professional and automate much of the release management! 🌞⚡
