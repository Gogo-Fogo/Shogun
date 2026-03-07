[CmdletBinding()]
param(
    [string]$SourceRoot = "E:\Backups\Shogun",

    [string]$DestinationRoot = "C:\Users\georg\Google Drive\Backups\Shogun",

    [string]$SnapshotName
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$sourceRootPath = [System.IO.Path]::GetFullPath($SourceRoot)
$destinationRootPath = [System.IO.Path]::GetFullPath($DestinationRoot)

if (-not (Test-Path $sourceRootPath -PathType Container)) {
    throw "Source backup root does not exist: $sourceRootPath"
}

if ([string]::IsNullOrWhiteSpace($SnapshotName)) {
    $latestSnapshot = Get-ChildItem -Path $sourceRootPath -Directory |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latestSnapshot) {
        throw "No backup snapshots were found under: $sourceRootPath"
    }

    $SnapshotName = $latestSnapshot.Name
}

$sourceSnapshotPath = Join-Path $sourceRootPath $SnapshotName
if (-not (Test-Path $sourceSnapshotPath -PathType Container)) {
    throw "Snapshot does not exist: $sourceSnapshotPath"
}

$destinationSnapshotPath = Join-Path $destinationRootPath $SnapshotName

$null = New-Item -ItemType Directory -Path $destinationRootPath -Force
$null = New-Item -ItemType Directory -Path $destinationSnapshotPath -Force

& robocopy $sourceSnapshotPath $destinationSnapshotPath /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
$exitCode = $LASTEXITCODE

if ($exitCode -ge 8) {
    throw "robocopy failed for '$SnapshotName' with exit code $exitCode."
}

$summary = [ordered]@{
    mirrored_at_iso     = (Get-Date).ToString("o")
    source_snapshot     = $sourceSnapshotPath
    destination_snapshot = $destinationSnapshotPath
    snapshot_name       = $SnapshotName
}

$summaryPath = Join-Path $destinationSnapshotPath "google-drive-mirror-manifest.json"
$summary | ConvertTo-Json -Depth 3 | Set-Content -Path $summaryPath -Encoding UTF8

Write-Host "Mirrored backup snapshot to Google Drive folder: $destinationSnapshotPath"
