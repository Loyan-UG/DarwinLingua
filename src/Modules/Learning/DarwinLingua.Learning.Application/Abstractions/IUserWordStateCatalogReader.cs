namespace DarwinLingua.Learning.Application.Abstractions;

/// <summary>
/// Reads lexical data needed by the learning module for user-word-state workflows.
/// </summary>
public interface IUserWordStateCatalogReader
{
    /// <summary>
    /// Determines whether an active lexical entry exists for the specified public identifier.
    /// </summary>
    Task<bool> ExistsAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);
}
