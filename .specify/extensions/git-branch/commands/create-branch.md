---
description: Create and switch to a new git branch for the feature specification workflow
---

## Command: git.create-branch

This command creates a new git branch and switches to it before the feature specification process begins.

### Execution Flow

1. **Generate branch name** from the feature description
2. **Check if branch already exists**
   - If exists and not allowed: abort with error
   - If exists and allowed: switch to existing branch
3. **Create and switch to new branch** if it doesn't exist
4. **Output JSON** with branch information

### Script Invocation

Execute the PowerShell script:

```powershell
$scriptPath = Join-Path $env:SPECIFY_ROOT ".specify\extensions\git-branch\scripts\create-git-branch.ps1"
$result = & $scriptPath -FeatureDescription "$ARGUMENTS" -Json
```

### Expected Output

JSON format:
```json
{
  "BRANCH_NAME": "003-user-auth",
  "FEATURE_NUM": "003",
  "BASE_BRANCH": "main"
}
```

### Error Handling

- If git is not available: abort with error message
- If there are uncommitted changes: warn user and ask for confirmation
- If branch already exists: abort unless `-AllowExistingBranch` is used
- If branch creation fails: abort with error details

### Post-Execution

The JSON output is parsed by SpecKit and the values are available for subsequent commands:
- `BRANCH_NAME` - used for directory naming if desired
- `FEATURE_NUM` - sequential or timestamp prefix
