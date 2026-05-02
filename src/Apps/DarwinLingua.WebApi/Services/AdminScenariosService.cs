using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IAdminScenariosService
{
    Task<AdminScenariosResponse> GetScenariosAsync(CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse> CreateScenarioAsync(AdminSaveScenarioRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> UpdateScenarioAsync(Guid scenarioId, AdminSaveScenarioRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddDialogueTurnAsync(Guid scenarioId, AdminAddScenarioDialogueTurnRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeleteDialogueTurnAsync(Guid scenarioId, Guid turnId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddPhraseAsync(Guid scenarioId, AdminAddScenarioPhraseRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeletePhraseAsync(Guid scenarioId, Guid phraseId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddQuestionAsync(Guid scenarioId, AdminAddScenarioQuestionRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeleteQuestionAsync(Guid scenarioId, Guid questionId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddAnswerAsync(Guid scenarioId, Guid questionId, AdminAddScenarioAnswerRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeleteAnswerAsync(Guid scenarioId, Guid questionId, Guid answerId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddDialogueTurnTranslationAsync(Guid scenarioId, Guid turnId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeleteDialogueTurnTranslationAsync(Guid scenarioId, Guid turnId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddPhraseTranslationAsync(Guid scenarioId, Guid phraseId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeletePhraseTranslationAsync(Guid scenarioId, Guid phraseId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddQuestionTranslationAsync(Guid scenarioId, Guid questionId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeleteQuestionTranslationAsync(Guid scenarioId, Guid questionId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> AddAnswerTranslationAsync(Guid scenarioId, Guid questionId, Guid answerId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailResponse?> DeleteAnswerTranslationAsync(Guid scenarioId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminBulkScenarioImportResponse> ImportScenariosAsync(AdminBulkScenarioImportRequest request, CancellationToken cancellationToken);
}

internal sealed class AdminScenariosService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IAdminScenariosService
{
    public async Task<AdminScenariosResponse> GetScenariosAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        AdminScenarioItemResponse[] scenarios = await dbContext.ScenarioLessons
            .AsNoTracking()
            .OrderBy(scenario => scenario.SortOrder)
            .ThenBy(scenario => scenario.Title)
            .Select(scenario => new AdminScenarioItemResponse(
                scenario.Id,
                scenario.Slug,
                scenario.Title,
                scenario.CefrLevel.ToString(),
                scenario.Category,
                scenario.PublicationStatus.ToString(),
                scenario.SortOrder,
                scenario.DialogueTurns.Count,
                scenario.UsefulPhrases.Count,
                scenario.Questions.Count,
                scenario.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminScenariosResponse(scenarios);
    }

    public async Task<AdminScenarioDetailResponse?> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        if (scenarioId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioLesson? scenario = await dbContext.ScenarioLessons
            .AsNoTracking()
            .Include(item => item.DialogueTurns)
                .ThenInclude(turn => turn.Translations)
            .Include(item => item.UsefulPhrases)
                .ThenInclude(phrase => phrase.Translations)
            .Include(item => item.Questions)
                .ThenInclude(question => question.Translations)
            .Include(item => item.Questions)
                .ThenInclude(question => question.Answers)
                    .ThenInclude(answer => answer.Translations)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == scenarioId, cancellationToken)
            .ConfigureAwait(false);

        return scenario is null ? null : MapDetail(scenario);
    }

    public async Task<AdminScenarioDetailResponse> CreateScenarioAsync(AdminSaveScenarioRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        CefrLevel level = ParseCefr(request.CefrLevel);
        PublicationStatus status = ParseStatus(request.PublicationStatus);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioLesson scenario = new(
            Guid.NewGuid(),
            request.Slug,
            request.Title,
            request.Description,
            request.LearnerGoal,
            level,
            request.Category,
            status,
            request.SortOrder,
            DateTime.UtcNow);

        dbContext.ScenarioLessons.Add(scenario);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapDetail(scenario);
    }

    public async Task<AdminScenarioDetailResponse?> UpdateScenarioAsync(Guid scenarioId, AdminSaveScenarioRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (scenarioId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioLesson? scenario = await dbContext.ScenarioLessons
            .Include(item => item.DialogueTurns)
            .Include(item => item.UsefulPhrases)
            .Include(item => item.Questions)
                .ThenInclude(question => question.Answers)
            .SingleOrDefaultAsync(item => item.Id == scenarioId, cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return null;
        }

        scenario.UpdateMetadata(
            request.Title,
            request.Description,
            request.LearnerGoal,
            ParseCefr(request.CefrLevel),
            request.Category,
            ParseStatus(request.PublicationStatus),
            request.SortOrder,
            DateTime.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapDetail(scenario);
    }

    public async Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        if (scenarioId == Guid.Empty)
        {
            return false;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioLesson? scenario = await dbContext.ScenarioLessons
            .SingleOrDefaultAsync(item => item.Id == scenarioId, cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return false;
        }

        dbContext.ScenarioLessons.Remove(scenario);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<AdminScenarioDetailResponse?> AddDialogueTurnAsync(Guid scenarioId, AdminAddScenarioDialogueTurnRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ScenarioLesson? scenario = await dbContext.ScenarioLessons
            .Include(item => item.DialogueTurns)
            .SingleOrDefaultAsync(item => item.Id == scenarioId, cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (scenario.DialogueTurns.Count == 0 ? 1 : scenario.DialogueTurns.Max(turn => turn.SortOrder) + 1)
            : request.SortOrder;

        scenario.AddDialogueTurn(Guid.NewGuid(), sortOrder, request.SpeakerRole, request.BaseText, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeleteDialogueTurnAsync(Guid scenarioId, Guid turnId, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioDialogueTurn? turn = await dbContext.ScenarioDialogueTurns
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == turnId, cancellationToken)
            .ConfigureAwait(false);

        if (turn is not null)
        {
            dbContext.ScenarioDialogueTurns.Remove(turn);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddPhraseAsync(Guid scenarioId, AdminAddScenarioPhraseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioLesson? scenario = await dbContext.ScenarioLessons
            .Include(item => item.UsefulPhrases)
            .SingleOrDefaultAsync(item => item.Id == scenarioId, cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (scenario.UsefulPhrases.Count == 0 ? 1 : scenario.UsefulPhrases.Max(phrase => phrase.SortOrder) + 1)
            : request.SortOrder;

        scenario.AddUsefulPhrase(Guid.NewGuid(), sortOrder, request.BaseText, request.UsageNote, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeletePhraseAsync(Guid scenarioId, Guid phraseId, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioPhrase? phrase = await dbContext.ScenarioPhrases
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == phraseId, cancellationToken)
            .ConfigureAwait(false);

        if (phrase is not null)
        {
            dbContext.ScenarioPhrases.Remove(phrase);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddQuestionAsync(Guid scenarioId, AdminAddScenarioQuestionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioLesson? scenario = await dbContext.ScenarioLessons
            .Include(item => item.Questions)
            .SingleOrDefaultAsync(item => item.Id == scenarioId, cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (scenario.Questions.Count == 0 ? 1 : scenario.Questions.Max(question => question.SortOrder) + 1)
            : request.SortOrder;

        scenario.AddQuestion(Guid.NewGuid(), sortOrder, request.Prompt, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeleteQuestionAsync(Guid scenarioId, Guid questionId, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioQuestion? question = await dbContext.ScenarioQuestions
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (question is not null)
        {
            dbContext.ScenarioQuestions.Remove(question);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddAnswerAsync(
        Guid scenarioId,
        Guid questionId,
        AdminAddScenarioAnswerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioQuestion? question = await dbContext.ScenarioQuestions
            .Include(item => item.Answers)
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (question is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (question.Answers.Count == 0 ? 1 : question.Answers.Max(answer => answer.SortOrder) + 1)
            : request.SortOrder;

        question.AddAnswer(Guid.NewGuid(), sortOrder, request.Text, request.IsCorrect, request.Feedback, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeleteAnswerAsync(
        Guid scenarioId,
        Guid questionId,
        Guid answerId,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ScenarioAnswer? answer = await dbContext.ScenarioAnswers
            .SingleOrDefaultAsync(item => item.ScenarioQuestionId == questionId && item.Id == answerId, cancellationToken)
            .ConfigureAwait(false);

        bool questionBelongsToScenario = await dbContext.ScenarioQuestions
            .AnyAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (answer is not null && questionBelongsToScenario)
        {
            dbContext.ScenarioAnswers.Remove(answer);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddDialogueTurnTranslationAsync(
        Guid scenarioId,
        Guid turnId,
        AdminAddScenarioTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ScenarioDialogueTurn? turn = await dbContext.ScenarioDialogueTurns
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == turnId, cancellationToken)
            .ConfigureAwait(false);

        if (turn is null)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        ScenarioDialogueTurnTranslation? existingTranslation = turn.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<ScenarioDialogueTurnTranslation>().Remove(existingTranslation);
        }

        turn.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeleteDialogueTurnTranslationAsync(
        Guid scenarioId,
        Guid turnId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<ScenarioDialogueTurnTranslation>(
            scenarioId,
            turnId,
            translationId,
            dbContext => dbContext.ScenarioDialogueTurns.AnyAsync(item => item.ScenarioLessonId == scenarioId && item.Id == turnId, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddPhraseTranslationAsync(
        Guid scenarioId,
        Guid phraseId,
        AdminAddScenarioTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ScenarioPhrase? phrase = await dbContext.ScenarioPhrases
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == phraseId, cancellationToken)
            .ConfigureAwait(false);

        if (phrase is null)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        ScenarioPhraseTranslation? existingTranslation = phrase.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<ScenarioPhraseTranslation>().Remove(existingTranslation);
        }

        phrase.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeletePhraseTranslationAsync(
        Guid scenarioId,
        Guid phraseId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<ScenarioPhraseTranslation>(
            scenarioId,
            phraseId,
            translationId,
            dbContext => dbContext.ScenarioPhrases.AnyAsync(item => item.ScenarioLessonId == scenarioId && item.Id == phraseId, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddQuestionTranslationAsync(
        Guid scenarioId,
        Guid questionId,
        AdminAddScenarioTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ScenarioQuestion? question = await dbContext.ScenarioQuestions
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (question is null)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        ScenarioQuestionTranslation? existingTranslation = question.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<ScenarioQuestionTranslation>().Remove(existingTranslation);
        }

        question.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeleteQuestionTranslationAsync(
        Guid scenarioId,
        Guid questionId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<ScenarioQuestionTranslation>(
            scenarioId,
            questionId,
            translationId,
            dbContext => dbContext.ScenarioQuestions.AnyAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> AddAnswerTranslationAsync(
        Guid scenarioId,
        Guid questionId,
        Guid answerId,
        AdminAddScenarioTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ScenarioAnswer? answer = await dbContext.ScenarioAnswers
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.ScenarioQuestionId == questionId && item.Id == answerId, cancellationToken)
            .ConfigureAwait(false);

        bool questionBelongsToScenario = await dbContext.ScenarioQuestions
            .AnyAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (answer is null || !questionBelongsToScenario)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        ScenarioAnswerTranslation? existingTranslation = answer.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<ScenarioAnswerTranslation>().Remove(existingTranslation);
        }

        answer.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminScenarioDetailResponse?> DeleteAnswerTranslationAsync(
        Guid scenarioId,
        Guid questionId,
        Guid answerId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<ScenarioAnswerTranslation>(
            scenarioId,
            answerId,
            translationId,
            async dbContext =>
            {
                bool questionBelongsToScenario = await dbContext.ScenarioQuestions
                    .AnyAsync(item => item.ScenarioLessonId == scenarioId && item.Id == questionId, cancellationToken)
                    .ConfigureAwait(false);

                bool answerBelongsToQuestion = await dbContext.ScenarioAnswers
                    .AnyAsync(item => item.ScenarioQuestionId == questionId && item.Id == answerId, cancellationToken)
                    .ConfigureAwait(false);

                return questionBelongsToScenario && answerBelongsToQuestion;
            },
            cancellationToken).ConfigureAwait(false);

        return await GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminBulkScenarioImportResponse> ImportScenariosAsync(AdminBulkScenarioImportRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Scenarios.Count == 0)
        {
            throw new InvalidOperationException("The import file must contain at least one scenario.");
        }

        List<AdminBulkScenarioImportItemResult> results = [];
        int importedCount = 0;
        int skippedCount = 0;
        int failedCount = 0;

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        for (int index = 0; index < request.Scenarios.Count; index++)
        {
            AdminBulkScenarioImportItemRequest item = request.Scenarios[index];
            int rowNumber = index + 1;

            try
            {
                ValidateCompleteScenarioImport(item);

                string slug = item.Slug.Trim().ToLowerInvariant();
                ScenarioLesson? existing = await dbContext.ScenarioLessons
                    .SingleOrDefaultAsync(scenario => scenario.Slug == slug, cancellationToken)
                    .ConfigureAwait(false);

                Guid scenarioId = existing?.Id ?? Guid.NewGuid();
                bool created = existing is null;

                DateTime now = DateTime.UtcNow;
                ScenarioLesson scenario = new(
                    scenarioId,
                    item.Slug,
                    item.Title,
                    item.Description,
                    item.LearnerGoal,
                    ParseCefr(string.IsNullOrWhiteSpace(item.CefrLevel) ? "A1" : item.CefrLevel),
                    string.IsNullOrWhiteSpace(item.Category) ? "general" : item.Category,
                    ParseStatus(string.IsNullOrWhiteSpace(item.PublicationStatus) ? "Draft" : item.PublicationStatus),
                    item.SortOrder,
                    now);

                foreach (AdminBulkScenarioDialogueTurnImportRequest turnItem in item.DialogueTurns ?? [])
                {
                    int sortOrder = turnItem.SortOrder <= 0 ? scenario.DialogueTurns.Count + 1 : turnItem.SortOrder;
                    ScenarioDialogueTurn turn = scenario.AddDialogueTurn(Guid.NewGuid(), sortOrder, turnItem.SpeakerRole, turnItem.BaseText, now);
                    foreach (AdminAddScenarioTranslationRequest translation in turnItem.Translations ?? [])
                    {
                        turn.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                    }
                }

                foreach (AdminBulkScenarioPhraseImportRequest phraseItem in item.UsefulPhrases ?? [])
                {
                    int sortOrder = phraseItem.SortOrder <= 0 ? scenario.UsefulPhrases.Count + 1 : phraseItem.SortOrder;
                    ScenarioPhrase phrase = scenario.AddUsefulPhrase(Guid.NewGuid(), sortOrder, phraseItem.BaseText, phraseItem.UsageNote, now);
                    foreach (AdminAddScenarioTranslationRequest translation in phraseItem.Translations ?? [])
                    {
                        phrase.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                    }
                }

                foreach (AdminBulkScenarioQuestionImportRequest questionItem in item.Questions ?? [])
                {
                    int sortOrder = questionItem.SortOrder <= 0 ? scenario.Questions.Count + 1 : questionItem.SortOrder;
                    ScenarioQuestion question = scenario.AddQuestion(Guid.NewGuid(), sortOrder, questionItem.Prompt, now);
                    foreach (AdminAddScenarioTranslationRequest translation in questionItem.Translations ?? [])
                    {
                        question.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                    }

                    foreach (AdminBulkScenarioAnswerImportRequest answerItem in questionItem.Answers ?? [])
                    {
                        int answerSortOrder = answerItem.SortOrder <= 0 ? question.Answers.Count + 1 : answerItem.SortOrder;
                        ScenarioAnswer answer = question.AddAnswer(Guid.NewGuid(), answerSortOrder, answerItem.Text, answerItem.IsCorrect, answerItem.Feedback, now);
                        foreach (AdminAddScenarioTranslationRequest translation in answerItem.Translations ?? [])
                        {
                            answer.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                        }
                    }
                }

                await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
                    await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                if (existing is not null)
                {
                    dbContext.ScenarioLessons.Remove(existing);
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                dbContext.ScenarioLessons.Add(scenario);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                importedCount++;
                results.Add(new AdminBulkScenarioImportItemResult(
                    rowNumber,
                    item.Slug,
                    scenario.Id,
                    created ? "Imported" : "Updated",
                    created ? "Scenario was created." : "Scenario was replaced with the imported content."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DomainRuleException)
            {
                failedCount++;
                results.Add(new AdminBulkScenarioImportItemResult(rowNumber, item.Slug, null, "Failed", ex.Message));
            }
            catch (DbUpdateException ex)
            {
                failedCount++;
                results.Add(new AdminBulkScenarioImportItemResult(rowNumber, item.Slug, null, "Failed", ex.InnerException?.Message ?? ex.Message));
            }
        }

        return new AdminBulkScenarioImportResponse(
            request.Scenarios.Count,
            importedCount,
            skippedCount,
            failedCount,
            results);
    }

    private static CefrLevel ParseCefr(string value)
    {
        if (!Enum.TryParse(value, ignoreCase: true, out CefrLevel level) || !Enum.IsDefined(level))
        {
            throw new InvalidOperationException($"'{value}' is not a supported CEFR level.");
        }

        return level;
    }

    private static PublicationStatus ParseStatus(string value)
    {
        if (!Enum.TryParse(value, ignoreCase: true, out PublicationStatus status) || !Enum.IsDefined(status))
        {
            throw new InvalidOperationException($"'{value}' is not a supported publication status.");
        }

        return status;
    }

    private static void ValidateCompleteScenarioImport(AdminBulkScenarioImportItemRequest item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new InvalidOperationException("Scenario title is required.");
        }

        if (string.IsNullOrWhiteSpace(item.Description))
        {
            throw new InvalidOperationException("Scenario description is required.");
        }

        if (string.IsNullOrWhiteSpace(item.LearnerGoal))
        {
            throw new InvalidOperationException("Scenario learner goal is required.");
        }

        if (item.DialogueTurns is null || item.DialogueTurns.Count == 0)
        {
            throw new InvalidOperationException("Each scenario must include at least one dialogue turn.");
        }

        if (item.UsefulPhrases is null || item.UsefulPhrases.Count == 0)
        {
            throw new InvalidOperationException("Each scenario must include at least one useful phrase.");
        }

        if (item.Questions is null || item.Questions.Count == 0)
        {
            throw new InvalidOperationException("Each scenario must include at least one practice question.");
        }

        for (int index = 0; index < item.DialogueTurns.Count; index++)
        {
            AdminBulkScenarioDialogueTurnImportRequest turn = item.DialogueTurns[index];
            if (string.IsNullOrWhiteSpace(turn.BaseText))
            {
                throw new InvalidOperationException($"Dialogue turn {index + 1} must include German base text.");
            }

            ValidateCompleteScenarioTranslations(turn.Translations, $"Dialogue turn {index + 1} translations");
        }

        for (int index = 0; index < item.UsefulPhrases.Count; index++)
        {
            AdminBulkScenarioPhraseImportRequest phrase = item.UsefulPhrases[index];
            if (string.IsNullOrWhiteSpace(phrase.BaseText))
            {
                throw new InvalidOperationException($"Useful phrase {index + 1} must include German base text.");
            }

            ValidateCompleteScenarioTranslations(phrase.Translations, $"Useful phrase {index + 1} translations");
        }

        for (int questionIndex = 0; questionIndex < item.Questions.Count; questionIndex++)
        {
            AdminBulkScenarioQuestionImportRequest question = item.Questions[questionIndex];
            if (string.IsNullOrWhiteSpace(question.Prompt))
            {
                throw new InvalidOperationException($"Question {questionIndex + 1} must include German prompt text.");
            }

            ValidateCompleteScenarioTranslations(question.Translations, $"Question {questionIndex + 1} translations");

            if (question.Answers is null || question.Answers.Count == 0)
            {
                throw new InvalidOperationException($"Question {questionIndex + 1} must include at least one answer.");
            }

            if (!question.Answers.Any(answer => answer.IsCorrect))
            {
                throw new InvalidOperationException($"Question {questionIndex + 1} must include a correct answer.");
            }

            for (int answerIndex = 0; answerIndex < question.Answers.Count; answerIndex++)
            {
                AdminBulkScenarioAnswerImportRequest answer = question.Answers[answerIndex];
                if (string.IsNullOrWhiteSpace(answer.Text))
                {
                    throw new InvalidOperationException($"Question {questionIndex + 1} answer {answerIndex + 1} must include German text.");
                }

                ValidateCompleteScenarioTranslations(
                    answer.Translations,
                    $"Question {questionIndex + 1} answer {answerIndex + 1} translations");
            }
        }
    }

    private static void ValidateCompleteScenarioTranslations(
        IReadOnlyList<AdminAddScenarioTranslationRequest>? translations,
        string label)
    {
        if (translations is null || translations.Count == 0)
        {
            throw new InvalidOperationException(
                $"{label} are required for every meaning language: {ContentLanguageRequirements.FormatRequiredMeaningLanguages()}.");
        }

        IReadOnlyList<string> missing = ContentLanguageRequirements.FindMissingMeaningLanguages(
            translations.Select(translation => translation.LanguageCode));
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"{label} are missing languages: {string.Join(", ", missing)}.");
        }

        string[] duplicates = translations
            .Where(translation => !string.IsNullOrWhiteSpace(translation.LanguageCode))
            .GroupBy(translation => translation.LanguageCode.Trim().ToLowerInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException($"{label} contain duplicate languages: {string.Join(", ", duplicates)}.");
        }
    }

    private async Task DeleteTranslationAsync<TTranslation>(
        Guid scenarioId,
        Guid ownerId,
        Guid translationId,
        Func<DarwinLinguaDbContext, Task<bool>> ownerBelongsToScenario,
        CancellationToken cancellationToken)
        where TTranslation : ScenarioTranslationBase
    {
        if (scenarioId == Guid.Empty || ownerId == Guid.Empty || translationId == Guid.Empty)
        {
            return;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        bool validOwner = await ownerBelongsToScenario(dbContext).ConfigureAwait(false);
        if (!validOwner)
        {
            return;
        }

        TTranslation? translation = await dbContext.Set<TTranslation>()
            .SingleOrDefaultAsync(item => item.OwnerId == ownerId && item.Id == translationId, cancellationToken)
            .ConfigureAwait(false);

        if (translation is null)
        {
            return;
        }

        dbContext.Set<TTranslation>().Remove(translation);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static AdminScenarioDetailResponse MapDetail(ScenarioLesson scenario) =>
        new(
            scenario.Id,
            scenario.Slug,
            scenario.Title,
            scenario.Description,
            scenario.LearnerGoal,
            scenario.CefrLevel.ToString(),
            scenario.Category,
            scenario.PublicationStatus.ToString(),
            scenario.SortOrder,
            scenario.CreatedAtUtc,
            scenario.UpdatedAtUtc,
            scenario.DialogueTurns
                .OrderBy(turn => turn.SortOrder)
                .Select(turn => new AdminScenarioDialogueTurnResponse(
                    turn.Id,
                    turn.SortOrder,
                    turn.SpeakerRole,
                    turn.BaseText,
                    MapTranslations(turn.Translations)))
                .ToArray(),
            scenario.UsefulPhrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new AdminScenarioPhraseResponse(
                    phrase.Id,
                    phrase.SortOrder,
                    phrase.BaseText,
                    phrase.UsageNote,
                    MapTranslations(phrase.Translations)))
                .ToArray(),
            scenario.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => new AdminScenarioQuestionResponse(
                    question.Id,
                    question.SortOrder,
                    question.Prompt,
                    MapTranslations(question.Translations),
                    question.Answers
                        .OrderBy(answer => answer.SortOrder)
                        .Select(answer => new AdminScenarioAnswerResponse(
                            answer.Id,
                            answer.SortOrder,
                            answer.Text,
                            answer.IsCorrect,
                            answer.Feedback,
                            MapTranslations(answer.Translations)))
                        .ToArray()))
                .ToArray());

    private static AdminScenarioTranslationResponse[] MapTranslations<TTranslation>(
        IEnumerable<TTranslation> translations)
        where TTranslation : ScenarioTranslationBase =>
        translations
            .OrderBy(translation => translation.LanguageCode.Value)
            .Select(translation => new AdminScenarioTranslationResponse(
                translation.Id,
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
}
