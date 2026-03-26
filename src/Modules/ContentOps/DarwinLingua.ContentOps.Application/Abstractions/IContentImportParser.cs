using DarwinLingua.ContentOps.Application.Models;

namespace DarwinLingua.ContentOps.Application.Abstractions;

/// <summary>
/// Parses raw JSON content into a structured package model.
/// </summary>
public interface IContentImportParser
{
    /// <summary>
    /// Parses a raw JSON package document.
    /// </summary>
    Task<ParsedContentPackageModel> ParseAsync(string rawContent, CancellationToken cancellationToken);
}
