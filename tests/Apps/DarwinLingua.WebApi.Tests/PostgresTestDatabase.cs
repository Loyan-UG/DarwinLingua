using System.Diagnostics;
using Npgsql;

namespace DarwinLingua.WebApi.Tests;

internal sealed class PostgresTestDatabase : IAsyncDisposable
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    private PostgresTestDatabase(string databaseName, string connectionString)
    {
        DatabaseName = databaseName;
        ConnectionString = connectionString;
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public static async Task<PostgresTestDatabase> CreateAsync(string prefix, CancellationToken cancellationToken = default)
    {
        string safePrefix = new(prefix.Where(static character => char.IsLetterOrDigit(character) || character == '_').ToArray());
        string databaseName = $"{safePrefix}_{Guid.NewGuid():N}"[..Math.Min(48, safePrefix.Length + 33)];
        await RunDockerPsqlAsync($"""CREATE DATABASE "{databaseName}" OWNER darwinlingua_app;""", cancellationToken);
        return new PostgresTestDatabase(databaseName, BuildAppConnectionString(databaseName));
    }

    public async ValueTask DisposeAsync()
    {
        await DropDatabaseAsync(DatabaseName, CancellationToken.None);
    }

    private static string BuildAppConnectionString(string databaseName)
    {
        string? configuredTemplate = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_APP_CONNECTION_TEMPLATE");
        if (!string.IsNullOrWhiteSpace(configuredTemplate))
        {
            return string.Format(configuredTemplate, databaseName);
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = "localhost",
            Port = 5432,
            Database = databaseName,
            Username = "darwinlingua_app",
            Password = "@pP@sS!13;X"
        };

        return builder.ConnectionString;
    }

    private static async Task DropDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        await RunDockerPsqlAsync(
            $"""
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = '{databaseName}'
              AND pid <> pg_backend_pid();
            """,
            cancellationToken);
        await RunDockerPsqlAsync($"""DROP DATABASE IF EXISTS "{databaseName}";""", cancellationToken);
    }

    private static async Task RunDockerPsqlAsync(string sql, CancellationToken cancellationToken)
    {
        string containerName = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_CONTAINER") ?? DefaultDockerContainerName;
        ProcessStartInfo startInfo = new()
        {
            FileName = "docker",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add(containerName);
        startInfo.ArgumentList.Add("psql");
        startInfo.ArgumentList.Add("-U");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-d");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("ON_ERROR_STOP=1");
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(sql);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start Docker PostgreSQL helper process.");
        string standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string standardError = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"PostgreSQL test database command failed with exit code {process.ExitCode}.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }
    }
}
