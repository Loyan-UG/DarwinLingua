using System.Security.Cryptography;
using System.Text;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace DarwinDeutsch.Maui.Services.Storage;

/// <summary>
/// Provides the initial local SQLite database and merges packaged seed updates into the app sandbox.
/// </summary>
internal sealed class SeedDatabaseProvisioningService : ISeedDatabaseProvisioningService
{
    private const string SeedDatabaseAssetName = "darwin-lingua.seed.db";
    private const string SeedSignaturePreferenceKey = "seed-database-applied-signature";
    private const string SeedLastAppliedAtPreferenceKey = "seed-database-last-applied-at-utc";
    private const string SeedLastAppliedPackageCountPreferenceKey = "seed-database-last-applied-package-count";
    private const string SeedLastAppliedWordCountPreferenceKey = "seed-database-last-applied-word-count";

    /// <inheritdoc />
    public async Task EnsureSeedDatabaseAsync(string databasePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        string? databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        await using SeedAssetSnapshot? seedAssetSnapshot = await LoadSeedAssetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (seedAssetSnapshot is null)
        {
            return;
        }

        if (!File.Exists(databasePath))
        {
            await using FileStream localDatabaseStream = File.Create(databasePath);
            seedAssetSnapshot.Stream.Position = 0;
            await seedAssetSnapshot.Stream.CopyToAsync(localDatabaseStream, cancellationToken).ConfigureAwait(false);

            int importedPackages = await CountRowsAsync(databasePath, "ContentPackages", cancellationToken).ConfigureAwait(false);
            int importedWords = await CountRowsAsync(databasePath, "WordEntries", cancellationToken).ConfigureAwait(false);
            DateTimeOffset appliedAtUtc = DateTimeOffset.UtcNow;
            PersistAppliedSeedMetadata(seedAssetSnapshot.Signature, appliedAtUtc, importedPackages, importedWords);
            return;
        }

        if (!Preferences.Default.ContainsKey(SeedSignaturePreferenceKey))
        {
            Preferences.Default.Set(SeedSignaturePreferenceKey, seedAssetSnapshot.Signature);
        }
    }

    /// <inheritdoc />
    public async Task<SeedDatabaseUpdateStatus> GetUpdateStatusAsync(string databasePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        await using SeedAssetSnapshot? seedAssetSnapshot = await LoadSeedAssetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (seedAssetSnapshot is null)
        {
            return new SeedDatabaseUpdateStatus(false, false, 0, 0, string.Empty, GetAppliedSeedSignature(), GetLastAppliedAtUtc(), GetLastAppliedPackageCount(), GetLastAppliedWordCount());
        }

        string? appliedSignature = Preferences.Default.Get<string?>(SeedSignaturePreferenceKey, null);
        if (!File.Exists(databasePath))
        {
            int seedPackages = await CountRowsInStreamAsync(seedAssetSnapshot.Stream, "ContentPackages", cancellationToken).ConfigureAwait(false);
            int seedWords = await CountRowsInStreamAsync(seedAssetSnapshot.Stream, "WordEntries", cancellationToken).ConfigureAwait(false);

            return new SeedDatabaseUpdateStatus(
                true,
                true,
                seedPackages,
                seedWords,
                seedAssetSnapshot.Signature,
                GetAppliedSeedSignature(),
                GetLastAppliedAtUtc(),
                GetLastAppliedPackageCount(),
                GetLastAppliedWordCount());
        }

        if (string.Equals(appliedSignature, seedAssetSnapshot.Signature, StringComparison.Ordinal))
        {
            return new SeedDatabaseUpdateStatus(
                true,
                false,
                0,
                0,
                seedAssetSnapshot.Signature,
                GetAppliedSeedSignature(),
                GetLastAppliedAtUtc(),
                GetLastAppliedPackageCount(),
                GetLastAppliedWordCount());
        }

        string temporarySeedPath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-seed-status-{Guid.NewGuid():N}.db");

        try
        {
            seedAssetSnapshot.Stream.Position = 0;
            await using (FileStream tempSeedStream = File.Create(temporarySeedPath))
            {
                await seedAssetSnapshot.Stream.CopyToAsync(tempSeedStream, cancellationToken).ConfigureAwait(false);
            }

            (int pendingPackages, int pendingWords) = await CountPendingSeedUpdatesAsync(
                databasePath,
                temporarySeedPath,
                cancellationToken).ConfigureAwait(false);

            return new SeedDatabaseUpdateStatus(
                true,
                pendingPackages > 0,
                pendingPackages,
                pendingWords,
                seedAssetSnapshot.Signature,
                GetAppliedSeedSignature(),
                GetLastAppliedAtUtc(),
                GetLastAppliedPackageCount(),
                GetLastAppliedWordCount());
        }
        finally
        {
            TryDeleteFile(temporarySeedPath);
        }
    }

    /// <inheritdoc />
    public async Task<SeedDatabaseUpdateResult> ApplySeedUpdateAsync(string databasePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        await using SeedAssetSnapshot? seedAssetSnapshot = await LoadSeedAssetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (seedAssetSnapshot is null)
        {
            return new SeedDatabaseUpdateResult(false, false, 0, 0, string.Empty, null, "The packaged seed database is not available.");
        }

        if (!File.Exists(databasePath))
        {
            await EnsureSeedDatabaseAsync(databasePath, cancellationToken).ConfigureAwait(false);

            int importedPackages = await CountRowsAsync(databasePath, "ContentPackages", cancellationToken).ConfigureAwait(false);
            int importedWords = await CountRowsAsync(databasePath, "WordEntries", cancellationToken).ConfigureAwait(false);
            DateTimeOffset appliedAtUtc = GetLastAppliedAtUtc() ?? DateTimeOffset.UtcNow;

            return new SeedDatabaseUpdateResult(true, true, importedPackages, importedWords, seedAssetSnapshot.Signature, appliedAtUtc, null);
        }

        string? appliedSignature = Preferences.Default.Get<string?>(SeedSignaturePreferenceKey, null);
        if (string.Equals(appliedSignature, seedAssetSnapshot.Signature, StringComparison.Ordinal))
        {
            return new SeedDatabaseUpdateResult(
                true,
                false,
                0,
                0,
                seedAssetSnapshot.Signature,
                GetLastAppliedAtUtc(),
                null);
        }

        string temporarySeedPath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-seed-{Guid.NewGuid():N}.db");

        try
        {
            seedAssetSnapshot.Stream.Position = 0;
            await using (FileStream tempSeedStream = File.Create(temporarySeedPath))
            {
                await seedAssetSnapshot.Stream.CopyToAsync(tempSeedStream, cancellationToken).ConfigureAwait(false);
            }

            await using SqliteConnection localConnection = new($"Data Source={databasePath}");
            await localConnection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using DbTransaction transaction = await localConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            await ExecuteNonQueryAsync(
                localConnection,
                transaction,
                "ATTACH DATABASE $seedPath AS seed;",
                cancellationToken,
                CreateParameter("$seedPath", temporarySeedPath)).ConfigureAwait(false);

            int importedPackages = await ExecuteScalarIntAsync(
                localConnection,
                transaction,
                """
                SELECT COUNT(*)
                FROM seed.ContentPackages AS seedPackages
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ContentPackages AS localPackages
                    WHERE localPackages.PackageId = seedPackages.PackageId);
                """,
                cancellationToken).ConfigureAwait(false);

            int importedWords = await ExecuteScalarIntAsync(
                localConnection,
                transaction,
                """
                SELECT COUNT(DISTINCT seedEntries.ImportedWordEntryPublicId)
                FROM seed.ContentPackageEntries AS seedEntries
                INNER JOIN seed.ContentPackages AS seedPackages
                    ON seedPackages.Id = seedEntries.ContentPackageId
                WHERE seedEntries.ImportedWordEntryPublicId IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM ContentPackages AS localPackages
                      WHERE localPackages.PackageId = seedPackages.PackageId);
                """,
                cancellationToken).ConfigureAwait(false);

            if (importedPackages > 0)
            {
                await ExecuteNonQueryAsync(localConnection, transaction, CreateSeedMergeScript(), cancellationToken).ConfigureAwait(false);
            }

            await ExecuteNonQueryAsync(localConnection, transaction, "DETACH DATABASE seed;", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            DateTimeOffset appliedAtUtc = DateTimeOffset.UtcNow;
            PersistAppliedSeedMetadata(seedAssetSnapshot.Signature, appliedAtUtc, importedPackages, importedWords);

            return new SeedDatabaseUpdateResult(
                true,
                importedPackages > 0,
                importedPackages,
                importedWords,
                seedAssetSnapshot.Signature,
                appliedAtUtc,
                null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SqliteException)
        {
            return new SeedDatabaseUpdateResult(false, false, 0, 0, seedAssetSnapshot.Signature, null, exception.Message);
        }
        finally
        {
            TryDeleteFile(temporarySeedPath);
        }
    }

    private static async Task<SeedAssetSnapshot?> LoadSeedAssetSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            Stream packagedSeedStream = await FileSystem.Current
                .OpenAppPackageFileAsync(SeedDatabaseAssetName)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            MemoryStream memoryStream = new();
            await using (packagedSeedStream)
            {
                await packagedSeedStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            }

            memoryStream.Position = 0;
            string signature = Convert.ToHexString(SHA256.HashData(memoryStream.ToArray())).ToLowerInvariant();
            memoryStream.Position = 0;

            return new SeedAssetSnapshot(memoryStream, signature);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private static async Task<int> CountRowsAsync(string databasePath, string tableName, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath}");
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await ExecuteScalarIntAsync(
            connection,
            null,
            $"SELECT COUNT(*) FROM {tableName};",
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> CountRowsInStreamAsync(
        MemoryStream seedStream,
        string tableName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(seedStream);

        string temporarySeedPath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-seed-count-{Guid.NewGuid():N}.db");

        try
        {
            seedStream.Position = 0;
            await using (FileStream tempSeedStream = File.Create(temporarySeedPath))
            {
                await seedStream.CopyToAsync(tempSeedStream, cancellationToken).ConfigureAwait(false);
            }

            return await CountRowsAsync(temporarySeedPath, tableName, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            TryDeleteFile(temporarySeedPath);
        }
    }

    private static async Task<(int PendingPackages, int PendingWords)> CountPendingSeedUpdatesAsync(
        string databasePath,
        string seedPath,
        CancellationToken cancellationToken)
    {
        await using SqliteConnection localConnection = new($"Data Source={databasePath}");
        await localConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction = await localConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(
            localConnection,
            transaction,
            "ATTACH DATABASE $seedPath AS seed;",
            cancellationToken,
            CreateParameter("$seedPath", seedPath)).ConfigureAwait(false);

        int pendingPackages = await ExecuteScalarIntAsync(
            localConnection,
            transaction,
            """
            SELECT COUNT(*)
            FROM seed.ContentPackages AS seedPackages
            WHERE NOT EXISTS (
                SELECT 1
                FROM ContentPackages AS localPackages
                WHERE localPackages.PackageId = seedPackages.PackageId);
            """,
            cancellationToken).ConfigureAwait(false);

        int pendingWords = await ExecuteScalarIntAsync(
            localConnection,
            transaction,
            """
            SELECT COUNT(DISTINCT seedEntries.ImportedWordEntryPublicId)
            FROM seed.ContentPackageEntries AS seedEntries
            INNER JOIN seed.ContentPackages AS seedPackages
                ON seedPackages.Id = seedEntries.ContentPackageId
            WHERE seedEntries.ImportedWordEntryPublicId IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1
                  FROM ContentPackages AS localPackages
                  WHERE localPackages.PackageId = seedPackages.PackageId);
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(localConnection, transaction, "DETACH DATABASE seed;", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return (pendingPackages, pendingWords);
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
        CancellationToken cancellationToken,
        params SqliteParameter[] parameters)
    {
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.Parameters.AddRange(parameters);

        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    private static SqliteParameter CreateParameter(string name, object value)
    {
        return new SqliteParameter(name, value);
    }

    private static void PersistAppliedSeedMetadata(string signature, DateTimeOffset appliedAtUtc, int packageCount, int wordCount)
    {
        Preferences.Default.Set(SeedSignaturePreferenceKey, signature);
        Preferences.Default.Set(SeedLastAppliedAtPreferenceKey, appliedAtUtc.UtcDateTime.ToString("O"));
        Preferences.Default.Set(SeedLastAppliedPackageCountPreferenceKey, packageCount);
        Preferences.Default.Set(SeedLastAppliedWordCountPreferenceKey, wordCount);
    }

    private static string GetAppliedSeedSignature()
    {
        return Preferences.Default.Get(SeedSignaturePreferenceKey, string.Empty);
    }

    private static DateTimeOffset? GetLastAppliedAtUtc()
    {
        string? persistedValue = Preferences.Default.Get<string?>(SeedLastAppliedAtPreferenceKey, null);
        return DateTimeOffset.TryParse(persistedValue, out DateTimeOffset parsedValue)
            ? parsedValue
            : null;
    }

    private static int GetLastAppliedPackageCount()
    {
        return Preferences.Default.Get(SeedLastAppliedPackageCountPreferenceKey, 0);
    }

    private static int GetLastAppliedWordCount()
    {
        return Preferences.Default.Get(SeedLastAppliedWordCountPreferenceKey, 0);
    }

    private static string CreateSeedMergeScript()
    {
        StringBuilder scriptBuilder = new();

        scriptBuilder.AppendLine(
            """
            CREATE TEMP TABLE IF NOT EXISTS NewSeedPackages AS
            SELECT seedPackages.Id,
                   seedPackages.PackageId
            FROM seed.ContentPackages AS seedPackages
            WHERE NOT EXISTS (
                SELECT 1
                FROM ContentPackages AS localPackages
                WHERE localPackages.PackageId = seedPackages.PackageId);

            CREATE TEMP TABLE IF NOT EXISTS NewSeedWordPublicIds AS
            SELECT DISTINCT seedEntries.ImportedWordEntryPublicId AS PublicId
            FROM seed.ContentPackageEntries AS seedEntries
            WHERE seedEntries.ContentPackageId IN (SELECT Id FROM NewSeedPackages)
              AND seedEntries.ImportedWordEntryPublicId IS NOT NULL;

            INSERT INTO Languages
            SELECT *
            FROM seed.Languages AS seedLanguages
            WHERE NOT EXISTS (
                SELECT 1
                FROM Languages AS localLanguages
                WHERE localLanguages.Code = seedLanguages.Code);

            INSERT INTO Topics
            SELECT *
            FROM seed.Topics AS seedTopics
            WHERE NOT EXISTS (
                SELECT 1
                FROM Topics AS localTopics
                WHERE localTopics.[Key] = seedTopics.[Key]);

            INSERT INTO TopicLocalizations
            SELECT *
            FROM seed.TopicLocalizations AS seedTopicLocalizations
            WHERE NOT EXISTS (
                SELECT 1
                FROM TopicLocalizations AS localTopicLocalizations
                WHERE localTopicLocalizations.TopicId = seedTopicLocalizations.TopicId
                  AND localTopicLocalizations.LanguageCode = seedTopicLocalizations.LanguageCode);

            INSERT INTO ContentPackages
            SELECT *
            FROM seed.ContentPackages AS seedPackages
            WHERE seedPackages.Id IN (SELECT Id FROM NewSeedPackages);

            INSERT INTO WordEntries
            SELECT seedWords.*
            FROM seed.WordEntries AS seedWords
            INNER JOIN NewSeedWordPublicIds AS selectedWords
                ON selectedWords.PublicId = seedWords.PublicId
            WHERE NOT EXISTS (
                SELECT 1
                FROM WordEntries AS localWords
                WHERE localWords.PublicId = seedWords.PublicId);

            INSERT INTO WordSenses
            SELECT seedSenses.*
            FROM seed.WordSenses AS seedSenses
            WHERE seedSenses.WordEntryId IN (
                SELECT localWords.Id
                FROM WordEntries AS localWords
                WHERE localWords.PublicId IN (SELECT PublicId FROM NewSeedWordPublicIds))
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordSenses AS localSenses
                  WHERE localSenses.Id = seedSenses.Id);

            INSERT INTO SenseTranslations
            SELECT seedTranslations.*
            FROM seed.SenseTranslations AS seedTranslations
            WHERE seedTranslations.WordSenseId IN (
                SELECT seedSenses.Id
                FROM seed.WordSenses AS seedSenses
                WHERE seedSenses.WordEntryId IN (
                    SELECT seedWords.Id
                    FROM seed.WordEntries AS seedWords
                    INNER JOIN NewSeedWordPublicIds AS selectedWords
                        ON selectedWords.PublicId = seedWords.PublicId))
              AND NOT EXISTS (
                  SELECT 1
                  FROM SenseTranslations AS localTranslations
                  WHERE localTranslations.Id = seedTranslations.Id);

            INSERT INTO ExampleSentences
            SELECT seedExamples.*
            FROM seed.ExampleSentences AS seedExamples
            WHERE seedExamples.WordSenseId IN (
                SELECT seedSenses.Id
                FROM seed.WordSenses AS seedSenses
                WHERE seedSenses.WordEntryId IN (
                    SELECT seedWords.Id
                    FROM seed.WordEntries AS seedWords
                    INNER JOIN NewSeedWordPublicIds AS selectedWords
                        ON selectedWords.PublicId = seedWords.PublicId))
              AND NOT EXISTS (
                  SELECT 1
                  FROM ExampleSentences AS localExamples
                  WHERE localExamples.Id = seedExamples.Id);

            INSERT INTO ExampleTranslations
            SELECT seedExampleTranslations.*
            FROM seed.ExampleTranslations AS seedExampleTranslations
            WHERE seedExampleTranslations.ExampleSentenceId IN (
                SELECT seedExamples.Id
                FROM seed.ExampleSentences AS seedExamples
                WHERE seedExamples.WordSenseId IN (
                    SELECT seedSenses.Id
                    FROM seed.WordSenses AS seedSenses
                    WHERE seedSenses.WordEntryId IN (
                        SELECT seedWords.Id
                        FROM seed.WordEntries AS seedWords
                        INNER JOIN NewSeedWordPublicIds AS selectedWords
                            ON selectedWords.PublicId = seedWords.PublicId)))
              AND NOT EXISTS (
                  SELECT 1
                  FROM ExampleTranslations AS localExampleTranslations
                  WHERE localExampleTranslations.Id = seedExampleTranslations.Id);

            INSERT INTO WordTopics
            SELECT seedWordTopics.*
            FROM seed.WordTopics AS seedWordTopics
            WHERE seedWordTopics.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordTopics AS localWordTopics
                  WHERE localWordTopics.Id = seedWordTopics.Id);

            INSERT INTO WordLabels
            SELECT seedLabels.*
            FROM seed.WordLabels AS seedLabels
            WHERE seedLabels.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordLabels AS localLabels
                  WHERE localLabels.Id = seedLabels.Id);

            INSERT INTO WordGrammarNotes
            SELECT seedGrammarNotes.*
            FROM seed.WordGrammarNotes AS seedGrammarNotes
            WHERE seedGrammarNotes.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordGrammarNotes AS localGrammarNotes
                  WHERE localGrammarNotes.Id = seedGrammarNotes.Id);

            INSERT INTO WordCollocations
            SELECT seedCollocations.*
            FROM seed.WordCollocations AS seedCollocations
            WHERE seedCollocations.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordCollocations AS localCollocations
                  WHERE localCollocations.Id = seedCollocations.Id);

            INSERT INTO WordFamilyMembers
            SELECT seedFamilyMembers.*
            FROM seed.WordFamilyMembers AS seedFamilyMembers
            WHERE seedFamilyMembers.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordFamilyMembers AS localFamilyMembers
                  WHERE localFamilyMembers.Id = seedFamilyMembers.Id);

            INSERT INTO WordRelations
            SELECT seedRelations.*
            FROM seed.WordRelations AS seedRelations
            WHERE seedRelations.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordRelations AS localRelations
                  WHERE localRelations.Id = seedRelations.Id);

            INSERT INTO ContentPackageEntries
            SELECT seedEntries.*
            FROM seed.ContentPackageEntries AS seedEntries
            WHERE seedEntries.ContentPackageId IN (SELECT Id FROM NewSeedPackages)
              AND NOT EXISTS (
                  SELECT 1
                  FROM ContentPackageEntries AS localEntries
                  WHERE localEntries.Id = seedEntries.Id);

            DROP TABLE IF EXISTS NewSeedWordPublicIds;
            DROP TABLE IF EXISTS NewSeedPackages;
            """);

        return scriptBuilder.ToString();
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class SeedAssetSnapshot : IAsyncDisposable
    {
        public SeedAssetSnapshot(MemoryStream stream, string signature)
        {
            Stream = stream;
            Signature = signature;
        }

        public MemoryStream Stream { get; }

        public string Signature { get; }

        public ValueTask DisposeAsync()
        {
            Stream.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
