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

/// <summary>
/// Implements the first mobile client path for server-backed full-content updates.
/// </summary>
internal sealed class RemoteContentUpdateService(
    HttpClient httpClient,
    RemoteContentUpdateOptions options) : IRemoteContentUpdateService
{
    private const string LastRemotePackageIdPreferenceKey = "remote-content-last-package-id";
    private const string LastRemotePackageVersionPreferenceKey = "remote-content-last-package-version";
    private const string LastRemoteSuccessAtPreferenceKey = "remote-content-last-success-at-utc";
    private const string LastRemoteFailureMessagePreferenceKey = "remote-content-last-failure-message";

    public async Task<RemoteContentUpdateStatus> GetUpdateStatusAsync(string databasePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!IsConfigured())
        {
            return new RemoteContentUpdateStatus(
                false,
                false,
                false,
                GetLastAppliedPackageId(),
                GetLastAppliedVersion(),
                string.Empty,
                string.Empty,
                0,
                GetLastSuccessfulUpdateAtUtc(),
                GetLastFailureMessage());
        }

        try
        {
            RemoteContentManifestModel? manifest = await httpClient
                .GetFromJsonAsync<RemoteContentManifestModel>(
                    BuildManifestUri(),
                    cancellationToken)
                .ConfigureAwait(false);

            RemoteContentPackageModel? remotePackage = SelectLatestFullPackage(manifest);
            if (remotePackage is null)
            {
                return new RemoteContentUpdateStatus(
                    true,
                    true,
                    false,
                    GetLastAppliedPackageId(),
                    GetLastAppliedVersion(),
                    string.Empty,
                    string.Empty,
                    0,
                    GetLastSuccessfulUpdateAtUtc(),
                    GetLastFailureMessage());
            }

            string localPackageId = await GetLocalFullPackageIdAsync(databasePath, cancellationToken).ConfigureAwait(false);
            bool updateAvailable = !string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase);

            return new RemoteContentUpdateStatus(
                true,
                true,
                updateAvailable,
                string.IsNullOrWhiteSpace(localPackageId) ? GetLastAppliedPackageId() : localPackageId,
                GetLastAppliedVersion(),
                remotePackage.PackageId,
                remotePackage.Version,
                updateAvailable ? remotePackage.WordCount : 0,
                GetLastSuccessfulUpdateAtUtc(),
                GetLastFailureMessage());
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            PersistLastFailure(exception.Message);

            return new RemoteContentUpdateStatus(
                true,
                false,
                false,
                GetLastAppliedPackageId(),
                GetLastAppliedVersion(),
                string.Empty,
                string.Empty,
                0,
                GetLastSuccessfulUpdateAtUtc(),
                GetLastFailureMessage());
        }
    }

    public async Task<RemoteContentUpdateResult> ApplyFullUpdateAsync(string databasePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!IsConfigured())
        {
            return new RemoteContentUpdateResult(false, false, string.Empty, string.Empty, 0, null, "Remote content updates are not configured.");
        }

        string tempDirectory = Path.Combine(Path.GetTempPath(), "darwinlingua-remote-update", Guid.NewGuid().ToString("N"));
        string tempJsonPath = Path.Combine(tempDirectory, "full-update.json");
        string tempDatabasePath = Path.Combine(tempDirectory, "remote-update.db");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            RemoteContentManifestModel? manifest = await httpClient
                .GetFromJsonAsync<RemoteContentManifestModel>(BuildManifestUri(), cancellationToken)
                .ConfigureAwait(false);
            RemoteContentPackageModel remotePackage = SelectLatestFullPackage(manifest)
                ?? throw new InvalidOperationException("The remote manifest does not contain a full-database package.");

            string localPackageId = await GetLocalFullPackageIdAsync(databasePath, cancellationToken).ConfigureAwait(false);
            if (string.Equals(localPackageId, remotePackage.PackageId, StringComparison.OrdinalIgnoreCase))
            {
                PersistLastFailure(string.Empty);
                return new RemoteContentUpdateResult(true, false, remotePackage.PackageId, remotePackage.Version, 0, GetLastSuccessfulUpdateAtUtc(), null);
            }

            await DownloadPackageAsync(remotePackage, tempJsonPath, cancellationToken).ConfigureAwait(false);
            await ImportIntoTemporaryDatabaseAsync(tempJsonPath, tempDatabasePath, cancellationToken).ConfigureAwait(false);

            int importedWords = await ReplaceContentTablesAsync(databasePath, tempDatabasePath, cancellationToken).ConfigureAwait(false);
            DateTimeOffset appliedAtUtc = DateTimeOffset.UtcNow;
            PersistLastSuccess(remotePackage.PackageId, remotePackage.Version, appliedAtUtc);

            return new RemoteContentUpdateResult(true, true, remotePackage.PackageId, remotePackage.Version, importedWords, appliedAtUtc, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
        {
            PersistLastFailure(exception.Message);
            return new RemoteContentUpdateResult(false, false, string.Empty, string.Empty, 0, null, exception.Message);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(options.BaseUrl);
    }

    private string BuildManifestUri()
    {
        return $"{options.BaseUrl.TrimEnd('/')}/api/mobile/content/manifest?clientProductKey={Uri.EscapeDataString(options.ClientProductKey)}";
    }

    private static RemoteContentPackageModel? SelectLatestFullPackage(RemoteContentManifestModel? manifest)
    {
        if (manifest is null)
        {
            return null;
        }

        return manifest.Packages
            .Where(package => string.Equals(package.PackageType, "full-database", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(package => package.CreatedAtUtc)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private async Task DownloadPackageAsync(RemoteContentPackageModel remotePackage, string targetPath, CancellationToken cancellationToken)
    {
        string requestUri =
            $"{options.BaseUrl.TrimEnd('/')}/api/mobile/content/packages/{Uri.EscapeDataString(remotePackage.PackageId)}/download" +
            $"?clientProductKey={Uri.EscapeDataString(options.ClientProductKey)}&clientSchemaVersion={options.ClientSchemaVersion}";

        using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
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

    private static async Task<int> ReplaceContentTablesAsync(string localDatabasePath, string tempDatabasePath, CancellationToken cancellationToken)
    {
        await using SqliteConnection localConnection = new($"Data Source={localDatabasePath}");
        await localConnection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using DbTransaction transaction = await localConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(
            localConnection,
            transaction,
            "ATTACH DATABASE $tempPath AS remote;",
            cancellationToken,
            CreateParameter("$tempPath", tempDatabasePath)).ConfigureAwait(false);

        int importedWords = await ExecuteScalarIntAsync(
            localConnection,
            transaction,
            "SELECT COUNT(*) FROM remote.WordEntries;",
            cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(localConnection, transaction, CreateFullReplaceScript(), cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(localConnection, transaction, "DETACH DATABASE remote;", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return importedWords;
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

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        DbTransaction? transaction,
        string commandText,
        CancellationToken cancellationToken,
        params SqliteParameter[] parameters)
    {
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> ExecuteScalarIntAsync(
        SqliteConnection connection,
        DbTransaction? transaction,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static SqliteParameter CreateParameter(string parameterName, string value)
    {
        return new SqliteParameter(parameterName, value);
    }

    private static string CreateFullReplaceScript()
    {
        return
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

            INSERT INTO Languages
            SELECT *
            FROM remote.Languages;

            INSERT INTO Topics
            SELECT *
            FROM remote.Topics;

            INSERT INTO TopicLocalizations
            SELECT *
            FROM remote.TopicLocalizations;

            INSERT INTO WordEntries
            SELECT *
            FROM remote.WordEntries;

            INSERT INTO WordSenses
            SELECT *
            FROM remote.WordSenses;

            INSERT INTO SenseTranslations
            SELECT *
            FROM remote.SenseTranslations;

            INSERT INTO ExampleSentences
            SELECT *
            FROM remote.ExampleSentences;

            INSERT INTO ExampleTranslations
            SELECT *
            FROM remote.ExampleTranslations;

            INSERT INTO WordTopics
            SELECT *
            FROM remote.WordTopics;

            INSERT INTO WordLabels
            SELECT *
            FROM remote.WordLabels;

            INSERT INTO WordGrammarNotes
            SELECT *
            FROM remote.WordGrammarNotes;

            INSERT INTO WordCollocations
            SELECT *
            FROM remote.WordCollocations;

            INSERT INTO WordFamilyMembers
            SELECT *
            FROM remote.WordFamilyMembers;

            INSERT INTO WordRelations
            SELECT *
            FROM remote.WordRelations;

            INSERT INTO ContentPackages
            SELECT *
            FROM remote.ContentPackages;

            INSERT INTO ContentPackageEntries
            SELECT *
            FROM remote.ContentPackageEntries;

            PRAGMA foreign_keys = ON;
            """;
    }

    private static void TryDeleteDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static DateTimeOffset? GetLastSuccessfulUpdateAtUtc()
    {
        string? rawValue = Preferences.Default.Get<string?>(LastRemoteSuccessAtPreferenceKey, null);
        return DateTimeOffset.TryParse(rawValue, out DateTimeOffset parsedValue) ? parsedValue : null;
    }

    private static string GetLastAppliedPackageId()
    {
        return Preferences.Default.Get(LastRemotePackageIdPreferenceKey, string.Empty);
    }

    private static string GetLastAppliedVersion()
    {
        return Preferences.Default.Get(LastRemotePackageVersionPreferenceKey, string.Empty);
    }

    private static string GetLastFailureMessage()
    {
        return Preferences.Default.Get(LastRemoteFailureMessagePreferenceKey, string.Empty);
    }

    private static void PersistLastSuccess(string packageId, string version, DateTimeOffset appliedAtUtc)
    {
        Preferences.Default.Set(LastRemotePackageIdPreferenceKey, packageId);
        Preferences.Default.Set(LastRemotePackageVersionPreferenceKey, version);
        Preferences.Default.Set(LastRemoteSuccessAtPreferenceKey, appliedAtUtc.ToString("O"));
        Preferences.Default.Remove(LastRemoteFailureMessagePreferenceKey);
    }

    private static void PersistLastFailure(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Preferences.Default.Remove(LastRemoteFailureMessagePreferenceKey);
            return;
        }

        Preferences.Default.Set(LastRemoteFailureMessagePreferenceKey, message);
    }
}
