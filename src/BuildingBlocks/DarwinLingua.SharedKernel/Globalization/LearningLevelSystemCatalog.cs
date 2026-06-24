namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Defines learning-level systems and the current German CEFR baseline labels.
/// </summary>
public static class LearningLevelSystemCatalog
{
    public const string CefrCode = "cefr";

    public static readonly IReadOnlyList<LearningLevelDefinition> GermanCefrLevels =
    [
        new(CefrCode, "A1", "Einstieg", "Erste Schritte", 10, "A1"),
        new(CefrCode, "A2", "Grundlagen", "Sicherer Alltag", 20, "A2"),
        new(CefrCode, "B1", "Selbststaendig", "Alltag und Arbeit", 30, "B1"),
        new(CefrCode, "B2", "Kompetent", "Kompetente Anwendung", 40, "B2"),
        new(CefrCode, "C1", "Souveraen", "Souveraene Kommunikation", 50, "C1"),
        new(CefrCode, "C2", "Meisterschaft", "Stil und Praezision", 60, "C2")
    ];
}

/// <summary>
/// Describes one learner-facing level inside a level system.
/// </summary>
public sealed record LearningLevelDefinition(
    string LevelSystemCode,
    string Code,
    string DisplayTitle,
    string LearnerDescription,
    int SortOrder,
    string? StandardMappingCode);
