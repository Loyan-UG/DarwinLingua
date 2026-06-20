param(
    [string]$PhaseLabel = "web-readiness-pre-user-testing",
    [string]$BackupRoot = "X:\Projects\DarwinLingua.Backup",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$PostgresUser = "postgres",
    [switch]$SkipDatabaseDump,
    [switch]$SkipRepoOverlay
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupPath = Join-Path $BackupRoot "$timestamp-$PhaseLabel"
$dbPath = Join-Path $backupPath "db"
$verificationPath = Join-Path $backupPath "verification"
$repoOverlayPath = Join-Path $backupPath "repo-overlay"
$secretsPath = Join-Path $backupPath "secrets"
$dockerPath = Join-Path $backupPath "docker"
$artifactsPath = Join-Path $backupPath "artifacts"

New-Item -ItemType Directory -Path $backupPath, $dbPath, $verificationPath, $secretsPath, $dockerPath, $artifactsPath -Force | Out-Null

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Content
    )

    $directory = Split-Path -Parent $Path
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -LiteralPath $Path -Value $Content -Encoding UTF8
}

function Invoke-Checked {
    param(
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$OutputPath
    )

    & $FilePath @ArgumentList *> $OutputPath
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE. See $OutputPath."
    }
}

$gitHead = (& git -C $repoRoot rev-parse HEAD).Trim()
$gitBranch = (& git -C $repoRoot branch --show-current).Trim()
$gitStatus = & git -C $repoRoot status --short

Write-TextFile -Path (Join-Path $verificationPath "git-status.txt") -Content (($gitStatus | Out-String).TrimEnd())

if (-not $SkipDatabaseDump) {
    $dumpFileName = "$DatabaseName`_$timestamp.dump"
    $dumpPath = Join-Path $dbPath $dumpFileName
    $restoreListPath = Join-Path $dbPath "$DatabaseName`_$timestamp.restore-list.txt"
    $globalsPath = Join-Path $dbPath "postgres-globals_$timestamp.sql"

    $pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue
    $pgDumpAll = Get-Command pg_dumpall -ErrorAction SilentlyContinue
    $pgRestore = Get-Command pg_restore -ErrorAction SilentlyContinue

    if ($pgDump -and $pgDumpAll -and $pgRestore) {
        Invoke-Checked $pgDump.Source @("-Fc", "-d", $DatabaseName, "-f", $dumpPath) (Join-Path $verificationPath "pg-dump.log")
        Invoke-Checked $pgDumpAll.Source @("--globals-only", "-f", $globalsPath) (Join-Path $verificationPath "pg-dumpall-globals.log")
        Invoke-Checked $pgRestore.Source @("--list", $dumpPath) $restoreListPath
    }
    else {
        $containerDump = "/tmp/$dumpFileName"
        $containerGlobals = "/tmp/postgres-globals_$timestamp.sql"
        Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_dump", "-U", $PostgresUser, "-Fc", "-d", $DatabaseName, "-f", $containerDump) (Join-Path $verificationPath "docker-pg-dump.log")
        Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_dumpall", "-U", $PostgresUser, "--globals-only", "-f", $containerGlobals) (Join-Path $verificationPath "docker-pg-dumpall-globals.log")
        Invoke-Checked "docker" @("cp", "$PostgresContainer`:$containerDump", $dumpPath) (Join-Path $verificationPath "docker-copy-dump.log")
        Invoke-Checked "docker" @("cp", "$PostgresContainer`:$containerGlobals", $globalsPath) (Join-Path $verificationPath "docker-copy-globals.log")
        Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_restore", "--list", $containerDump) $restoreListPath
    }
}

if (-not $SkipRepoOverlay) {
    New-Item -ItemType Directory -Path $repoOverlayPath -Force | Out-Null

    $diffPath = Join-Path $repoOverlayPath "git-diff-binary.patch"
    git -C $repoRoot diff --binary -- . ':!bin' ':!obj' ':!.vs' ':!node_modules' ':!*.log' ':!*.tmp' > $diffPath
    if ($LASTEXITCODE -ne 0) {
        throw "git diff failed while creating repo overlay patch."
    }

    $changedFilesRoot = Join-Path $repoOverlayPath "changed-files"
    New-Item -ItemType Directory -Path $changedFilesRoot -Force | Out-Null
    $statusEntries = git -C $repoRoot status --porcelain=v1
    foreach ($entry in $statusEntries) {
        if ([string]::IsNullOrWhiteSpace($entry) -or $entry.Length -lt 4) {
            continue
        }

        $path = $entry.Substring(3).Trim()
        if ($path.Contains(" -> ")) {
            $path = ($path -split " -> ", 2)[1]
        }

        $path = $path.Trim('"')
        if ([string]::IsNullOrWhiteSpace($path)) {
            continue
        }

        $normalizedPath = $path -replace "/", "\"
        if ($normalizedPath -match '(^|\\)(bin|obj|\.vs|node_modules)(\\|$)' -or
            $normalizedPath.EndsWith(".log", [StringComparison]::OrdinalIgnoreCase) -or
            $normalizedPath.EndsWith(".tmp", [StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $sourcePath = Join-Path $repoRoot $normalizedPath
        if (-not (Test-Path $sourcePath -PathType Leaf)) {
            continue
        }

        $targetPath = Join-Path $changedFilesRoot $normalizedPath
        New-Item -ItemType Directory -Path (Split-Path -Parent $targetPath) -Force | Out-Null
        Copy-Item -LiteralPath $sourcePath -Destination $targetPath -Force
    }
}

$secretCandidates = @(
    "tools\Server\Postgres\.env",
    "src\Apps\DarwinLingua.WebApi\appsettings.Development.Local.json",
    "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json"
)

foreach ($relativePath in $secretCandidates) {
    $sourcePath = Join-Path $repoRoot $relativePath
    if (Test-Path $sourcePath) {
        $targetPath = Join-Path $secretsPath $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $targetPath) -Force | Out-Null
        Copy-Item -LiteralPath $sourcePath -Destination $targetPath -Force
    }
}

$artifactCandidates = @(
    "artifacts\validation",
    "artifacts\installability-report.json",
    "artifacts\installability-report-android.json"
)

foreach ($relativePath in $artifactCandidates) {
    $sourcePath = Join-Path $repoRoot $relativePath
    if (-not (Test-Path $sourcePath)) {
        continue
    }

    $targetPath = Join-Path $backupPath $relativePath
    New-Item -ItemType Directory -Path (Split-Path -Parent $targetPath) -Force | Out-Null
    if (Test-Path $sourcePath -PathType Container) {
        Copy-Item -LiteralPath $sourcePath -Destination $targetPath -Recurse -Force
    }
    else {
        Copy-Item -LiteralPath $sourcePath -Destination $targetPath -Force
    }
}

try {
    docker inspect $PostgresContainer | Set-Content -LiteralPath (Join-Path $dockerPath "$PostgresContainer.inspect.json") -Encoding UTF8
}
catch {
    Write-TextFile -Path (Join-Path $dockerPath "$PostgresContainer.inspect.error.txt") -Content $_.Exception.Message
}

$checksumPath = Join-Path $backupPath "checksums.sha256"
Get-ChildItem -LiteralPath $backupPath -Recurse -File |
    Where-Object { $_.FullName -ne $checksumPath } |
    Sort-Object FullName |
    ForEach-Object {
        $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256
        "$($hash.Hash.ToLowerInvariant())  $($_.FullName.Substring($backupPath.Length + 1))"
    } | Set-Content -LiteralPath $checksumPath -Encoding UTF8

$manifest = @"
# Darwin Lingua Phase Backup

- Timestamp: $timestamp
- Phase label: $PhaseLabel
- Backup path: $backupPath
- Git branch: $gitBranch
- Git commit: $gitHead
- Database: $DatabaseName
- Database dump skipped: $($SkipDatabaseDump.IsPresent)
- Repo overlay skipped: $($SkipRepoOverlay.IsPresent)

## Restore Outline

1. Start from a clean checkout at the recorded Git commit when possible.
2. Copy `repo-overlay/` over the checkout if local uncommitted files are needed.
3. Restore secrets from `secrets/` to their original local paths.
4. Restore PostgreSQL from `db/*.dump` with `pg_restore --clean --if-exists --create`.
5. Verify counts and run Web/WebApi smoke before exposing the environment.

## Verification

- `checksums.sha256` was generated after backup collection.
- `db/*.restore-list.txt` exists when database dump was enabled.
- `verification/git-status.txt` records dirty/untracked state at backup time.
"@

Write-TextFile -Path (Join-Path $backupPath "manifest.md") -Content $manifest

Write-Output "BACKUP_PATH=$backupPath"
