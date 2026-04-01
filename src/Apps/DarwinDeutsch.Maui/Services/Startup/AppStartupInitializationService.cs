using System.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Storage;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Startup;

/// <summary>
/// Performs local database and localization startup work after the first page is visible.
/// </summary>
internal sealed class AppStartupInitializationService : IAppStartupInitializationService
{
    private readonly ILogger<AppStartupInitializationService> _logger;
    private readonly ISeedDatabaseProvisioningService _seedDatabaseProvisioningService;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppStartupInitializationService"/> class.
    /// </summary>
    public AppStartupInitializationService(
        ILogger<AppStartupInitializationService> logger,
        ISeedDatabaseProvisioningService seedDatabaseProvisioningService,
        IDatabaseInitializer databaseInitializer,
        IAppLocalizationService appLocalizationService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(seedDatabaseProvisioningService);
        ArgumentNullException.ThrowIfNull(databaseInitializer);
        ArgumentNullException.ThrowIfNull(appLocalizationService);

        _logger = logger;
        _seedDatabaseProvisioningService = seedDatabaseProvisioningService;
        _databaseInitializer = databaseInitializer;
        _appLocalizationService = appLocalizationService;
    }

    /// <inheritdoc />
    public async Task<AppStartupInitializationResult> InitializeAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return AppStartupInitializationResult.Success;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_isInitialized)
            {
                return AppStartupInitializationResult.Success;
            }

            return await Task.Run(
                    async () => await InitializeCoreAsync(cancellationToken).ConfigureAwait(false),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<AppStartupInitializationResult> InitializeCoreAsync(CancellationToken cancellationToken)
    {
        Stopwatch startupStopwatch = Stopwatch.StartNew();
        string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");

        try
        {
            Stopwatch stepStopwatch = Stopwatch.StartNew();
            await _seedDatabaseProvisioningService
                .EnsureSeedDatabaseAsync(databasePath, cancellationToken)
                .ConfigureAwait(false);
            _logger.LogInformation("Startup ensured packaged seed database in {ElapsedMs} ms.", stepStopwatch.ElapsedMilliseconds);

            stepStopwatch.Restart();
            await _databaseInitializer.InitializeAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Startup initialized local database in {ElapsedMs} ms.", stepStopwatch.ElapsedMilliseconds);

            stepStopwatch.Restart();
            await _appLocalizationService.InitializeAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Startup initialized localization in {ElapsedMs} ms.", stepStopwatch.ElapsedMilliseconds);

            stepStopwatch.Restart();
            await _seedDatabaseProvisioningService
                .ApplySeedUpdateAsync(databasePath, cancellationToken)
                .ConfigureAwait(false);
            _logger.LogInformation("Startup applied packaged seed updates in {ElapsedMs} ms.", stepStopwatch.ElapsedMilliseconds);

            _logger.LogInformation("Startup initialization completed in {ElapsedMs} ms.", startupStopwatch.ElapsedMilliseconds);
            _isInitialized = true;
            return AppStartupInitializationResult.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Startup initialization failed after {ElapsedMs} ms.", startupStopwatch.ElapsedMilliseconds);
            return new AppStartupInitializationResult(false, exception.Message);
        }
    }
}
