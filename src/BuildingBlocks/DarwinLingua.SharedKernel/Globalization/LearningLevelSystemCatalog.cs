namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Defines learning-level systems and target-language-specific learner-facing CEFR labels.
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

    public static readonly IReadOnlyList<LearningLevelDefinition> EnglishCefrLevels =
    [
        new(CefrCode, "A1", "First steps", "Understand and use very simple everyday English.", 10, "A1"),
        new(CefrCode, "A2", "Everyday basics", "Handle common routines with simple sentences.", 20, "A2"),
        new(CefrCode, "B1", "Independent use", "Explain needs, plans, opinions, and everyday problems.", 30, "B1"),
        new(CefrCode, "B2", "Confident use", "Discuss familiar and professional topics with control.", 40, "B2"),
        new(CefrCode, "C1", "Advanced control", "Use English flexibly in study, work, and public contexts.", 50, "C1"),
        new(CefrCode, "C2", "Expert style", "Handle nuance, rhetoric, and complex discourse.", 60, "C2")
    ];

    public static readonly IReadOnlyList<LearningLevelDefinition> SpanishCefrLevels =
    [
        new(CefrCode, "A1", "Primeros pasos", "Comprender y usar frases muy simples en situaciones cotidianas.", 10, "A1"),
        new(CefrCode, "A2", "Bases cotidianas", "Resolver rutinas comunes con frases sencillas.", 20, "A2"),
        new(CefrCode, "B1", "Uso independiente", "Explicar necesidades, planes, opiniones y problemas cotidianos.", 30, "B1"),
        new(CefrCode, "B2", "Uso seguro", "Hablar y escribir con control sobre temas conocidos y profesionales.", 40, "B2"),
        new(CefrCode, "C1", "Dominio avanzado", "Usar el espanol con flexibilidad en estudio, trabajo y debate publico.", 50, "C1"),
        new(CefrCode, "C2", "Estilo experto", "Manejar matices, retorica y discurso complejo con precision.", 60, "C2")
    ];

    public static readonly IReadOnlyList<LearningLevelDefinition> FrenchCefrLevels =
    [
        new(CefrCode, "A1", "Premiers pas", "Comprendre et utiliser des phrases tres simples du quotidien.", 10, "A1"),
        new(CefrCode, "A2", "Bases du quotidien", "Gerer des routines courantes avec des phrases simples.", 20, "A2"),
        new(CefrCode, "B1", "Usage autonome", "Expliquer des besoins, des projets, des opinions et des problemes courants.", 30, "B1"),
        new(CefrCode, "B2", "Usage assure", "Communiquer avec controle sur des sujets familiers et professionnels.", 40, "B2"),
        new(CefrCode, "C1", "Maitrise avancee", "Utiliser le francais avec souplesse dans les etudes, le travail et le debat public.", 50, "C1"),
        new(CefrCode, "C2", "Style expert", "Maitriser les nuances, la rhetorique et les discours complexes.", 60, "C2")
    ];

    public static IReadOnlyList<LearningLevelDefinition> GetCefrLevelsForTargetLanguage(string? targetLearningLanguageCode)
    {
        string normalized = string.IsNullOrWhiteSpace(targetLearningLanguageCode)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : targetLearningLanguageCode.Trim().ToLowerInvariant();

        return normalized switch
        {
            "en" => EnglishCefrLevels,
            "es" => SpanishCefrLevels,
            "fr" => FrenchCefrLevels,
            _ => GermanCefrLevels
        };
    }

    public static IReadOnlyList<LearningLevelDefinition> GetLevelDefinitionsForTargetLanguage(
        string? targetLearningLanguageCode,
        string? levelSystemCode)
    {
        if (!string.Equals(levelSystemCode, CefrCode, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return GetCefrLevelsForTargetLanguage(targetLearningLanguageCode);
    }
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
