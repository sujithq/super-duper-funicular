# 📝 Changelog Maintenance Guide

## How the Changelog is Kept Up to Date

The changelog in SolarScope CLI is maintained through a **hybrid approach** combining automation and manual updates.

## 🔄 Current System

### 1. **Manual Updates During Development** (Recommended)

Developers should manually update the `[Unreleased]` section as they add features or fix bugs:

```markdown
## [Unreleased]

### Added
- New weather correlation analysis command
- Support for multiple solar panel configurations  
- Dark mode theme for dashboard

### Changed
- Improved performance of anomaly detection algorithm
- Updated Spectre.Console to version 0.50.1

### Fixed
- Dashboard crash when no data is available
- Memory leak in weather correlation analysis

### Removed
- Deprecated legacy command options
```

### 2. **Automated Processing During Release**

When creating a release, the workflows automatically:

#### Version Bump Workflow (`version-bump.yml`)

- Adds basic version entry to changelog
- Creates structured sections for the new version

#### Release Workflow (`release.yml`)

- Converts `[Unreleased]` section to versioned section with date
- Creates new empty `[Unreleased]` section for future changes
- Extracts release notes for GitHub release

### 3. **Workflow Integration**

```bash
# Release workflow automatically:
# 1. Replace [Unreleased] with actual version and date
sed -i "s/## \[Unreleased\]/## [$VERSION] - $DATE/" CHANGELOG.md

# 2. Add new [Unreleased] section at top
sed -i "/## \[$VERSION\]/i\\## [Unreleased]\\n\\n### Added\\n\\n### Changed\\n\\n### Fixed\\n\\n### Removed\\n" CHANGELOG.md

# 3. Extract release notes for GitHub release
awk '/## \[$VERSION\]/{flag=1; next} /## \[/{flag=0} flag' CHANGELOG.md > release_notes.md
```

## 🎯 Best Practices

### Commit Message Guidelines

Use conventional commit format to make changelog updates easier:

```bash
# Features
git commit -m "feat: add weather correlation analysis"
git commit -m "feat(dashboard): add dark mode theme"

# Bug fixes  
git commit -m "fix: resolve dashboard crash on empty data"
git commit -m "fix(anomalies): memory leak in correlation analysis"

# Breaking changes
git commit -m "feat!: redesign command structure"
git commit -m "feat(cli)!: remove deprecated options"

# Other changes
git commit -m "docs: update installation guide"
git commit -m "chore: update dependencies"
git commit -m "refactor: simplify dashboard rendering"
```

### Changelog Entry Guidelines

#### **Added** - New features

```markdown
### Added
- Weather correlation analysis command (`solarscope analyze --type correlation`)
- Support for multiple solar panel configurations in dashboard
- Dark mode theme with `--theme dark` option
```

#### **Changed** - Changes in existing functionality

```markdown
### Changed
- Improved anomaly detection algorithm performance by 40%
- Updated dashboard layout for better readability
- Enhanced error messages with suggested solutions
```

#### **Fixed** - Bug fixes

```markdown
### Fixed
- Dashboard crash when solar data file is empty
- Memory leak in weather correlation analysis
- Incorrect date formatting in production reports
```

#### **Removed** - Removed features

```markdown
### Removed
- Deprecated `--legacy` command option
- Support for JSON schema v1.0 (use v2.0+)
```

#### **Security** - Security fixes

```markdown
### Security
- Fixed potential path traversal in data file loading
- Updated dependencies to address CVE-2024-12345
```

## 🔄 Release Flow Example

### 1. Development Phase

```markdown
## [Unreleased]

### Added
- New weather correlation command
- Dashboard dark mode theme

### Fixed
- Memory leak in anomaly detection
```

### 2. Version Bump (e.g., 1.1.0)

Workflow automatically transforms to:

```markdown
## [Unreleased]

### Added

### Changed

### Fixed

### Removed

## [1.1.0] - 2025-08-01

### Added
- New weather correlation command
- Dashboard dark mode theme

### Fixed
- Memory leak in anomaly detection
```

### 3. GitHub Release

Release notes automatically generated:

```markdown
## 🌞 SolarScope CLI v1.1.0

### What's New

### Added
- New weather correlation command
- Dashboard dark mode theme

### Fixed
- Memory leak in anomaly detection

### Installation
dotnet tool install --global SolarScope --version 1.1.0
```

## 🚀 Future Enhancements

### Automated Changelog Generation

Could implement conventional commits parsing to auto-generate changelog entries:

```bash
# Analyze commits since last release
git log v1.0.0..HEAD --oneline --grep="feat\|fix\|break"

# Auto-generate changelog entries
feat: add weather correlation → ### Added - Weather correlation analysis
fix: dashboard crash → ### Fixed - Dashboard crash on empty data
feat!: redesign CLI → ### Changed - **BREAKING**: Redesigned command structure
```

### PR-based Updates

Could require changelog updates in pull requests:

- PR template includes changelog section
- CI checks verify changelog is updated
- Automated reminders for missing changelog entries

## 📋 Maintenance Checklist

### For Developers

- [ ] Update `[Unreleased]` section for each significant change
- [ ] Use conventional commit messages
- [ ] Review changelog before creating releases
- [ ] Ensure breaking changes are clearly marked

### For Releases

- [ ] Verify `[Unreleased]` section has all changes
- [ ] Use version bump workflow to create release
- [ ] Review auto-generated release notes
- [ ] Update documentation if needed

## 🎯 Summary

The changelog maintenance strategy balances **automation** (structure and processing) with **human input** (meaningful descriptions). This ensures:

- ✅ **Consistent Format**: Automated structure following Keep a Changelog
- ✅ **Meaningful Content**: Human-written descriptions of changes
- ✅ **Release Integration**: Automatic release note generation
- ✅ **Developer Friendly**: Simple process for updating during development
- ✅ **User Value**: Clear communication of what's new in each version

The key is **developers manually updating the `[Unreleased]` section** during development, then letting automation handle the version conversion and release processes.
