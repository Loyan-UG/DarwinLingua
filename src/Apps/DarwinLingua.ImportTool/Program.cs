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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DarwinLingua.ImportTool;

/// <summary>
/// Bootstraps the Darwin Lingua import tool host.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Creates the import host and validates the current dependency wiring.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the process.</param>
    /// <returns>A task that completes when the host bootstrap finishes.</returns>
    private static async Task Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        ConfigureLogging(builder);
        RegisterModules(builder.Services);

        using IHost host = builder.Build();

        IDatabaseInitializer databaseInitializer = host.Services.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

        ILogger logger = host.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DarwinLingua.ImportTool");

        if (args.Length == 0)
        {
            logger.LogInformation("Darwin Lingua import tool is ready. Pass a JSON file path as the first argument to import content.");
            await host.StopAsync().ConfigureAwait(false);
            return;
        }

        IContentImportService contentImportService = host.Services.GetRequiredService<IContentImportService>();
        ImportContentPackageResult result = await contentImportService
            .ImportAsync(new ImportContentPackageRequest(args[0]), CancellationToken.None)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Import finished. Success={IsSuccess}; PackageId={PackageId}; Status={Status}; Total={Total}; Imported={Imported}; Duplicates={Duplicates}; Invalid={Invalid}; Warnings={Warnings}",
            result.IsSuccess,
            result.PackageId ?? "<none>",
            result.Status,
            result.TotalEntries,
            result.ImportedEntries,
            result.SkippedDuplicateEntries,
            result.InvalidEntries,
            result.WarningCount);

        foreach (ImportIssueModel issue in result.Issues)
        {
            logger.LogInformation(
                "{Severity}: {EntryContext}{Message}",
                issue.Severity,
                issue.EntryIndex is null ? string.Empty : $"Entry {issue.EntryIndex}: ",
                issue.Message);
        }

        await host.StopAsync().ConfigureAwait(false);
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
    }

    /// <summary>
    /// Registers the current application and infrastructure modules used by the import tool.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    private static void RegisterModules(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        string databaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DarwinLingua");
        string databasePath = Path.Combine(databaseDirectory, "darwin-lingua.db");

        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLearningApplication()
            .AddLearningInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure();
    }
}
