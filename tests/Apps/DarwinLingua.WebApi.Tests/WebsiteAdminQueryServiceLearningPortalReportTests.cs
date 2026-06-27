using System.Text.Json;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebsiteAdminQueryServiceLearningPortalReportTests
{
    [Fact]
    public async Task GetSystemReportAsync_ShouldIncludeLearningPortalCountsAndQualitySignals()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_report");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedLearningPortalContentAsync(serviceProvider, CancellationToken.None);

            IWebsiteAdminQueryService service = serviceProvider.GetRequiredService<IWebsiteAdminQueryService>();
            AdminSystemReportResponse report = await service.GetSystemReportAsync("de", CancellationToken.None);

            Assert.Equal("de", report.LearningPortal.TargetLearningLanguageCode);
            Assert.Equal("DE", report.LearningPortal.CountryContextCode);
            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "grammar-topic" && row.Count == 2);
            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "roleplay" && row.Count == 1);
            Assert.Contains(report.LearningPortal.CountsByCefr, row => row.Key == "A1" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.CountsByCefr, row => row.Key == "B1" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.GrammarByCategory, row => row.Key == "articles" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsByType, row => row.Key == "fixed-expression" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsByRegister, row => row.Key == "neutral" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsByMeaningTransparency, row => row.Key == "pragmatic-formula" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsBySafetyRating, row => row.Key == "general" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExercisesByType, row => row.Key == "multiple-choice" && row.Count == 1);
            Assert.Contains(report.LearningPortal.CountsByTargetLanguage, row => row.Key == "grammar-topic:de" && row.Count == 2);
            Assert.Contains(report.LearningPortal.CountsByTargetLanguage, row => row.Key == "course-lesson:de" && row.Count == 6);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "selected-target-active:de" && row.Count == 1);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "pilot-target-languages" && row.Count == 1);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "planned-target-languages" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-active:de" && row.Count == 1);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-content-items:de" && row.Count > 0);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-active:en" && row.Count == 0);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-pilot:en" && row.Count == 1);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-planned:en" && row.Count == 0);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-content-items:en" && row.Count == 0);
            Assert.Contains(report.LearningPortal.TargetLanguageActivationGate, row => row.Key == "target-planned-country-contexts:en" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.MissingTranslationsByHelperLanguage, row => row.Key == "fa" && row.Count > 0);
            Assert.Contains(report.LearningPortal.MissingTranslationsByModule, row => row.Key == "course-lesson" && row.Count > 0);
            Assert.Contains(report.LearningPortal.DuplicateSlugsByType, row => row.Key == "cross-module:de:a1-articles" && row.Count == 2);
            Assert.True(report.LearningPortal.DuplicateSlugCount >= 2);
            Assert.Equal(1, report.LearningPortal.MissingTranslationCount);
            Assert.Equal(1, report.LearningPortal.UnresolvedLinkedContentReferenceCount);
            Assert.Equal(1, report.LearningPortal.GrammarTopicsMissingExercises);
            Assert.Equal(0, report.LearningPortal.ExpressionEntriesMissingEligibilityMetadata);
            Assert.Equal(0, report.LearningPortal.ExpressionEntriesWithOrdinaryLiteralLeakage);
            Assert.Equal(0, report.LearningPortal.ExpressionEntriesMissingTeachingReason);
            Assert.Equal(1, report.LearningPortal.ExpressionEntriesWithFewerThanTwoExamples);
            Assert.Equal(1, report.LearningPortal.RoleplayScenariosMissingTranslations);
            Assert.Equal(0, report.LearningPortal.RoleplayScenariosUnpublishedDrafts);
            Assert.Equal(1, report.LearningPortal.RoleplayScenariosMissingRequiredImageAssets);
            Assert.Equal(1, report.LearningPortal.RoleplayScenariosWithoutAnswerChoices);
            Assert.Equal(1, report.LearningPortal.RoleplayScenariosWithoutStaticFeedback);
            Assert.Equal(1, report.LearningPortal.RoleplayScenariosWithInvalidPlayableSequence);
            Assert.Equal(1, report.LearningPortal.ExamPrepProfilesMissingTranslations);
            Assert.Equal(1, report.LearningPortal.ExamPrepUnitsMissingTranslations);
            Assert.Equal(2, report.LearningPortal.ExamPrepUnpublishedDrafts);
            Assert.Equal(1, report.LearningPortal.ExamPrepUnitsWithMalformedStrategyOrChecklist);
            Assert.Equal(1, report.LearningPortal.ExamPrepUnitsWithoutActiveProfile);
            Assert.Equal(1, report.LearningPortal.PublishedCourseLessonsWithoutActivityBlocks);
            Assert.Equal(1, report.LearningPortal.CourseLessonsWithMalformedActivityBlocksJson);
            Assert.Equal(1, report.LearningPortal.CourseActivityBlocksWithUnsupportedTargetType);
            Assert.Equal(1, report.LearningPortal.CourseActivityBlocksWithUnresolvedTargetSlug);
            Assert.Equal(1, report.LearningPortal.ExerciseSetsWithUnresolvedOwnerReferences);
            Assert.NotEmpty(report.LearningPortal.SampleIssues);

            AdminLearningPortalIssuesResponse issues = await service.GetLearningPortalIssuesAsync("Grammar linked exercise", "missing", "de", 10, CancellationToken.None);
            Assert.Equal("de", issues.TargetLearningLanguageCode);
            Assert.Equal("DE", issues.CountryContextCode);
            Assert.True(issues.TotalCount >= report.LearningPortal.SampleIssues.Count);
            Assert.True(issues.FilteredCount >= 1);
            Assert.Equal("Grammar linked exercise", issues.AreaFilter);
            Assert.Equal("missing", issues.Query);
            Assert.Contains(issues.Issues, issue => issue.Area == "Grammar linked exercise" && issue.Target == "missing-exercise");

            AdminLearningPortalIssuesResponse activityIssues = await service.GetLearningPortalIssuesAsync("CourseLesson activity", "missing-activity-exercise", "de", 10, CancellationToken.None);
            Assert.Contains(activityIssues.Issues, issue =>
                issue.Area == "CourseLesson activity" &&
                issue.Owner == "a1-activity-unresolved" &&
                issue.Target == "exercise:missing-activity-exercise");

            AdminLearningPortalIssuesResponse exerciseSetOwnerIssues = await service.GetLearningPortalIssuesAsync("ExerciseSet owner", "missing-owner-lesson", "de", 10, CancellationToken.None);
            Assert.Contains(exerciseSetOwnerIssues.Issues, issue =>
                issue.Area == "ExerciseSet owner" &&
                issue.Owner == "a1-article-practice-set" &&
                issue.Target == "course-lesson:missing-owner-lesson");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

        }
    }

    [Fact]
    public async Task GetSystemReportAsync_ShouldTreatMissingLearningPortalTablesAsEmpty()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_report");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedLearningPortalContentAsync(serviceProvider, CancellationToken.None);
            await DropTableAsync(serviceProvider, "Exercises", CancellationToken.None);

            IWebsiteAdminQueryService service = serviceProvider.GetRequiredService<IWebsiteAdminQueryService>();
            AdminSystemReportResponse report = await service.GetSystemReportAsync("de", CancellationToken.None);

            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "expression" && row.Count == 1);
            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "exercise" && row.Count == 0);
            Assert.Empty(report.LearningPortal.ExercisesByType);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

        }
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
            .AddScoped<IWebsiteAdminQueryService, WebsiteAdminQueryService>();

        return services.BuildServiceProvider();
    }

    private static async Task SeedLearningPortalContentAsync(
        ServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory = serviceProvider
            .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        GrammarTopic grammarTopic = new(
            Guid.NewGuid(),
            "a1-articles",
            "A1 articles",
            "Article basics",
            CefrLevel.A1,
            "articles",
            PublicationStatus.Active,
            10,
            now);
        grammarTopic.AddSection(Guid.NewGuid(), 10, "Articles", "Use der, die, das.", now);
        grammarTopic.AddLinkedExercise(Guid.NewGuid(), "missing-exercise", 10, now);
        GrammarTopic grammarTopicWithoutExercise = new(
            Guid.NewGuid(),
            "a1-nouns",
            "A1 nouns",
            "Noun basics",
            CefrLevel.A1,
            "nouns",
            PublicationStatus.Active,
            20,
            now);

        Exercise exercise = new(
            Guid.NewGuid(),
            "a1-article-choice",
            "Article choice",
            "Choose the article.",
            CefrLevel.A1,
            "multiple-choice",
            "grammar",
            "grammar-topic",
            "a1-articles",
            "{}",
            "{}",
            "Correct.",
            "Try again.",
            null,
            null,
            PublicationStatus.Active,
            10,
            now);
        ExerciseSet exerciseSet = new(
            Guid.NewGuid(),
            "a1-article-practice-set",
            "Article practice",
            "Practice article choice.",
            CefrLevel.A1,
            "course-lesson",
            "missing-owner-lesson",
            PublicationStatus.Active,
            10,
            now);
        exerciseSet.AddExercise(Guid.NewGuid(), "a1-article-choice", 10, now);

        ExpressionEntry expression = new(
            Guid.NewGuid(),
            "alles-klar",
            "Alles klar.",
            "Everything clear.",
            "Understood or okay.",
            "Use it to confirm a simple plan.",
            CefrLevel.A1,
            "fixed-expression",
            "neutral",
            "daily-life",
            "de",
            false,
            PublicationStatus.Active,
            10,
            now,
            "pragmatic-formula",
            "It is a conventional confirmation formula.",
            "general",
            0,
            false);
        expression.AddMeaning(Guid.NewGuid(), LanguageCode.From("en"), "Understood or okay.", "Everything clear.", "Use it to confirm a simple plan.", now);

        RoleplayScenario roleplayScenario = new(
            Guid.NewGuid(),
            "b1-polito-complaint-roleplay",
            null,
            "Polite complaint roleplay",
            "[]",
            "A deterministic roleplay for making a polite complaint.",
            "[]",
            "Explain a problem and choose a polite next step.",
            "[]",
            CefrLevel.B1,
            "customer-service",
            "complain-politely",
            "service-counter",
            "formal",
            12,
            JsonSerializer.Serialize(new[] { "goethe-b1" }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            JsonSerializer.Serialize(new[] { "speaking", "roleplay", "complaint-handling" }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            "[]",
            "[]",
            "[]",
            "[]",
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        slotKey = "main",
                        placement = "hero",
                        purpose = "Visual context",
                        altText = "Service counter",
                        altTextTranslations = Array.Empty<object>(),
                        imagePrompt = "Clean educational illustration of a service counter.",
                        assetPath = (string?)null,
                        isRequired = true,
                    },
                },
                new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            PublicationStatus.Active,
            30,
            now);

        ExamProfile examProfile = new(
            Guid.NewGuid(),
            "goethe-a2",
            "Goethe A2",
            "A2",
            "Vorbereitung auf die Goethe-A2-Pruefung.",
            PublicationStatus.Draft,
            40,
            now,
            "[]",
            "[]");

        ExamPrepUnit examPrepUnit = new(
            Guid.NewGuid(),
            "a2-goethe-speaking-roleplay",
            "goethe-a2",
            "Strategie fuer das Sprechrollenspiel",
            "Bereite ein kurzes A2-Rollenspiel vor.",
            CefrLevel.A2,
            "speaking",
            "roleplay",
            "exam-preparation",
            "Nutze kurze, klare Fragen und Antworten.",
            "not-json",
            JsonSerializer.Serialize(new[] { "Answer the prompt directly." }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            PublicationStatus.Draft,
            40,
            now,
            "[]",
            "[]",
            "[]",
            "[]",
            "[]");

        CoursePath coursePath = new(
            Guid.NewGuid(),
            "a1-course",
            "A1 course",
            "Course basics.",
            CefrLevel.A1,
            null,
            PublicationStatus.Active,
            50,
            now);
        CourseModule courseModule = new(
            Guid.NewGuid(),
            "a1-course",
            "a1-module",
            "A1 module",
            "Module basics.",
            1,
            CefrLevel.A1,
            PublicationStatus.Active,
            50,
            now);
        courseModule.AttachToCoursePath(coursePath.Id);
        CourseLesson activityReadyLesson = CreateCourseLesson(
            "a1-activity-ready",
            1,
            PublicationStatus.Active,
            now,
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        kind = "read",
                        targetType = "grammar-topic",
                        targetSlug = "a1-articles",
                    },
                },
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        CourseLesson missingActivityLesson = CreateCourseLesson("a1-activity-missing", 2, PublicationStatus.Active, now, "[]");
        CourseLesson malformedActivityLesson = CreateCourseLesson("a1-activity-malformed", 3, PublicationStatus.Active, now, "{not-json");
        CourseLesson unsupportedActivityLesson = CreateCourseLesson(
            "a1-activity-unsupported",
            4,
            PublicationStatus.Active,
            now,
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        kind = "practice",
                        targetType = "unsupported-target",
                        targetSlug = "a1-anything",
                    },
                },
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        CourseLesson unresolvedActivityLesson = CreateCourseLesson(
            "a1-activity-unresolved",
            5,
            PublicationStatus.Active,
            now,
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        kind = "practice",
                        targetType = "exercise",
                        targetSlug = "missing-activity-exercise",
                    },
                },
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        CourseLesson crossModuleDuplicateSlugLesson = CreateCourseLesson(
            "a1-articles",
            6,
            PublicationStatus.Active,
            now,
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        kind = "read",
                        targetType = "grammar-topic",
                        targetSlug = "a1-articles",
                    },
                },
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        activityReadyLesson.AttachToCourseModule(courseModule.Id);
        missingActivityLesson.AttachToCourseModule(courseModule.Id);
        malformedActivityLesson.AttachToCourseModule(courseModule.Id);
        unsupportedActivityLesson.AttachToCourseModule(courseModule.Id);
        unresolvedActivityLesson.AttachToCourseModule(courseModule.Id);
        crossModuleDuplicateSlugLesson.AttachToCourseModule(courseModule.Id);

        dbContext.GrammarTopics.Add(grammarTopic);
        dbContext.GrammarTopics.Add(grammarTopicWithoutExercise);
        dbContext.Exercises.Add(exercise);
        dbContext.ExerciseSets.Add(exerciseSet);
        dbContext.ExpressionEntries.Add(expression);
        dbContext.RoleplayScenarios.Add(roleplayScenario);
        dbContext.ExamProfiles.Add(examProfile);
        dbContext.ExamPrepUnits.Add(examPrepUnit);
        dbContext.CoursePaths.Add(coursePath);
        dbContext.CourseModules.Add(courseModule);
        dbContext.CourseLessons.AddRange(
            activityReadyLesson,
            missingActivityLesson,
            malformedActivityLesson,
            unsupportedActivityLesson,
            unresolvedActivityLesson,
            crossModuleDuplicateSlugLesson);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static CourseLesson CreateCourseLesson(
        string slug,
        int lessonNumber,
        PublicationStatus publicationStatus,
        DateTime now,
        string activityBlocksJson) =>
        new(
            Guid.NewGuid(),
            "a1-course",
            "a1-module",
            slug,
            lessonNumber,
            "Course activity test",
            "Short description.",
            "Narrative for the course activity test.",
            CefrLevel.A1,
            8,
            JsonSerializer.Serialize(new[] { "Learn the route." }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            "[]",
            null,
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "Review.",
            null,
            publicationStatus,
            10,
            now,
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            activityBlocksJson);

    private static async Task DropTableAsync(
        ServiceProvider serviceProvider,
        string tableName,
        CancellationToken cancellationToken)
    {
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory = serviceProvider
            .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string sql = tableName switch
        {
            "Exercises" => "DROP TABLE \"Exercises\"",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported test table."),
        };

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken)
            .ConfigureAwait(false);
    }

}
