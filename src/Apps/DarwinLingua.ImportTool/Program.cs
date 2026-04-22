using System.Text.Json;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Infrastructure.DependencyInjection;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.Practice.Application.DependencyInjection;
using DarwinLingua.Practice.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DarwinLingua.ImportTool;

internal static class Program
{
    private const string DefaultContentRoot = @"D:\_Projects\DarwinLingua.Content";
    private const string SeedDatabaseRelativePath = @"src\Apps\DarwinDeutsch.Maui\Resources\Raw\darwin-lingua.seed.db";
    private const string WebApiSettingsRelativePath = @"src\Apps\DarwinLingua.WebApi\appsettings.Development.json";
    private const string SharedCatalogConnectionStringEnvironmentVariable = "DARWINLINGUA_SHARED_CATALOG_ADMIN";

    private static async Task Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        try
        {
            ImportExecutionOptions executionOptions = ResolveExecutionOptions(args);

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            ConfigureLogging(builder);
            RegisterModules(builder.Services, executionOptions);

            using IHost host = builder.Build();

            IDatabaseInitializer databaseInitializer = host.Services.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

            PrintBanner(executionOptions);

            ImportSessionInput sessionInput = ResolveSessionInput(executionOptions.PathArguments);
            if (sessionInput.IsInteractive)
            {
                sessionInput = RunInteractiveSession(sessionInput);
            }

            if (sessionInput.JsonFilePaths.Count == 0)
            {
                Console.WriteLine("No JSON files were found in the selected folder.");
                await host.StopAsync().ConfigureAwait(false);
                return;
            }

            Console.WriteLine($"Content area: {sessionInput.ContentAreaLabel}");
            Console.WriteLine($"Source folder: {sessionInput.SourceDirectory}");
            Console.WriteLine($"JSON files found: {sessionInput.JsonFilePaths.Count}");
            Console.WriteLine();

            if (!executionOptions.SkipConfirmation && !ConfirmImport())
            {
                Console.WriteLine("Import cancelled.");
                await host.StopAsync().ConfigureAwait(false);
                return;
            }

            IContentImportService contentImportService = host.Services.GetRequiredService<IContentImportService>();
            BatchImportSummary summary = await ImportFilesAsync(
                contentImportService,
                sessionInput.JsonFilePaths,
                CancellationToken.None).ConfigureAwait(false);

            PrintSummary(summary);

            await host.StopAsync().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is DirectoryNotFoundException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Console.Error.WriteLine($"Import failed: {exception.Message}");
        }
    }

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        builder.Logging.AddFilter("DarwinLingua.ImportTool", LogLevel.Information);
    }

    private static void RegisterModules(IServiceCollection services, ImportExecutionOptions executionOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(executionOptions);

        if (executionOptions.Target == ImportTarget.Seed)
        {
            services.AddDarwinLinguaInfrastructure(options => options.DatabasePath = executionOptions.ConnectionOrPath);
        }
        else
        {
            services.AddDarwinLinguaInfrastructureForPostgres(executionOptions.ConnectionOrPath);
        }

        services
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLearningApplication()
            .AddLearningInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure()
            .AddPracticeApplication()
            .AddPracticeInfrastructure();
    }

    private static ImportExecutionOptions ResolveExecutionOptions(string[] args)
    {
        ImportTarget target = ImportTarget.Seed;
        string? explicitConnectionString = null;
        bool skipConfirmation = false;
        List<string> pathArguments = [];

        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];

            if (string.Equals(argument, "--target", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                if (index >= args.Length)
                {
                    throw new InvalidOperationException("The --target option requires a value of 'seed' or 'shared'.");
                }

                target = ParseTarget(args[index]);
                continue;
            }

            if (string.Equals(argument, "--connection", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                if (index >= args.Length)
                {
                    throw new InvalidOperationException("The --connection option requires a PostgreSQL connection string value.");
                }

                explicitConnectionString = args[index];
                continue;
            }

            if (string.Equals(argument, "--yes", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(argument, "-y", StringComparison.OrdinalIgnoreCase))
            {
                skipConfirmation = true;
                continue;
            }

            pathArguments.Add(argument);
        }

        return target switch
        {
            ImportTarget.Seed => new ImportExecutionOptions(
                target,
                ResolveSeedDatabasePath(),
                skipConfirmation,
                pathArguments),
            ImportTarget.Shared => new ImportExecutionOptions(
                target,
                ResolveSharedCatalogConnectionString(explicitConnectionString),
                skipConfirmation,
                pathArguments),
            _ => throw new InvalidOperationException($"Unsupported import target '{target}'."),
        };
    }

    private static string ResolveSeedDatabasePath()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string databasePath = Path.Combine(repositoryRoot, SeedDatabaseRelativePath);
        string databaseDirectory = Path.GetDirectoryName(databasePath)
            ?? throw new InvalidOperationException("Unable to resolve the seed database directory.");

        Directory.CreateDirectory(databaseDirectory);
        return databasePath;
    }

    private static string ResolveSharedCatalogConnectionString(string? explicitConnectionString)
    {
        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
        {
            return explicitConnectionString.Trim();
        }

        string? environmentValue = Environment.GetEnvironmentVariable(SharedCatalogConnectionStringEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue.Trim();
        }

        string repositoryRoot = ResolveRepositoryRoot();
        string settingsPath = Path.Combine(repositoryRoot, WebApiSettingsRelativePath);

        if (!File.Exists(settingsPath))
        {
            throw new InvalidOperationException(
                $"The shared catalog connection string could not be resolved because '{settingsPath}' was not found.");
        }

        using FileStream stream = File.OpenRead(settingsPath);
        using JsonDocument document = JsonDocument.Parse(stream);

        if (document.RootElement.TryGetProperty("ConnectionStrings", out JsonElement connectionStrings) &&
            connectionStrings.TryGetProperty("SharedCatalogAdmin", out JsonElement sharedCatalogAdmin) &&
            sharedCatalogAdmin.ValueKind == JsonValueKind.String)
        {
            string? value = sharedCatalogAdmin.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        throw new InvalidOperationException(
            $"The shared catalog connection string could not be resolved. Set {SharedCatalogConnectionStringEnvironmentVariable}, use --connection, or populate SharedCatalogAdmin in '{settingsPath}'.");
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");
            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to resolve the repository root.");
    }

    private static ImportSessionInput ResolveSessionInput(IReadOnlyList<string> pathArguments)
    {
        if (pathArguments.Count == 0)
        {
            return new ImportSessionInput(
                true,
                "Main word-learning content",
                DefaultContentRoot,
                []);
        }

        if (pathArguments.Count > 1)
        {
            throw new InvalidOperationException("Only one file or directory path can be provided.");
        }

        string candidatePath = Path.GetFullPath(pathArguments[0]);

        if (File.Exists(candidatePath))
        {
            return new ImportSessionInput(
                false,
                "Main word-learning content",
                Path.GetDirectoryName(candidatePath) ?? string.Empty,
                [candidatePath]);
        }

        if (Directory.Exists(candidatePath))
        {
            return new ImportSessionInput(
                false,
                "Main word-learning content",
                candidatePath,
                GetJsonFiles(candidatePath));
        }

        throw new DirectoryNotFoundException($"The provided path was not found: {candidatePath}");
    }

    private static ImportSessionInput RunInteractiveSession(ImportSessionInput defaultInput)
    {
        Console.WriteLine("Select the content area to import:");
        Console.WriteLine("  1. Main word-learning content");

        while (true)
        {
            Console.Write("Choice [1]: ");
            string choice = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(choice) || string.Equals(choice, "1", StringComparison.Ordinal))
            {
                break;
            }

            Console.WriteLine("Only option 1 is available right now.");
        }

        string sourceDirectory = PromptForDirectory(defaultInput.SourceDirectory);
        IReadOnlyList<string> jsonFiles = GetJsonFiles(sourceDirectory);

        return new ImportSessionInput(
            true,
            "Main word-learning content",
            sourceDirectory,
            jsonFiles);
    }

    private static string PromptForDirectory(string defaultDirectory)
    {
        while (true)
        {
            Console.Write($"Folder path [{defaultDirectory}]: ");
            string input = (Console.ReadLine() ?? string.Empty).Trim();
            string selectedDirectory = string.IsNullOrWhiteSpace(input) ? defaultDirectory : input;
            string fullPath = Path.GetFullPath(selectedDirectory);

            if (!Directory.Exists(fullPath))
            {
                Console.WriteLine($"Folder not found: {fullPath}");
                continue;
            }

            return fullPath;
        }
    }

    private static IReadOnlyList<string> GetJsonFiles(string sourceDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);

        return Directory
            .EnumerateFiles(sourceDirectory, "*.json", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool ConfirmImport()
    {
        while (true)
        {
            Console.Write("Start import? [Y/n]: ");
            string input = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(input) ||
                string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
    }

    private static async Task<BatchImportSummary> ImportFilesAsync(
        IContentImportService contentImportService,
        IReadOnlyList<string> jsonFilePaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contentImportService);
        ArgumentNullException.ThrowIfNull(jsonFilePaths);

        BatchImportSummary summary = new();

        foreach (string jsonFilePath in jsonFilePaths)
        {
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(jsonFilePath), cancellationToken)
                .ConfigureAwait(false);

            summary.TotalFiles++;
            summary.TotalEntries += result.TotalEntries;
            summary.ImportedEntries += result.ImportedEntries;
            summary.SkippedDuplicateEntries += result.SkippedDuplicateEntries;
            summary.InvalidEntries += result.InvalidEntries;
            summary.WarningCount += result.WarningCount;

            foreach (string lemma in result.ImportedLemmas)
            {
                Console.WriteLine(lemma);
            }

            if (result.IsSuccess)
            {
                summary.SuccessfulFiles++;
                continue;
            }

            summary.FailedFiles++;
            summary.FailedFileSummaries.Add(
                $"{Path.GetFileName(jsonFilePath)}: {result.Issues.FirstOrDefault()?.Message ?? result.Status}");
        }

        return summary;
    }

    private static void PrintBanner(ImportExecutionOptions executionOptions)
    {
        Console.WriteLine("Darwin Lingua Import Tool");
        Console.WriteLine($"Target: {executionOptions.Target}");

        if (executionOptions.Target == ImportTarget.Seed)
        {
            Console.WriteLine($"Seed database: {executionOptions.ConnectionOrPath}");
            Console.WriteLine("Note: this target updates the packaged MAUI seed database.");
            Console.WriteLine("New app installs copy this seed on first launch.");
        }
        else
        {
            Console.WriteLine("Target database: shared PostgreSQL catalog");
            Console.WriteLine("Note: this target updates the shared server-side content database used by WebApi.");
        }

        Console.WriteLine();
    }

    private static void PrintSummary(BatchImportSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        Console.WriteLine();
        Console.WriteLine("Import summary");
        Console.WriteLine($"Files processed: {summary.TotalFiles}");
        Console.WriteLine($"Files succeeded: {summary.SuccessfulFiles}");
        Console.WriteLine($"Files failed: {summary.FailedFiles}");
        Console.WriteLine($"Entries total: {summary.TotalEntries}");
        Console.WriteLine($"Entries imported: {summary.ImportedEntries}");
        Console.WriteLine($"Entries skipped as duplicates: {summary.SkippedDuplicateEntries}");
        Console.WriteLine($"Entries invalid: {summary.InvalidEntries}");
        Console.WriteLine($"Warnings: {summary.WarningCount}");

        if (summary.FailedFileSummaries.Count > 0)
        {
            Console.WriteLine("Failed files:");
            foreach (string failedFileSummary in summary.FailedFileSummaries)
            {
                Console.WriteLine($"- {failedFileSummary}");
            }
        }
    }

    private static ImportTarget ParseTarget(string value)
    {
        string normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "seed" => ImportTarget.Seed,
            "shared" => ImportTarget.Shared,
            _ => throw new InvalidOperationException("The --target value must be 'seed' or 'shared'."),
        };
    }

    private sealed record ImportSessionInput(
        bool IsInteractive,
        string ContentAreaLabel,
        string SourceDirectory,
        IReadOnlyList<string> JsonFilePaths);

    private sealed class BatchImportSummary
    {
        public int TotalFiles { get; set; }

        public int SuccessfulFiles { get; set; }

        public int FailedFiles { get; set; }

        public int TotalEntries { get; set; }

        public int ImportedEntries { get; set; }

        public int SkippedDuplicateEntries { get; set; }

        public int InvalidEntries { get; set; }

        public int WarningCount { get; set; }

        public List<string> FailedFileSummaries { get; } = [];
    }

    private sealed record ImportExecutionOptions(
        ImportTarget Target,
        string ConnectionOrPath,
        bool SkipConfirmation,
        IReadOnlyList<string> PathArguments);

    private enum ImportTarget
    {
        Seed,
        Shared
    }
}
