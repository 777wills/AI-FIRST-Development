#!/usr/bin/env pwsh
# Create and switch to a new git branch for feature development
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$FeatureDescription,
    
    [switch]$Json,
    [switch]$AllowExistingBranch,
    [switch]$DryRun,
    [string]$ShortName,
    [Parameter()]
    [long]$Number = 0,
    [switch]$Timestamp,
    [string]$BaseBranch = "main"
)

$ErrorActionPreference = 'Stop'

# Check if git is available
try {
    $null = git --version
} catch {
    Write-Error "Git is not available. Please install git and try again."
    exit 1
}

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus -and -not $DryRun) {
    Write-Warning "You have uncommitted changes in your working directory."
    Write-Warning "It's recommended to commit or stash them before creating a new feature branch."
    
    $response = Read-Host "Do you want to continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Branch creation cancelled."
        exit 0
    }
}

# Load common functions
$commonPath = Join-Path $PSScriptRoot "..\..\..\scripts\powershell\common.ps1"
if (Test-Path $commonPath) {
    . $commonPath
} else {
    Write-Error "Common functions not found at: $commonPath"
    exit 1
}

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

# Determine the specs directory to check for existing feature numbers
$specsDir = Join-Path $repoRoot 'specs'
if (-not (Test-Path $specsDir)) {
    New-Item -ItemType Directory -Path $specsDir -Force | Out-Null
}

# Function to get highest sequential number from existing directories
function Get-HighestNumberFromSpecs {
    param([string]$SpecsDir)

    [long]$highest = 0
    if (Test-Path $SpecsDir) {
        Get-ChildItem -Path $SpecsDir -Directory | ForEach-Object {
            # Match sequential prefixes (>=3 digits), but skip timestamp dirs.
            if ($_.Name -match '^(\d{3,})-' -and $_.Name -notmatch '^\d{8}-\d{6}-') {
                [long]$num = 0
                if ([long]::TryParse($matches[1], [ref]$num) -and $num -gt $highest) {
                    $highest = $num
                }
            }
        }
    }
    return $highest
}

# Function to clean branch name
function ConvertTo-CleanBranchName {
    param([string]$Name)
    return $Name.ToLower() -replace '[^a-z0-9]', '-' -replace '-{2,}', '-' -replace '^-', '' -replace '-$', ''
}

# Function to generate branch name with stop word filtering
function Get-BranchName {
    param([string]$Description)

    $stopWords = @(
        'i', 'a', 'an', 'the', 'to', 'for', 'of', 'in', 'on', 'at', 'by', 'with', 'from',
        'is', 'are', 'was', 'were', 'be', 'been', 'being', 'have', 'has', 'had',
        'do', 'does', 'did', 'will', 'would', 'should', 'could', 'can', 'may', 'might', 'must', 'shall',
        'this', 'that', 'these', 'those', 'my', 'your', 'our', 'their',
        'want', 'need', 'add', 'get', 'set'
    )

    $cleanName = $Description.ToLower() -replace '[^a-z0-9\s]', ' '
    $words = $cleanName -split '\s+' | Where-Object { $_ }

    $meaningfulWords = @()
    foreach ($word in $words) {
        if ($stopWords -contains $word) { continue }
        if ($word.Length -ge 3) {
            $meaningfulWords += $word
        } elseif ($Description -match "\b$($word.ToUpper())\b") {
            $meaningfulWords += $word
        }
    }

    if ($meaningfulWords.Count -gt 0) {
        $maxWords = if ($meaningfulWords.Count -eq 4) { 4 } else { 3 }
        $result = ($meaningfulWords | Select-Object -First $maxWords) -join '-'
        return $result
    } else {
        $result = ConvertTo-CleanBranchName -Name $Description
        $fallbackWords = ($result -split '-') | Where-Object { $_ } | Select-Object -First 3
        return [string]::Join('-', $fallbackWords)
    }
}

# Generate branch suffix
if ($ShortName) {
    $branchSuffix = ConvertTo-CleanBranchName -Name $ShortName
} else {
    $branchSuffix = Get-BranchName -Description $FeatureDescription
}

# Warn if -Number and -Timestamp are both specified
if ($Timestamp -and $Number -ne 0) {
    Write-Warning "[git-branch] Warning: -Number is ignored when -Timestamp is used"
    $Number = 0
}

# Determine branch prefix
if ($Timestamp) {
    $featureNum = Get-Date -Format 'yyyyMMdd-HHmmss'
    $branchName = "$featureNum-$branchSuffix"
} else {
    if ($Number -eq 0) {
        $Number = (Get-HighestNumberFromSpecs -SpecsDir $specsDir) + 1
    }
    $featureNum = ('{0:000}' -f $Number)
    $branchName = "$featureNum-$branchSuffix"
}

# GitHub enforces a 244-byte limit on branch names
$maxBranchLength = 244
if ($branchName.Length -gt $maxBranchLength) {
    $prefixLength = $featureNum.Length + 1
    $maxSuffixLength = $maxBranchLength - $prefixLength
    $truncatedSuffix = $branchSuffix.Substring(0, [Math]::Min($branchSuffix.Length, $maxSuffixLength))
    $truncatedSuffix = $truncatedSuffix -replace '-$', ''
    
    $originalBranchName = $branchName
    $branchName = "$featureNum-$truncatedSuffix"
    
    Write-Warning "[git-branch] Branch name exceeded GitHub's 244-byte limit"
    Write-Warning "[git-branch] Original: $originalBranchName ($($originalBranchName.Length) bytes)"
    Write-Warning "[git-branch] Truncated to: $branchName ($($branchName.Length) bytes)"
}

# Check if branch already exists
$branchExists = git branch --list $branchName
if ($branchExists -and -not $AllowExistingBranch -and -not $DryRun) {
    Write-Error "Branch '$branchName' already exists. Use -AllowExistingBranch to switch to it anyway."
    exit 1
}

if (-not $DryRun) {
    if ($branchExists) {
        # Switch to existing branch
        Write-Host "[git-branch] Switching to existing branch: $branchName"
        git checkout $branchName
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to switch to branch '$branchName'"
            exit 1
        }
    } else {
        # Ensure we're on the base branch before creating new one
        $currentBranch = git rev-parse --abbrev-ref HEAD
        if ($currentBranch -ne $BaseBranch) {
            Write-Host "[git-branch] Switching to base branch: $BaseBranch"
            git checkout $BaseBranch
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Failed to switch to base branch '$BaseBranch', staying on '$currentBranch'"
            }
        }
        
        # Create and switch to new branch
        Write-Host "[git-branch] Creating new branch: $branchName"
        git checkout -b $branchName
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to create branch '$branchName'"
            exit 1
        }
    }
    
    Write-Host "[git-branch] ✓ Now on branch: $branchName"
}

# Output result
if ($Json) {
    $obj = [PSCustomObject]@{
        BRANCH_NAME = $branchName
        FEATURE_NUM = $featureNum
        BASE_BRANCH = $BaseBranch
    }
    if ($DryRun) {
        $obj | Add-Member -NotePropertyName 'DRY_RUN' -NotePropertyValue $true
    }
    $obj | ConvertTo-Json -Compress
} else {
    Write-Output "BRANCH_NAME: $branchName"
    Write-Output "FEATURE_NUM: $featureNum"
    Write-Output "BASE_BRANCH: $BaseBranch"
}
