using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ExerciseAttemptAndSearchHardeningStructuralTests
{
    [Fact]
    public void Program_ShouldRequireAuthorizationAndRateLimit_ForPersistedExerciseAttempts()
    {
        string program = File.ReadAllText(GetProgramPath());
        string normalized = NormalizeWhitespace(program);

        Assert.Contains("MapPost(", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/learning/exercises/{slug}/attempts\"", program, StringComparison.Ordinal);
        Assert.Contains("attemptService.SubmitAttemptAsync( slug, request, GetRequiredUserId(principal), primaryMeaningLanguageCode ?? \"en\", cancellationToken)", normalized, StringComparison.Ordinal);
        Assert.Contains(".RequireAuthorization()", program, StringComparison.Ordinal);
        Assert.Contains(".RequireRateLimiting(\"ExerciseAttempts\")", program, StringComparison.Ordinal);
        Assert.DoesNotContain("Identity?.Name ?? \"anonymous\"", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_ShouldExposeStatelessRateLimitedExerciseEvaluation()
    {
        string program = File.ReadAllText(GetProgramPath());
        string normalized = NormalizeWhitespace(program);

        Assert.Contains("MapPost(", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/exercises/{slug}/evaluate\"", program, StringComparison.Ordinal);
        Assert.Contains("attemptService.EvaluateAttemptAsync( slug, request, primaryMeaningLanguageCode ?? \"en\", cancellationToken)", normalized, StringComparison.Ordinal);
        Assert.Contains(".RequireRateLimiting(\"ExerciseAttempts\")", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_ShouldRateLimitUnifiedLearningSearch()
    {
        string program = File.ReadAllText(GetProgramPath());

        Assert.Contains("MapGet(", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/search\"", program, StringComparison.Ordinal);
        Assert.Contains(".RequireRateLimiting(\"CatalogSearch\")", program, StringComparison.Ordinal);
        Assert.Contains("AddPolicy(\"CatalogSearch\"", program, StringComparison.Ordinal);
    }

    [Fact]
    public void ExerciseRunnerView_ShouldNotExposeAdvancedJsonAnswerToLearners()
    {
        string view = File.ReadAllText(GetExerciseDetailViewPath());

        Assert.DoesNotContain("Advanced JSON answer", view, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Answer JSON", view, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PromptJson", view, StringComparison.Ordinal);
        Assert.DoesNotContain("submittedAnswerJson", view, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_ShouldNotAllowPublicQueryStringToUnlockSensitiveEducationalLanguage()
    {
        string program = File.ReadAllText(GetProgramPath());
        string normalized = NormalizeWhitespace(program);

        Assert.Contains("IsSensitiveEducationalLanguageRequestAllowed", program, StringComparison.Ordinal);
        Assert.Contains("IsMatchingSecret(suppliedKey, options.ApiKey)", normalized, StringComparison.Ordinal);
        Assert.Contains("IsSensitiveEducationalLanguageRequestAllowed(httpContext, includeSensitiveEducationalLanguage)", program, StringComparison.Ordinal);
        Assert.DoesNotContain("includeSensitiveEducationalLanguage == true", program, StringComparison.Ordinal);
    }

    [Fact]
    public void SearchIndexMigration_ShouldUsePostgresTrigramIndexes()
    {
        string migration = File.ReadAllText(GetSearchIndexMigrationPath());
        string initializer = File.ReadAllText(GetDatabaseInitializerPath());

        Assert.Contains("CREATE EXTENSION IF NOT EXISTS pg_trgm", migration, StringComparison.Ordinal);
        Assert.Contains("EnsurePostgresExtensionAsync(dbContext, \"pg_trgm\"", initializer, StringComparison.Ordinal);
        Assert.Contains("USING GIN", migration, StringComparison.Ordinal);
        Assert.Contains("USING GIN", initializer, StringComparison.Ordinal);
        Assert.Contains("gin_trgm_ops", migration, StringComparison.Ordinal);
        Assert.Contains("gin_trgm_ops", initializer, StringComparison.Ordinal);
        Assert.Contains("IX_WordEntries_Lemma_Trgm", migration, StringComparison.Ordinal);
        Assert.Contains("IX_GrammarTopics_Title_Trgm", migration, StringComparison.Ordinal);
        Assert.Contains("IX_ExpressionEntries_Text_Trgm", migration, StringComparison.Ordinal);
        Assert.Contains("IX_CourseLessons_SearchFilters", migration, StringComparison.Ordinal);
        Assert.Contains("IX_WritingTemplates_SearchFilters", migration, StringComparison.Ordinal);
        Assert.Contains("ApplyPostgresUnifiedSearchIndexesAsync", initializer, StringComparison.Ordinal);
    }

    private static string GetProgramPath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Apps", "DarwinLingua.WebApi", "Program.cs");

    private static string GetSearchIndexMigrationPath() =>
        Path.Combine(
            FindRepositoryRoot(),
            "src",
            "BuildingBlocks",
            "DarwinLingua.Infrastructure",
            "Persistence",
            "Migrations",
            "20260512143000_AddUnifiedLearningSearchIndexes.cs");

    private static string GetDatabaseInitializerPath() =>
        Path.Combine(
            FindRepositoryRoot(),
            "src",
            "BuildingBlocks",
            "DarwinLingua.Infrastructure",
            "Persistence",
            "DarwinLinguaDatabaseInitializer.cs");

    private static string GetExerciseDetailViewPath() =>
        Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Views",
            "Exercises",
            "Detail.cshtml");

    private static string FindRepositoryRoot()
    {
        foreach (string startPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            DirectoryInfo? directory = new(startPath);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DarwinLingua.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not find the DarwinLingua repository root.");
    }

    private static string NormalizeWhitespace(string value) =>
        string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
