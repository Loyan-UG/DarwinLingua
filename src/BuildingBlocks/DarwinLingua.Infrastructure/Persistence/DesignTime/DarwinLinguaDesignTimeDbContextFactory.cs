using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarwinLingua.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Creates the EF Core design-time database context used for migration generation.
/// </summary>
public sealed class DarwinLinguaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DarwinLinguaDbContext>
{
    /// <inheritdoc />
    public DarwinLinguaDbContext CreateDbContext(string[] args)
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string designTimeDirectory = Path.Combine(repositoryRoot, "artifacts", "design-time");
        string designTimeDatabasePath = Path.Combine(designTimeDirectory, "darwin-lingua-design.db");

        Directory.CreateDirectory(designTimeDirectory);

        DbContextOptionsBuilder<DarwinLinguaDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={designTimeDatabasePath}");

        return new DarwinLinguaDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Resolves the repository root directory from the infrastructure project output location.
    /// </summary>
    /// <returns>The absolute repository root path.</returns>
    private static string ResolveRepositoryRoot()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        DirectoryInfo? directoryInfo = new(currentDirectory);

        while (directoryInfo is not null &&
               !File.Exists(Path.Combine(directoryInfo.FullName, "DarwinLingua.slnx")))
        {
            directoryInfo = directoryInfo.Parent;
        }

        return directoryInfo?.FullName
               ?? throw new InvalidOperationException("The repository root could not be resolved for design-time EF Core operations.");
    }
}
