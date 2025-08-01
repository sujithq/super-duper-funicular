# 🔐 GitHub Actions Permissions Fix

## ❌ The Problem

The GitHub Actions workflows were failing with a 403 permission error:

```bash
remote: Permission to sujithq/super-duper-funicular.git denied to github-actions[bot].
fatal: unable to access 'https://github.com/sujithq/super-duper-funicular/': The requested URL returned error: 403
```

This happened because workflows that need to push commits, create tags, or create releases require explicit permissions.

## ✅ The Solution

Added the `permissions` section to all workflows that perform write operations:

### **Fixed Workflows:**

#### **1. auto-changelog.yml**

- **Needs**: Push commits to update changelog
- **Added**: `contents: write` and `pull-requests: write`

#### **2. conventional-release.yml**

- **Needs**: Create releases, push tags, and publish packages
- **Added**: `contents: write` and `pull-requests: write`

#### **3. version-bump.yml**

- **Needs**: Push commits and create/push tags
- **Added**: `contents: write`

#### **4. release.yml**

- **Needs**: Create GitHub releases and publish packages
- **Added**: `contents: write`

#### **5. ci.yml**

- **Status**: ✅ No changes needed (read-only operations)

## 🔧 What Was Changed

Each workflow now includes this permissions block at the top level:

```yaml
permissions:
  contents: write           # Required for: commits, tags, releases
  pull-requests: write     # Required for: PR operations (where needed)
```

## 🎯 Permission Details

### **`contents: write`**

Allows the workflow to:

- ✅ Push commits to the repository
- ✅ Create and push tags
- ✅ Create GitHub releases
- ✅ Modify repository files

### **`pull-requests: write`**

Allows the workflow to:

- ✅ Create and update pull requests
- ✅ Add comments to pull requests
- ✅ Update PR status

## 🚀 Now Working

All workflows can now successfully:

- **📝 Auto-update changelog** from conventional commits
- **🏷️ Create version tags** and push them
- **📦 Create GitHub releases** with release notes
- **🚀 Publish to NuGet** automatically
- **⚡ Detect version bumps** from commit messages

## 🔒 Security Notes

These permissions are:

- ✅ **Scoped to specific workflows** - not repository-wide
- ✅ **Minimal required permissions** - only what each workflow needs
- ✅ **GitHub's recommended approach** for automated workflows
- ✅ **Secure by design** - no excessive permissions granted

Your conventional commits workflow is now fully operational! 🌞⚡
