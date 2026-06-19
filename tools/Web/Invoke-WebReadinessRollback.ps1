param(
    [Parameter(Mandatory = $true)]
    [string]$BackupPath,
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres",
    [switch]$ApplyDatabaseRestore,
    [switch]$ApplyRepoOverlay,
    [switch]$ConfirmRollback
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$backup = Resolve-Path $BackupPath
$manifestPath = Join-Path $backup "manifest.md"
$repoOverlayPath = Join-Path $backup "repo-overlay"
$dbPath = Join-Path $backup "db"

if (-not (Test-Path $manifestPath)) {
    throw "Backup manifest was not found: $manifestPath"
}

Write-Output "Rollback source: $backup"
Write-Output "Current repo: $repoRoot"
Write-Output "Mode: dry-run unless -ApplyDatabaseRestore or -ApplyRepoOverlay is supplied."

$dump = Get-ChildItem -LiteralPath $dbPath -Filter "$DatabaseName*.dump" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($dump) {
    Write-Output "Database dump: $($dump.FullName)"
}
else {
    Write-Output "Database dump: not found"
}

if ($ApplyRepoOverlay) {
    if (-not $ConfirmRollback) {
        throw "Refusing to copy repo overlay without -ConfirmRollback."
    }

    if (-not (Test-Path $repoOverlayPath)) {
        throw "Repo overlay not found: $repoOverlayPath"
    }

    robocopy $repoOverlayPath $repoRoot /MIR /XD ".git" "bin" "obj" ".vs" "node_modules" /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -gt 7) {
        throw "robocopy repo overlay restore failed with exit code $LASTEXITCODE."
    }

    Write-Output "Repo overlay restored."
}

if ($ApplyDatabaseRestore) {
    if (-not $ConfirmRollback) {
        throw "Refusing to restore database without -ConfirmRollback."
    }

    if (-not $dump) {
        throw "No database dump found for $DatabaseName."
    }

    $pgRestore = Get-Command pg_restore -ErrorAction SilentlyContinue
    if (-not $pgRestore) {
        throw "pg_restore was not found in PATH. Install PostgreSQL client tools or restore manually using the dump path above."
    }

    & $pgRestore.Source "--clean" "--if-exists" "--dbname" $DatabaseName "--username" $PostgresUser $dump.FullName
    if ($LASTEXITCODE -ne 0) {
        throw "pg_restore failed with exit code $LASTEXITCODE."
    }

    Write-Output "Database restore applied."
}

if (-not $ApplyDatabaseRestore -and -not $ApplyRepoOverlay) {
    Write-Output "Dry run complete. Re-run with -ApplyDatabaseRestore and/or -ApplyRepoOverlay plus -ConfirmRollback to apply."
}
