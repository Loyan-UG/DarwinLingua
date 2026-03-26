namespace DarwinLingua.ContentOps.Application.Abstractions;

/// <summary>
/// Reads raw content-package files from storage.
/// </summary>
public interface IContentImportFileReader
{
    /// <summary>
    /// Reads the raw file contents for the specified path.
    /// </summary>
    Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken);
}
