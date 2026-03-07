[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$DestinationRoot,

    [string]$Label,

    [switch]$IncludeGitMetadata
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Snapshot only the authored project data that matters for recovery.
# This intentionally avoids rebuildable Unity folders like Library/ and Temp/.
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$destinationRootPath = [System.IO.Path]::GetFullPath($DestinationRoot)
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

$cleanLabel = ""
if (-not [string]::IsNullOrWhiteSpace($Label)) {
    $cleanLabel = "_" + (($Label.Trim()) -replace "[^A-Za-z0-9._-]", "-")
}

$snapshotName = "{0}_Shogun{1}" -f $timestamp, $cleanLabel
$snapshotRoot = Join-Path $destinationRootPath $snapshotName

$directoryItems = @(
    "Assets",
    "Packages",
    "ProjectSettings",
    "docs",
    "tools",
    ".claude"
)

if ($IncludeGitMetadata) {
    $directoryItems += ".git"
}

$fileItems = @(
    ".gitignore",
    ".mcp.json",
    "AGENTS.md",
    "CLAUDE.md",
    "IMPLEMENTATION_PROGRESS.md",
    "README.md",
    "TODO_AI_SAFETY_AND_BACKUP.md"
)

function Get-GitValue {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    try {
        $result = & git @Arguments 2>$null
        if ($LASTEXITCODE -eq 0) {
            return ($result | Select-Object -First 1)
        }
    } catch {
        return $null
    }

    return $null
}

function Copy-RepoDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $source = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path $source -PathType Container)) {
        return
    }

    $destination = Join-Path $snapshotRoot $RelativePath
    $null = New-Item -ItemType Directory -Path $destination -Force

    & robocopy $source $destination /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    $exitCode = $LASTEXITCODE

    if ($exitCode -ge 8) {
        throw "robocopy failed for '$RelativePath' with exit code $exitCode."
    }
}

function Copy-RepoFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $source = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path $source -PathType Leaf)) {
        return
    }

    $destination = Join-Path $snapshotRoot $RelativePath
    $destinationParent = Split-Path -Parent $destination
    if (-not [string]::IsNullOrWhiteSpace($destinationParent)) {
        $null = New-Item -ItemType Directory -Path $destinationParent -Force
    }

    Copy-Item -Path $source -Destination $destination -Force
}

$null = New-Item -ItemType Directory -Path $snapshotRoot -Force

foreach ($directoryItem in $directoryItems) {
    Copy-RepoDirectory -RelativePath $directoryItem
}

foreach ($fileItem in $fileItems) {
    Copy-RepoFile -RelativePath $fileItem
}

$manifest = [ordered]@{
    created_at_iso       = (Get-Date).ToString("o")
    source_root          = $repoRoot
    snapshot_root        = $snapshotRoot
    git_branch           = Get-GitValue -Arguments @("rev-parse", "--abbrev-ref", "HEAD")
    git_commit           = Get-GitValue -Arguments @("rev-parse", "HEAD")
    include_git_metadata = [bool]$IncludeGitMetadata
    included_directories = $directoryItems
    included_files       = $fileItems
}

$manifestPath = Join-Path $snapshotRoot "backup-manifest.json"
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath -Encoding UTF8

Write-Host "Created backup snapshot: $snapshotRoot"
