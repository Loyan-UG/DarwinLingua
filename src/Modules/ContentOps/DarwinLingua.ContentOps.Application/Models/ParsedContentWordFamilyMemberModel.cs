namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed word-family member from an import package entry.
/// </summary>
public sealed record ParsedContentWordFamilyMemberModel(
    string Lemma,
    string RelationLabel,
    string? Note);
