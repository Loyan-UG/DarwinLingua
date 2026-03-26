using DarwinLingua.ContentOps.Application.Abstractions;

namespace DarwinLingua.ContentOps.Infrastructure.Services;

/// <summary>
/// Reads raw JSON content-package files from the local filesystem.
/// </summary>
internal sealed class ContentImportFileReader : IContentImportFileReader
{
    /// <inheritdoc />
    public Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return File.ReadAllTextAsync(filePath, cancellationToken);
    }
}
