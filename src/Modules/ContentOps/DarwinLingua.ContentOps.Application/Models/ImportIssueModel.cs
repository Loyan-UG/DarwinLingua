namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents a single import warning or error produced during package processing.
/// </summary>
public sealed record ImportIssueModel(int? EntryIndex, string Severity, string Message);
