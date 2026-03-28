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

/// <summary>
/// Bootstraps the Darwin Lingua import tool host.
/// </summary>
internal static class Program
{
    private const string DefaultContentRoot = @"D:\_Projects\DarwinLingua.Content";

    /// <summary>
    /// Creates the import host and validates the current dependency wiring.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the process.</param>
    /// <returns>A task that completes when the host bootstrap finishes.</returns>
    private static async Task Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        try
        {
            string databasePath = ResolveDatabasePath();

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            ConfigureLogging(builder);
            RegisterModules(builder.Services, databasePath);

            using IHost host = builder.Build();

            IDatabaseInitializer databaseInitializer = host.Services.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

            Console.WriteLine("Darwin Lingua Import Tool");
            Console.WriteLine($"Target database: {databasePath}");
            Console.WriteLine("Note: this tool writes to the host-machine database. Android and iOS apps use their own sandbox databases.");
            Console.WriteLine();

            ImportSessionInput sessionInput = ResolveSessionInput(args);
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

            if (!ConfirmImport())
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
        catch (Exception exception) when (exception is DirectoryNotFoundException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Import failed: {exception.Message}");
        }
    }

    /// <summary>
    /// Configures logging for the import tool host.
    /// </summary>
    /// <param name="builder">The host builder being configured.</param>
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

    /// <summary>
    /// Registers the current application and infrastructure modules used by the import tool.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="databasePath">The SQLite database path targeted by the import tool.</param>
    private static void RegisterModules(IServiceCollection services, string databasePath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
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

    private static string ResolveDatabasePath()
    {
        string databaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DarwinLingua");

        Directory.CreateDirectory(databaseDirectory);
        return Path.Combine(databaseDirectory, "darwin-lingua.db");
    }

    private static ImportSessionInput ResolveSessionInput(string[] args)
    {
        if (args.Length == 0)
        {
            return new ImportSessionInput(
                true,
                "Main word-learning content",
                DefaultContentRoot,
                []);
        }

        string candidatePath = Path.GetFullPath(args[0]);

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
}
