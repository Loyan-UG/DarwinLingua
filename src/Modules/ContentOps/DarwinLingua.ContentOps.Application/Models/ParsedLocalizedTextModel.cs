namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedLocalizedTextModel(
    string Language,
    string Name,
    string? Description);
