param(
    [string]$PhaseLabel = "web-readiness-pre-user-testing",
    [string]$BackupRoot = "X:\Projects\DarwinLingua.Backup",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$PostgresUser = "postgres",
    [switch]$SkipDatabaseDump,
    [switch]$SkipRestoreDryRun,
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

function Invoke-DockerPostgresSqlFile {
    param(
        [string]$Database,
        [string]$SqlPath,
        [string]$OutputPath
    )

    $containerSqlPath = "/tmp/$(Split-Path -Leaf $SqlPath)"
    Invoke-Checked "docker" @("cp", $SqlPath, "$PostgresContainer`:$containerSqlPath") (Join-Path $verificationPath "docker-copy-$(Split-Path -Leaf $SqlPath).log")
    Invoke-Checked "docker" @("exec", $PostgresContainer, "psql", "-U", $PostgresUser, "-d", $Database, "-f", $containerSqlPath) $OutputPath
}

function Write-DatabaseInventorySql {
    param([string]$Path)

    $sql = @'
\pset format unaligned
\pset fieldsep |
\pset tuples_only on

select
    'content_by_target_language' as section,
    table_name,
    target_language_code,
    row_count::text
from (
    select 'ContentPackages' as table_name, coalesce("TargetLearningLanguageCode", '<null>') as target_language_code, count(*) as row_count from "ContentPackages" group by 2
    union all select 'ConversationEvents', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "ConversationEvents" group by 2
    union all select 'ConversationStarterPacks', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "ConversationStarterPacks" group by 2
    union all select 'CoursePaths', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "CoursePaths" group by 2
    union all select 'CourseModules', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "CourseModules" group by 2
    union all select 'CourseLessons', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "CourseLessons" group by 2
    union all select 'CountryGuidanceNotes', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "CountryGuidanceNotes" group by 2
    union all select 'DialogueLessons', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "DialogueLessons" group by 2
    union all select 'EventPreparationPacks', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "EventPreparationPacks" group by 2
    union all select 'EventRsvps', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "EventRsvps" group by 2
    union all select 'ExamProfiles', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "ExamProfiles" group by 2
    union all select 'ExamPrepUnits', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "ExamPrepUnits" group by 2
    union all select 'ExerciseSets', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "ExerciseSets" group by 2
    union all select 'Exercises', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "Exercises" group by 2
    union all select 'ExpressionEntries', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "ExpressionEntries" group by 2
    union all select 'GrammarTopics', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "GrammarTopics" group by 2
    union all select 'OrganizerProfileSupportedLevels', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "OrganizerProfileSupportedLevels" group by 2
    union all select 'RoleplayScenarios', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "RoleplayScenarios" group by 2
    union all select 'TalkTopics', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "TalkTopics" group by 2
    union all select 'UserContentProgress', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "UserContentProgress" group by 2
    union all select 'UserExerciseAttempts', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "UserExerciseAttempts" group by 2
    union all select 'UserLearningProfiles', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "UserLearningProfiles" group by 2
    union all select 'WebUserPreferences', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "WebUserPreferences" group by 2
    union all select 'WritingTemplates', coalesce("TargetLearningLanguageCode", '<null>'), count(*) from "WritingTemplates" group by 2
) as counts
order by section, table_name, target_language_code;

select
    'country_guidance_by_target_country' as section,
    coalesce("TargetLearningLanguageCode", '<null>') as target_language_code,
    coalesce("CountryContextCode", '<null>') as country_context_code,
    count(*)::text as row_count
from "CountryGuidanceNotes"
group by 2, 3
order by 2, 3;

select
    'helper_translation_json_nonempty' as section,
    table_name,
    field_name,
    nonempty_count::text
from (
    select 'CourseLessons' as table_name, 'TitleTranslationsJson' as field_name, count(*) filter (where coalesce("TitleTranslationsJson", '') <> '') as nonempty_count from "CourseLessons"
    union all select 'CourseLessons', 'NarrativeTranslationsJson', count(*) filter (where coalesce("NarrativeTranslationsJson", '') <> '') from "CourseLessons"
    union all select 'CountryGuidanceNotes', 'TitleTranslationsJson', count(*) filter (where coalesce("TitleTranslationsJson", '') <> '') from "CountryGuidanceNotes"
    union all select 'CountryGuidanceNotes', 'SectionsTranslationsJson', count(*) filter (where coalesce("SectionsTranslationsJson", '') <> '') from "CountryGuidanceNotes"
    union all select 'ExamPrepUnits', 'TitleTranslationsJson', count(*) filter (where coalesce("TitleTranslationsJson", '') <> '') from "ExamPrepUnits"
    union all select 'WritingTemplates', 'TitleTranslationsJson', count(*) filter (where coalesce("TitleTranslationsJson", '') <> '') from "WritingTemplates"
    union all select 'Exercises', 'TitleTranslationsJson', count(*) filter (where coalesce("TitleTranslationsJson", '') <> '') from "Exercises"
    union all select 'RoleplayScenarios', 'TitleTranslationsJson', count(*) filter (where coalesce("TitleTranslationsJson", '') <> '') from "RoleplayScenarios"
) as translation_counts
order by section, table_name, field_name;
'@

    Write-TextFile -Path $Path -Content $sql
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
    $containerDumpForRestore = "/tmp/$dumpFileName"

    $pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue
    $pgDumpAll = Get-Command pg_dumpall -ErrorAction SilentlyContinue
    $pgRestore = Get-Command pg_restore -ErrorAction SilentlyContinue

    if ($pgDump -and $pgDumpAll -and $pgRestore) {
        Invoke-Checked $pgDump.Source @("-Fc", "-d", $DatabaseName, "-f", $dumpPath) (Join-Path $verificationPath "pg-dump.log")
        Invoke-Checked $pgDumpAll.Source @("--globals-only", "-f", $globalsPath) (Join-Path $verificationPath "pg-dumpall-globals.log")
        Invoke-Checked $pgRestore.Source @("--list", $dumpPath) $restoreListPath
        Invoke-Checked "docker" @("cp", $dumpPath, "$PostgresContainer`:$containerDumpForRestore") (Join-Path $verificationPath "docker-copy-local-dump-for-restore.log")
    }
    else {
        $containerDump = "/tmp/$dumpFileName"
        $containerGlobals = "/tmp/postgres-globals_$timestamp.sql"
        Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_dump", "-U", $PostgresUser, "-Fc", "-d", $DatabaseName, "-f", $containerDump) (Join-Path $verificationPath "docker-pg-dump.log")
        Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_dumpall", "-U", $PostgresUser, "--globals-only", "-f", $containerGlobals) (Join-Path $verificationPath "docker-pg-dumpall-globals.log")
        Invoke-Checked "docker" @("cp", "$PostgresContainer`:$containerDump", $dumpPath) (Join-Path $verificationPath "docker-copy-dump.log")
        Invoke-Checked "docker" @("cp", "$PostgresContainer`:$containerGlobals", $globalsPath) (Join-Path $verificationPath "docker-copy-globals.log")
        Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_restore", "--list", $containerDump) $restoreListPath
        $containerDumpForRestore = $containerDump
    }

    $databaseInventorySqlPath = Join-Path $verificationPath "database-inventory.sql"
    Write-DatabaseInventorySql -Path $databaseInventorySqlPath
    Invoke-DockerPostgresSqlFile -Database $DatabaseName -SqlPath $databaseInventorySqlPath -OutputPath (Join-Path $verificationPath "database-inventory.txt")

    if (-not $SkipRestoreDryRun) {
        $restoreDatabaseName = "$($DatabaseName)_restore_verify_$($timestamp -replace '-', '_')"
        Invoke-Checked "docker" @("exec", $PostgresContainer, "createdb", "-U", $PostgresUser, $restoreDatabaseName) (Join-Path $verificationPath "restore-dry-run-createdb.log")

        try {
            Invoke-Checked "docker" @("exec", $PostgresContainer, "pg_restore", "-U", $PostgresUser, "-d", $restoreDatabaseName, $containerDumpForRestore) (Join-Path $verificationPath "restore-dry-run-pg-restore.log")
            Invoke-DockerPostgresSqlFile -Database $restoreDatabaseName -SqlPath $databaseInventorySqlPath -OutputPath (Join-Path $verificationPath "restore-dry-run-database-inventory.txt")
        }
        finally {
            docker exec $PostgresContainer dropdb -U $PostgresUser --if-exists $restoreDatabaseName *> (Join-Path $verificationPath "restore-dry-run-dropdb.log")
        }
    }
}

if (-not $SkipRepoOverlay) {
    New-Item -ItemType Directory -Path $repoOverlayPath -Force | Out-Null

    $bundlePath = Join-Path $repoOverlayPath "darwinlingua-current-head.bundle"
    git -C $repoRoot bundle create $bundlePath HEAD
    if ($LASTEXITCODE -ne 0) {
        throw "git bundle failed while creating the repo restore bundle."
    }

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

function Get-AspNetUserSecretsPath {
    param([string]$ProjectRelativePath)

    $projectPath = Join-Path $repoRoot $ProjectRelativePath
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        return $null
    }

    [xml]$projectXml = Get-Content -LiteralPath $projectPath
    $userSecretsId = ($projectXml.Project.PropertyGroup |
        ForEach-Object { $_.UserSecretsId } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1)

    if ([string]::IsNullOrWhiteSpace($userSecretsId)) {
        return $null
    }

    $appData = [Environment]::GetFolderPath("ApplicationData")
    if ([string]::IsNullOrWhiteSpace($appData)) {
        return $null
    }

    return [ordered]@{
        Id = $userSecretsId
        Path = Join-Path $appData "Microsoft\UserSecrets\$userSecretsId\secrets.json"
    }
}

$aspNetUserSecrets = @(
    Get-AspNetUserSecretsPath -ProjectRelativePath "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj"
)

foreach ($userSecrets in $aspNetUserSecrets) {
    if ($null -eq $userSecrets -or -not (Test-Path -LiteralPath $userSecrets.Path -PathType Leaf)) {
        continue
    }

    $targetPath = Join-Path $secretsPath "aspnet-user-secrets\$($userSecrets.Id)\secrets.json"
    New-Item -ItemType Directory -Path (Split-Path -Parent $targetPath) -Force | Out-Null
    Copy-Item -LiteralPath $userSecrets.Path -Destination $targetPath -Force
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
- Restore dry-run skipped: $($SkipRestoreDryRun.IsPresent)
- Repo overlay skipped: $($SkipRepoOverlay.IsPresent)

## Restore Outline

1. Start from a clean checkout at the recorded Git commit when possible.
2. If the recorded Git commit is not available from the remote repository, clone or fetch ``repo-overlay/darwinlingua-current-head.bundle`` first, then check out the recorded commit.
3. Copy ``repo-overlay/`` over the checkout if local uncommitted files are needed.
4. Restore secrets from `secrets/` to their original local paths.
   - ASP.NET Core user-secrets are stored under `secrets/aspnet-user-secrets/<UserSecretsId>/secrets.json`.
5. Restore PostgreSQL from `db/*.dump` with `pg_restore --clean --if-exists --create`.
6. Verify counts and run Web/WebApi smoke before exposing the environment.

## Verification

- `checksums.sha256` was generated after backup collection.
- `db/*.restore-list.txt` exists when database dump was enabled.
- ``verification/database-inventory.txt`` records source counts by target language, Country Guidance country context, and helper-translation JSON coverage.
- ``verification/restore-dry-run-database-inventory.txt`` records the same counts after a temporary restore unless dry-run was skipped.
- ``verification/git-status.txt`` records dirty/untracked state at backup time.
"@

Write-TextFile -Path (Join-Path $backupPath "manifest.md") -Content $manifest

Write-Output "BACKUP_PATH=$backupPath"
