# 🔄 Semantic Versioning & Release Process

This document outlines the semantic versioning strategy and automated release process for SolarScope CLI.

## 📋 Overview

SolarScope CLI follows [Semantic Versioning (SemVer)](https://semver.org/) principles and uses [Conventional Commits](https://www.conventionalcommits.org/) for automated release management:

- **MAJOR** version: Breaking changes
- **MINOR** version: New features (backward compatible)
- **PATCH** version: Bug fixes (backward compatible)

### Conventional Commits Integration

The project uses conventional commit messages to:

- **Automatically detect** version bump type (major/minor/patch)
- **Generate changelogs** from commit messages
- **Create release notes** for GitHub releases
- **Validate** commit message format

**Commit Format**: `<type>[optional scope]: <description>`

**Examples**:

```bash
feat(dashboard): add dark mode theme
fix: resolve memory leak in data processing
docs: update API documentation
feat!: redesign command structure (breaking change)
```

## 🔄 Versioning Workflows

### 1. Version Bump Workflow (`version-bump.yml`)

**Trigger**: Manual dispatch
**Purpose**: Increment version and create release tag

**Usage**:

1. Go to Actions → Version Bump
2. Click "Run workflow"
3. Select bump type: `patch`, `minor`, or `major`
4. Optionally create prerelease version
5. The workflow will:
   - Update version in `SolarScope.csproj`
   - Update `CHANGELOG.md` (for stable releases)
   - Commit changes
   - Create and push git tag

### 2. Release Workflow (`release.yml`)

**Trigger**: Git tag push (`v*.*.*`) or manual dispatch
**Purpose**: Build, test, and publish release

**Process**:

1. Build and test the application
2. Create NuGet package
3. Generate release notes from changelog
4. Create GitHub release
5. Upload package to release assets
6. Publish to NuGet.org (for tag-triggered releases)

### 3. CI Workflow (`ci.yml`)

**Trigger**: Push to main/develop, pull requests
**Purpose**: Continuous integration and testing

**Features**:

- Cross-platform testing (Ubuntu, Windows, macOS)
- Code formatting validation
- Security vulnerability scanning
- Package creation for main branch
- Installation testing

## 🎯 Release Process

### Standard Release

1. **Development**

   ```bash
   # Make your changes
   git add .
   git commit -m "feat: add new solar panel efficiency calculation"
   git push origin main
   ```

2. **Version Bump**
   - Navigate to GitHub Actions
   - Run "Version Bump" workflow
   - Select appropriate bump type
   - This creates a new tag (e.g., `v1.1.0`)

3. **Automatic Release**
   - Tag push triggers "Release" workflow
   - Package is built and published to NuGet.org
   - GitHub release is created with assets

### Prerelease

1. **Create Prerelease**

   ```bash
   # Use version bump workflow with prerelease option
   # Creates version like: 1.1.0-alpha.20250801120000
   ```

2. **Manual Testing**

   ```bash
   dotnet tool install --global SolarScope --version 1.1.0-alpha.20250801120000 --prerelease
   ```

3. **Promote to Stable**
   - Run version bump without prerelease option
   - Creates stable release

### Hotfix Release

1. **Create Hotfix Branch**

   ```bash
   git checkout -b hotfix/1.0.1
   # Fix the issue
   git commit -m "fix: resolve critical dashboard crash"
   git push origin hotfix/1.0.1
   ```

2. **Merge and Release**

   ```bash
   git checkout main
   git merge hotfix/1.0.1
   git push origin main
   # Use version bump workflow with "patch"
   ```

## 📝 Changelog Management

### Format

- Follows [Keep a Changelog](https://keepachangelog.com/) format
- Organized by version with dates
- Categories: Added, Changed, Deprecated, Removed, Fixed, Security

### Automation

- Version bump workflow updates changelog automatically
- Release workflow extracts release notes from changelog
- Manual updates recommended for detailed change descriptions

### Example Entry

```markdown
## [1.1.0] - 2025-08-15

### Added
- New weather pattern analysis command
- Support for multiple solar panel configurations
- Dark mode theme for dashboard

### Changed
- Improved performance of anomaly detection algorithm
- Updated Spectre.Console to version 0.50.1

### Fixed
- Dashboard crash when no data is available
- Memory leak in weather correlation analysis
```

## 🔐 Secrets Configuration

Configure these secrets in GitHub repository settings:

### Required Secrets

- `NUGET_API_KEY`: Your NuGet.org API key for publishing packages

### Optional Secrets

- `GITHUB_TOKEN`: Automatically provided by GitHub Actions

## 🚀 Version Strategy

### Major Versions (1.0.0 → 2.0.0)

- Breaking API changes
- Major architecture changes
- Removal of deprecated features
- New command structure

### Minor Versions (1.0.0 → 1.1.0)

- New commands or features
- Enhanced visualizations
- New data analysis capabilities
- Performance improvements

### Patch Versions (1.0.0 → 1.0.1)

- Bug fixes
- Security patches
- Documentation updates
- Dependency updates

## 📊 Monitoring

### Release Health

- Monitor download statistics on NuGet.org
- Check GitHub release metrics
- Review user feedback and issues

### Quality Gates

- All CI checks must pass
- Cross-platform compatibility verified
- Security vulnerabilities addressed
- Documentation updated

## 🛠️ Troubleshooting

### Failed Release

1. Check workflow logs in GitHub Actions
2. Verify secrets are configured correctly
3. Ensure version follows SemVer format
4. Check for conflicts in changelog

### Version Conflicts

```bash
# If version already exists, bump again
git tag -d v1.1.0  # Delete local tag
git push origin :refs/tags/v1.1.0  # Delete remote tag
# Use version bump workflow to create new version
```

### NuGet Publishing Issues

- Verify NuGet API key is valid
- Check package ID conflicts
- Ensure version doesn't already exist
- Review NuGet.org status page

## 📚 Resources

- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Package Publishing](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
