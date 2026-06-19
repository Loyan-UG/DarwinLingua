using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Application.Tests;

public sealed class ExerciseAttemptServiceTests
{
    [Fact]
    public async Task SubmitAttemptAsync_ShouldPersistAuthenticatedUserId()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        ExerciseAttemptResultModel? result = await service.SubmitAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("""{ "selectedOptionIds": ["der"] }"""),
            " user-123 ",
            "en",
            CancellationToken.None);

        Assert.NotNull(result);
        UserExerciseAttempt attempt = Assert.Single(repository.Attempts);
        Assert.Equal("user-123", attempt.UserId);
        Assert.Equal("""{"selectedOptionIds":["der"]}""", attempt.SubmittedAnswerJson);
        Assert.True(attempt.IsCorrect);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldRejectMissingUserId()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SubmitAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("""{ "selectedOptionIds": ["der"] }"""),
            " ",
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task EvaluateAttemptAsync_ShouldNotPersistAnonymousAttempt()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        ExerciseAttemptResultModel? result = await service.EvaluateAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("""{ "selectedOptionIds": ["der"] }"""),
            "en",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsCorrect);
        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldRejectMalformedJson()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SubmitAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("{ nope"),
            "user-123",
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task EvaluateAttemptAsync_ShouldRejectMalformedJsonWithoutPersisting()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.EvaluateAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("{ nope"),
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldRejectPrimitiveJsonShape()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SubmitAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("\"der\""),
            "user-123",
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task EvaluateAttemptAsync_ShouldRejectPrimitiveJsonShapeWithoutPersisting()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.EvaluateAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("\"der\""),
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldRejectOversizedJson()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SubmitAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel($$"""{ "answer": "{{new string('a', 4097)}}" }"""),
            "user-123",
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task EvaluateAttemptAsync_ShouldRejectOversizedJsonWithoutPersisting()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.EvaluateAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel($$"""{ "answer": "{{new string('a', 4097)}}" }"""),
            "en",
            CancellationToken.None));

        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        FakeExerciseRepository repository = new(null);
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        ExerciseAttemptResultModel? result = await service.SubmitAttemptAsync(
            "missing-exercise",
            new ExerciseAttemptRequestModel("""{ "selectedOptionIds": ["der"] }"""),
            "user-123",
            "en",
            CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(repository.Attempts);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldNotExposeAnswerKey()
    {
        FakeExerciseRepository repository = new(CreateExercise());
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IExerciseAttemptService service = serviceProvider.GetRequiredService<IExerciseAttemptService>();

        ExerciseAttemptResultModel? result = await service.SubmitAttemptAsync(
            "a1-article-choice",
            new ExerciseAttemptRequestModel("""{ "selectedOptionIds": ["die"] }"""),
            "user-123",
            "en",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.DoesNotContain("correctOptionIds", result.Explanation, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("der", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    private static Exercise CreateExercise() =>
        new(
            Guid.NewGuid(),
            "a1-article-choice",
            "Choose the article",
            "Choose the correct article.",
            CefrLevel.A1,
            "multiple-choice",
            "grammar",
            "grammar-topic",
            "a1-articles",
            """{ "options": [{ "id": "der", "text": "der" }, { "id": "die", "text": "die" }] }""",
            """{ "correctOptionIds": ["der"] }""",
            "Correct.",
            "Try again.",
            "Check the noun gender.",
            "Article gender is often memorized with the noun.",
            PublicationStatus.Active,
            10,
            DateTime.UtcNow);

    private static ServiceProvider BuildServiceProvider(IExerciseRepository repository)
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton(repository);
        return services.BuildServiceProvider();
    }

    private sealed class FakeExerciseRepository(Exercise? exercise) : IExerciseRepository
    {
        public List<UserExerciseAttempt> Attempts { get; } = [];

        public Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, CancellationToken cancellationToken) =>
            Task.FromResult(exercise is not null && string.Equals(exercise.Slug, slug, StringComparison.Ordinal) ? exercise : null);

        public Task SaveAttemptAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken)
        {
            Attempts.Add(attempt);
            return Task.CompletedTask;
        }
    }
}
