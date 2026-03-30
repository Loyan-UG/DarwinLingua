using System.Data.Common;
using System.Net.Http.Json;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinDeutsch.Maui.Services.Updates;

internal sealed class RemoteContentUpdateService(
    HttpClient httpClient,
    RemoteContentUpdateOptions options) : IRemoteContentUpdateService
{
    private const string LegacyLastRemotePackageIdPreferenceKey = "remote-content-last-package-id";
    private const string LegacyLastRemotePackageVersionPreferenceKey = "remote-content-last-package-version";
    private const string LegacyLastRemoteSuccessAtPreferenceKey = "remote-content-last-success-at-utc";
    private const string LegacyLastRemoteFailureMessagePreferenceKey = "remote-content-last-failure-message";

    public Task<RemoteContentUpdateStatus> GetUpdateStatusAsync(string databasePath, CancellationToken cancellationToken) =>
        GetUpdateStatusCoreAsync(databasePath, RemoteUpdateScope.FullDatabase, cancellationToken);

    public Task<RemoteContentUpdateStatus> GetAreaUpdateStatusAsync(string databasePath, string areaKey, CancellationToken cancellationToken) =>
        GetUpdateStatusCoreAsync(databasePath, RemoteUpdateScope.ForArea(areaKey), cancellationToken);

    public Task<RemoteContentUpdateStatus> GetCefrUpdateStatusAsync(string databasePath, string cefrLevel, CancellationToken cancellationToken) =>
        GetUpdateStatusCoreAsync(databasePath, RemoteUpdateScope.ForCefrLevel(cefrLevel), cancellationToken);

    public Task<RemoteContentUpdateResult> ApplyFullUpdateAsync(string databasePath, CancellationToken cancellationToken) =>
        ApplyUpdateCoreAsync(databasePath, RemoteUpdateScope.FullDatabase, cancellationToken);

    public Task<RemoteContentUpdateResult> ApplyAreaUpdateAsync(string databasePath, string areaKey, CancellationToken cancellationToken) =>
        ApplyUpdateCoreAsync(databasePath, RemoteUpdateScope.ForArea(areaKey), cancellationToken);

    public Task<RemoteContentUpdateResult> ApplyCefrUpdateAsync(string databasePath, string cefrLevel, CancellationToken cancellationToken) =>
        ApplyUpdateCoreAsync(databasePath, RemoteUpdateScope.ForCefrLevel(cefrLevel), cancellationToken);

    private async Task<RemoteContentUpdateStatus> GetUpdateStatusCoreAsync(
        string databasePath,
        RemoteUpdateScope scope,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return CreateUnavailableStatus(scope);
        }

        try
        {
            RemoteContentManifestModel? manifest = await httpClient
                .GetFromJsonAsync<RemoteContentManifestModel>(scope.BuildManifestUri(options), cancellationToken)
                .ConfigureAwait(false);

            RemoteContentPackageModel? remotePackage = SelectLatestPackage(manifest, scope);
            if (remotePackage is null)
            {
                return new RemoteContentUpdateStatus(
                    scope.ScopeKey,
                    scope.ContentAreaKey,
                    scope.SliceKey,
                    scope.PackageType,
                    true,
                    true,
                    false,
                    await GetLocalPackageIdAsync(databasePath, scope, cancellationToken).ConfigureAwait(false),
                    GetLastAppliedVersion(scope),
                    GetLastAppliedChecksum(scope),
                    GetLastAppliedSchemaVersion(scope),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    0,
                    0,
                    manifest?.GeneratedAtUtc,
                    GetLastSuccessfulUpdateAtUtc(scope),
                    GetLastFailureMessage(scope));
            }

            string localPackageId = await GetLocalPackageIdAsync(databasePath, scope, cancellationToken).ConfigureAwait(false);
            string localVersion = GetLastAppliedVersion(scope);
            string localChecksum = GetLastAppliedChecksum(scope);
            int localSchemaVersion = GetLastAppliedSchemaVersion(scope);

            if (string.IsNullOrWhiteSpace(localVersion) &&
                !string.IsNullOrWhiteSpace(localPackageId) &&
                string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase))
            {
                localVersion = remotePackage.Version;
            }

            if (string.IsNullOrWhiteSpace(localChecksum) &&
                !string.IsNullOrWhiteSpace(localPackageId) &&
                string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase))
            {
                localChecksum = remotePackage.Checksum;
            }

            if (localSchemaVersion <= 0 &&
                !string.IsNullOrWhiteSpace(localPackageId) &&
                string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase))
            {
                localSchemaVersion = remotePackage.SchemaVersion;
            }

            bool updateAvailable = !string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase);
            return new RemoteContentUpdateStatus(
                scope.ScopeKey,
                scope.ContentAreaKey,
                scope.SliceKey,
                remotePackage.PackageType,
                true,
                true,
                updateAvailable,
                localPackageId,
                localVersion,
                localChecksum,
                localSchemaVersion,
                remotePackage.PackageId,
                remotePackage.Version,
                remotePackage.Checksum,
                remotePackage.SchemaVersion,
                updateAvailable ? remotePackage.WordCount : 0,
                manifest?.GeneratedAtUtc,
                GetLastSuccessfulUpdateAtUtc(scope),
                GetLastFailureMessage(scope));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            PersistLastFailure(scope, exception.Message);

            return new RemoteContentUpdateStatus(
                scope.ScopeKey,
                scope.ContentAreaKey,
                scope.SliceKey,
                scope.PackageType,
                true,
                false,
                false,
                GetLastAppliedPackageId(scope),
                GetLastAppliedVersion(scope),
                GetLastAppliedChecksum(scope),
                GetLastAppliedSchemaVersion(scope),
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                0,
                null,
                GetLastSuccessfulUpdateAtUtc(scope),
                GetLastFailureMessage(scope));
        }
    }

    private async Task<RemoteContentUpdateResult> ApplyUpdateCoreAsync(
        string databasePath,
        RemoteUpdateScope scope,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return new RemoteContentUpdateResult(false, false, string.Empty, string.Empty, 0, null, "Remote content updates are not configured.");
        }

        string tempDirectory = Path.Combine(Path.GetTempPath(), "darwinlingua-remote-update", Guid.NewGuid().ToString("N"));
        string tempJsonPath = Path.Combine(tempDirectory, "scope-update.json");
        string tempDatabasePath = Path.Combine(tempDirectory, "remote-update.db");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            RemoteContentManifestModel? manifest = await httpClient
                .GetFromJsonAsync<RemoteContentManifestModel>(scope.BuildManifestUri(options), cancellationToken)
                .ConfigureAwait(false);
            RemoteContentPackageModel remotePackage = SelectLatestPackage(manifest, scope)
                ?? throw new InvalidOperationException("The remote manifest does not contain a compatible package for this update scope.");

            string localPackageId = await GetLocalPackageIdAsync(databasePath, scope, cancellationToken).ConfigureAwait(false);
            if (string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase))
            {
                PersistLastFailure(scope, string.Empty);
                return new RemoteContentUpdateResult(true, false, remotePackage.PackageId, remotePackage.Version, 0, GetLastSuccessfulUpdateAtUtc(scope), null);
            }

            await DownloadPackageAsync(scope, remotePackage.PackageId, tempJsonPath, cancellationToken).ConfigureAwait(false);
            await ImportIntoTemporaryDatabaseAsync(tempJsonPath, tempDatabasePath, cancellationToken).ConfigureAwait(false);

            int importedWords = scope.ReplaceMode switch
            {
                ReplaceMode.FullDatabase => await ReplaceAllContentTablesAsync(databasePath, tempDatabasePath, cancellationToken).ConfigureAwait(false),
                ReplaceMode.CefrLevel => await ReplaceCefrLevelContentAsync(databasePath, tempDatabasePath, scope.CefrLevel, cancellationToken).ConfigureAwait(false),
                _ => throw new InvalidOperationException($"The update scope '{scope.ScopeKey}' is not supported.")
            };

            DateTimeOffset appliedAtUtc = DateTimeOffset.UtcNow;
            PersistScopeReceipts(scope, manifest, remotePackage, appliedAtUtc);

            return new RemoteContentUpdateResult(true, true, remotePackage.PackageId, remotePackage.Version, importedWords, appliedAtUtc, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
        {
            PersistLastFailure(scope, exception.Message);
            return new RemoteContentUpdateResult(false, false, string.Empty, string.Empty, 0, null, exception.Message);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private bool IsConfigured() => !string.IsNullOrWhiteSpace(options.BaseUrl);

    private static RemoteContentUpdateStatus CreateUnavailableStatus(RemoteUpdateScope scope) =>
        new(
            scope.ScopeKey,
            scope.ContentAreaKey,
            scope.SliceKey,
            scope.PackageType,
            false,
            false,
            false,
            GetLastAppliedPackageId(scope),
            GetLastAppliedVersion(scope),
            GetLastAppliedChecksum(scope),
            GetLastAppliedSchemaVersion(scope),
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0,
            null,
            GetLastSuccessfulUpdateAtUtc(scope),
            GetLastFailureMessage(scope));

    private static RemoteContentPackageModel? SelectLatestPackage(RemoteContentManifestModel? manifest, RemoteUpdateScope scope) =>
        manifest?.Packages
            .Where(scope.Matches)
            .OrderByDescending(package => package.CreatedAtUtc)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

    private async Task DownloadPackageAsync(RemoteUpdateScope scope, string packageId, string targetPath, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient
            .GetAsync(scope.BuildDownloadUri(options, packageId), cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using FileStream targetStream = File.Create(targetPath);
        await using Stream sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ImportIntoTemporaryDatabaseAsync(string jsonPath, string tempDatabasePath, CancellationToken cancellationToken)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services
            .AddDarwinLinguaInfrastructure(databaseOptions => databaseOptions.DatabasePath = tempDatabasePath)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationInfrastructure();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync(cancellationToken).ConfigureAwait(false);

        IContentImportService importService = serviceProvider.GetRequiredService<IContentImportService>();
        ImportContentPackageResult importResult = await importService
            .ImportAsync(new ImportContentPackageRequest(jsonPath), cancellationToken)
            .ConfigureAwait(false);

        if (!importResult.IsSuccess)
        {
            throw new InvalidOperationException(string.Join(" | ", importResult.Issues.Select(issue => issue.Message)));
        }
    }

    private static async Task<int> ReplaceAllContentTablesAsync(string localDatabasePath, string tempDatabasePath, CancellationToken cancellationToken)
    {
        await using SqliteConnection localConnection = new($"Data Source={localDatabasePath}");
        await localConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await localConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(localConnection, transaction, "ATTACH DATABASE $tempPath AS remote;", cancellationToken, CreateParameter("$tempPath", tempDatabasePath)).ConfigureAwait(false);
        int importedWords = await ExecuteScalarIntAsync(localConnection, transaction, "SELECT COUNT(*) FROM remote.WordEntries;", cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(localConnection, transaction, FullReplaceScript, cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(localConnection, transaction, "DETACH DATABASE remote;", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return importedWords;
    }

    private static async Task<int> ReplaceCefrLevelContentAsync(string localDatabasePath, string tempDatabasePath, string cefrLevel, CancellationToken cancellationToken)
    {
        await using SqliteConnection localConnection = new($"Data Source={localDatabasePath}");
        await localConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await localConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(localConnection, transaction, "ATTACH DATABASE $tempPath AS remote;", cancellationToken, CreateParameter("$tempPath", tempDatabasePath)).ConfigureAwait(false);
        int importedWords = await ExecuteScalarIntAsync(localConnection, transaction, "SELECT COUNT(*) FROM remote.WordEntries;", cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(localConnection, transaction, CefrReplaceScript, cancellationToken, CreateParameter("$cefrLevel", cefrLevel.ToUpperInvariant())).ConfigureAwait(false);
        await ExecuteNonQueryAsync(localConnection, transaction, "DETACH DATABASE remote;", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return importedWords;
    }

    private static async Task<string> GetLocalPackageIdAsync(string databasePath, RemoteUpdateScope scope, CancellationToken cancellationToken)
    {
        if (scope == RemoteUpdateScope.FullDatabase)
        {
            string localFullPackageId = await GetLocalFullPackageIdAsync(databasePath, cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(localFullPackageId) ? GetLastAppliedPackageId(scope) : localFullPackageId;
        }

        return GetLastAppliedPackageId(scope);
    }

    private static async Task<string> GetLocalFullPackageIdAsync(string databasePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(databasePath))
        {
            return string.Empty;
        }

        await using SqliteConnection connection = new($"Data Source={databasePath}");
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using DbCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT PackageId
            FROM ContentPackages
            WHERE PackageId LIKE '%-all-full-%'
            ORDER BY CreatedAtUtc DESC, PackageId DESC
            LIMIT 1;
            """;

        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result?.ToString() ?? string.Empty;
    }

    private static async Task ExecuteNonQueryAsync(SqliteConnection connection, DbTransaction? transaction, string commandText, CancellationToken cancellationToken, params SqliteParameter[] parameters)
    {
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> ExecuteScalarIntAsync(SqliteConnection connection, DbTransaction? transaction, string commandText, CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static SqliteParameter CreateParameter(string parameterName, string value) => new(parameterName, value);

    private static void TryDeleteDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return;
        }

        try { Directory.Delete(path, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    private static DateTimeOffset? GetLastSuccessfulUpdateAtUtc(RemoteUpdateScope scope)
    {
        string? rawValue = Preferences.Default.Get<string?>(BuildPreferenceKey(scope, "last-success-at-utc"), null);
        if (DateTimeOffset.TryParse(rawValue, out DateTimeOffset parsedValue))
        {
            return parsedValue;
        }

        if (scope == RemoteUpdateScope.FullDatabase)
        {
            string? legacyRawValue = Preferences.Default.Get<string?>(LegacyLastRemoteSuccessAtPreferenceKey, null);
            return DateTimeOffset.TryParse(legacyRawValue, out parsedValue) ? parsedValue : null;
        }

        return null;
    }

    private static string GetLastAppliedPackageId(RemoteUpdateScope scope)
    {
        string packageId = Preferences.Default.Get(BuildPreferenceKey(scope, "package-id"), string.Empty);
        return !string.IsNullOrWhiteSpace(packageId)
            ? packageId
            : scope == RemoteUpdateScope.FullDatabase
                ? Preferences.Default.Get(LegacyLastRemotePackageIdPreferenceKey, string.Empty)
                : string.Empty;
    }

    private static string GetLastAppliedVersion(RemoteUpdateScope scope)
    {
        string version = Preferences.Default.Get(BuildPreferenceKey(scope, "package-version"), string.Empty);
        return !string.IsNullOrWhiteSpace(version)
            ? version
            : scope == RemoteUpdateScope.FullDatabase
                ? Preferences.Default.Get(LegacyLastRemotePackageVersionPreferenceKey, string.Empty)
                : string.Empty;
    }

    private static string GetLastAppliedChecksum(RemoteUpdateScope scope)
    {
        return Preferences.Default.Get(BuildPreferenceKey(scope, "checksum"), string.Empty);
    }

    private static int GetLastAppliedSchemaVersion(RemoteUpdateScope scope)
    {
        return Preferences.Default.Get(BuildPreferenceKey(scope, "schema-version"), 0);
    }

    private static string GetLastFailureMessage(RemoteUpdateScope scope)
    {
        string message = Preferences.Default.Get(BuildPreferenceKey(scope, "last-failure-message"), string.Empty);
        return !string.IsNullOrWhiteSpace(message)
            ? message
            : scope == RemoteUpdateScope.FullDatabase
                ? Preferences.Default.Get(LegacyLastRemoteFailureMessagePreferenceKey, string.Empty)
                : string.Empty;
    }

    private static void PersistScopeReceipts(RemoteUpdateScope appliedScope, RemoteContentManifestModel? manifest, RemoteContentPackageModel appliedPackage, DateTimeOffset appliedAtUtc)
    {
        IReadOnlyList<RemoteContentPackageModel> packagesToPersist = manifest?.Packages
            .Where(package => ShouldPersistPackageReceipt(appliedScope, package))
            .ToArray()
            ?? [appliedPackage];

        foreach (RemoteContentPackageModel package in packagesToPersist)
        {
            PersistLastSuccess(RemoteUpdateScope.ForReceipt(package.ContentAreaKey, package.SliceKey, package.PackageType), package, appliedAtUtc);
        }
    }

    private static bool ShouldPersistPackageReceipt(RemoteUpdateScope appliedScope, RemoteContentPackageModel package) =>
        appliedScope == RemoteUpdateScope.FullDatabase
            ? true
            : appliedScope.ReplaceMode == ReplaceMode.FullDatabase
                ? string.Equals(package.ContentAreaKey, appliedScope.ContentAreaKey, StringComparison.OrdinalIgnoreCase)
                : appliedScope.Matches(package);

    private static void PersistLastSuccess(RemoteUpdateScope scope, RemoteContentPackageModel package, DateTimeOffset appliedAtUtc)
    {
        Preferences.Default.Set(BuildPreferenceKey(scope, "package-id"), package.PackageId);
        Preferences.Default.Set(BuildPreferenceKey(scope, "package-version"), package.Version);
        Preferences.Default.Set(BuildPreferenceKey(scope, "checksum"), package.Checksum);
        Preferences.Default.Set(BuildPreferenceKey(scope, "schema-version"), package.SchemaVersion);
        Preferences.Default.Set(BuildPreferenceKey(scope, "last-success-at-utc"), appliedAtUtc.ToString("O"));
        Preferences.Default.Remove(BuildPreferenceKey(scope, "last-failure-message"));

        if (scope == RemoteUpdateScope.FullDatabase)
        {
            Preferences.Default.Set(LegacyLastRemotePackageIdPreferenceKey, package.PackageId);
            Preferences.Default.Set(LegacyLastRemotePackageVersionPreferenceKey, package.Version);
            Preferences.Default.Set(LegacyLastRemoteSuccessAtPreferenceKey, appliedAtUtc.ToString("O"));
            Preferences.Default.Remove(LegacyLastRemoteFailureMessagePreferenceKey);
        }
    }

    private static void PersistLastFailure(RemoteUpdateScope scope, string message)
    {
        string preferenceKey = BuildPreferenceKey(scope, "last-failure-message");
        if (string.IsNullOrWhiteSpace(message))
        {
            Preferences.Default.Remove(preferenceKey);
            if (scope == RemoteUpdateScope.FullDatabase) { Preferences.Default.Remove(LegacyLastRemoteFailureMessagePreferenceKey); }
            return;
        }

        Preferences.Default.Set(preferenceKey, message);
        if (scope == RemoteUpdateScope.FullDatabase) { Preferences.Default.Set(LegacyLastRemoteFailureMessagePreferenceKey, message); }
    }

    private static string BuildPreferenceKey(RemoteUpdateScope scope, string suffix) => $"remote-content-{scope.ScopeKey}-{suffix}";

    private enum ReplaceMode { FullDatabase, CefrLevel }

    private sealed record RemoteUpdateScope(string ScopeKey, string ContentAreaKey, string SliceKey, string PackageType, ReplaceMode ReplaceMode, string CefrLevel)
    {
        public static readonly RemoteUpdateScope FullDatabase = new("all-full", "all", "full", "full-database", ReplaceMode.FullDatabase, string.Empty);

        public static RemoteUpdateScope ForArea(string areaKey)
        {
            string normalizedAreaKey = areaKey.Trim().ToLowerInvariant();
            return new RemoteUpdateScope($"{normalizedAreaKey}-full", normalizedAreaKey, "full", $"full-{normalizedAreaKey}", ReplaceMode.FullDatabase, string.Empty);
        }

        public static RemoteUpdateScope ForCefrLevel(string cefrLevel)
        {
            string normalizedLevel = cefrLevel.Trim().ToLowerInvariant();
            return new RemoteUpdateScope($"catalog-cefr-{normalizedLevel}", "catalog", $"cefr:{normalizedLevel}", "catalog-cefr", ReplaceMode.CefrLevel, normalizedLevel.ToUpperInvariant());
        }

        public static RemoteUpdateScope ForReceipt(string contentAreaKey, string sliceKey, string packageType) =>
            string.Equals(packageType, "full-database", StringComparison.OrdinalIgnoreCase)
                ? FullDatabase
                : sliceKey.StartsWith("cefr:", StringComparison.OrdinalIgnoreCase)
                    ? ForCefrLevel(sliceKey["cefr:".Length..])
                    : ForArea(contentAreaKey);

        public bool Matches(RemoteContentPackageModel package) =>
            string.Equals(package.ContentAreaKey, ContentAreaKey, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(package.SliceKey, SliceKey, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(package.PackageType, PackageType, StringComparison.OrdinalIgnoreCase);

        public string BuildManifestUri(RemoteContentUpdateOptions remoteOptions)
        {
            string baseUrl = remoteOptions.BaseUrl.TrimEnd('/');
            string clientProductKey = Uri.EscapeDataString(remoteOptions.ClientProductKey);
            if (this == FullDatabase) { return $"{baseUrl}/api/mobile/content/manifest?clientProductKey={clientProductKey}"; }
            if (ReplaceMode == ReplaceMode.CefrLevel) { return $"{baseUrl}/api/mobile/content/areas/catalog/cefr/{Uri.EscapeDataString(CefrLevel)}/manifest?clientProductKey={clientProductKey}"; }
            return $"{baseUrl}/api/mobile/content/areas/{Uri.EscapeDataString(ContentAreaKey)}/manifest?clientProductKey={clientProductKey}";
        }

        public string BuildDownloadUri(RemoteContentUpdateOptions remoteOptions, string packageId)
        {
            string baseUrl = remoteOptions.BaseUrl.TrimEnd('/');
            string clientProductKey = Uri.EscapeDataString(remoteOptions.ClientProductKey);
            return $"{baseUrl}/api/mobile/content/packages/{Uri.EscapeDataString(packageId)}/download?clientProductKey={clientProductKey}&clientSchemaVersion={remoteOptions.ClientSchemaVersion}";
        }
    }

    private const string FullReplaceScript =
        """
        PRAGMA foreign_keys = OFF;

        DELETE FROM ExampleTranslations;
        DELETE FROM ExampleSentences;
        DELETE FROM SenseTranslations;
        DELETE FROM WordSenses;
        DELETE FROM WordTopics;
        DELETE FROM WordLabels;
        DELETE FROM WordGrammarNotes;
        DELETE FROM WordCollocations;
        DELETE FROM WordFamilyMembers;
        DELETE FROM WordRelations;
        DELETE FROM ContentPackageEntries;
        DELETE FROM ContentPackages;
        DELETE FROM WordEntries;
        DELETE FROM TopicLocalizations;
        DELETE FROM Topics;
        DELETE FROM Languages;

        INSERT INTO Languages SELECT * FROM remote.Languages;
        INSERT INTO Topics SELECT * FROM remote.Topics;
        INSERT INTO TopicLocalizations SELECT * FROM remote.TopicLocalizations;
        INSERT INTO WordEntries SELECT * FROM remote.WordEntries;
        INSERT INTO WordSenses SELECT * FROM remote.WordSenses;
        INSERT INTO SenseTranslations SELECT * FROM remote.SenseTranslations;
        INSERT INTO ExampleSentences SELECT * FROM remote.ExampleSentences;
        INSERT INTO ExampleTranslations SELECT * FROM remote.ExampleTranslations;
        INSERT INTO WordTopics SELECT * FROM remote.WordTopics;
        INSERT INTO WordLabels SELECT * FROM remote.WordLabels;
        INSERT INTO WordGrammarNotes SELECT * FROM remote.WordGrammarNotes;
        INSERT INTO WordCollocations SELECT * FROM remote.WordCollocations;
        INSERT INTO WordFamilyMembers SELECT * FROM remote.WordFamilyMembers;
        INSERT INTO WordRelations SELECT * FROM remote.WordRelations;
        INSERT INTO ContentPackages SELECT * FROM remote.ContentPackages;
        INSERT INTO ContentPackageEntries SELECT * FROM remote.ContentPackageEntries;

        PRAGMA foreign_keys = ON;
        """;

    private const string CefrReplaceScript =
        """
        INSERT INTO Languages (Id, Code, EnglishName, NativeName, IsActive, SupportsMeanings, SupportsUserInterface)
        SELECT Id, Code, EnglishName, NativeName, IsActive, SupportsMeanings, SupportsUserInterface
        FROM remote.Languages
        ON CONFLICT(Id) DO UPDATE SET
            Code = excluded.Code,
            EnglishName = excluded.EnglishName,
            NativeName = excluded.NativeName,
            IsActive = excluded.IsActive,
            SupportsMeanings = excluded.SupportsMeanings,
            SupportsUserInterface = excluded.SupportsUserInterface;

        INSERT INTO Topics (Id, Key, IsSystem, SortOrder, CreatedAtUtc, UpdatedAtUtc)
        SELECT Id, Key, IsSystem, SortOrder, CreatedAtUtc, UpdatedAtUtc
        FROM remote.Topics
        ON CONFLICT(Id) DO UPDATE SET
            Key = excluded.Key,
            IsSystem = excluded.IsSystem,
            SortOrder = excluded.SortOrder,
            CreatedAtUtc = excluded.CreatedAtUtc,
            UpdatedAtUtc = excluded.UpdatedAtUtc;

        INSERT INTO TopicLocalizations (Id, TopicId, LanguageCode, DisplayName, CreatedAtUtc, UpdatedAtUtc)
        SELECT Id, TopicId, LanguageCode, DisplayName, CreatedAtUtc, UpdatedAtUtc
        FROM remote.TopicLocalizations
        ON CONFLICT(Id) DO UPDATE SET
            TopicId = excluded.TopicId,
            LanguageCode = excluded.LanguageCode,
            DisplayName = excluded.DisplayName,
            CreatedAtUtc = excluded.CreatedAtUtc,
            UpdatedAtUtc = excluded.UpdatedAtUtc;

        DELETE FROM WordEntries WHERE PrimaryCefrLevel = $cefrLevel;

        INSERT INTO WordEntries SELECT * FROM remote.WordEntries;
        INSERT INTO WordSenses SELECT * FROM remote.WordSenses;
        INSERT INTO SenseTranslations SELECT * FROM remote.SenseTranslations;
        INSERT INTO ExampleSentences SELECT * FROM remote.ExampleSentences;
        INSERT INTO ExampleTranslations SELECT * FROM remote.ExampleTranslations;
        INSERT INTO WordTopics SELECT * FROM remote.WordTopics;
        INSERT INTO WordLabels SELECT * FROM remote.WordLabels;
        INSERT INTO WordGrammarNotes SELECT * FROM remote.WordGrammarNotes;
        INSERT INTO WordCollocations SELECT * FROM remote.WordCollocations;
        INSERT INTO WordFamilyMembers SELECT * FROM remote.WordFamilyMembers;
        INSERT INTO WordRelations SELECT * FROM remote.WordRelations;
        INSERT INTO ContentPackages SELECT * FROM remote.ContentPackages;
        INSERT INTO ContentPackageEntries SELECT * FROM remote.ContentPackageEntries;
        """;
}
