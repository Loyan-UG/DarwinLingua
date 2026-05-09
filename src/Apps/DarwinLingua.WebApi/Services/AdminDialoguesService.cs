using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IAdminDialoguesService
{
    Task<AdminDialoguesResponse> GetDialoguesAsync(CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> GetDialogueAsync(Guid dialogueId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse> CreateDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> UpdateDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteDialogueAsync(Guid dialogueId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeleteDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddPhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeletePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeleteQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddAnswerAsync(Guid dialogueId, Guid questionId, AdminAddDialogueAnswerRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeleteAnswerAsync(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeleteDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddPhraseTranslationAsync(Guid dialogueId, Guid phraseId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeletePhraseTranslationAsync(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddQuestionTranslationAsync(Guid dialogueId, Guid questionId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeleteQuestionTranslationAsync(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> AddAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailResponse?> DeleteAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminBulkDialogueImportResponse> ImportDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken);
}

internal sealed class AdminDialoguesService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IAdminDialoguesService
{
    public async Task<AdminDialoguesResponse> GetDialoguesAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        AdminDialogueItemResponse[] dialogues = await dbContext.DialogueLessons
            .AsNoTracking()
            .OrderBy(dialogue => dialogue.SortOrder)
            .ThenBy(dialogue => dialogue.Title)
            .Select(dialogue => new AdminDialogueItemResponse(
                dialogue.Id,
                dialogue.Slug,
                dialogue.Title,
                dialogue.CefrLevel.ToString(),
                dialogue.Category,
                dialogue.PublicationStatus.ToString(),
                dialogue.SortOrder,
                dialogue.DialogueTurns.Count,
                dialogue.UsefulPhrases.Count,
                dialogue.Questions.Count,
                dialogue.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminDialoguesResponse(dialogues);
    }

    public async Task<AdminDialogueDetailResponse?> GetDialogueAsync(Guid dialogueId, CancellationToken cancellationToken)
    {
        if (dialogueId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueLesson? dialogue = await dbContext.DialogueLessons
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
            .SingleOrDefaultAsync(item => item.Id == dialogueId, cancellationToken)
            .ConfigureAwait(false);

        return dialogue is null ? null : MapDetail(dialogue);
    }

    public async Task<AdminDialogueDetailResponse> CreateDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        CefrLevel level = ParseCefr(request.CefrLevel);
        PublicationStatus status = ParseStatus(request.PublicationStatus);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueLesson dialogue = new(
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

        dbContext.DialogueLessons.Add(dialogue);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapDetail(dialogue);
    }

    public async Task<AdminDialogueDetailResponse?> UpdateDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (dialogueId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueLesson? dialogue = await dbContext.DialogueLessons
            .Include(item => item.DialogueTurns)
            .Include(item => item.UsefulPhrases)
            .Include(item => item.Questions)
                .ThenInclude(question => question.Answers)
            .SingleOrDefaultAsync(item => item.Id == dialogueId, cancellationToken)
            .ConfigureAwait(false);

        if (dialogue is null)
        {
            return null;
        }

        dialogue.UpdateMetadata(
            request.Title,
            request.Description,
            request.LearnerGoal,
            ParseCefr(request.CefrLevel),
            request.Category,
            ParseStatus(request.PublicationStatus),
            request.SortOrder,
            DateTime.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapDetail(dialogue);
    }

    public async Task<bool> DeleteDialogueAsync(Guid dialogueId, CancellationToken cancellationToken)
    {
        if (dialogueId == Guid.Empty)
        {
            return false;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueLesson? dialogue = await dbContext.DialogueLessons
            .SingleOrDefaultAsync(item => item.Id == dialogueId, cancellationToken)
            .ConfigureAwait(false);

        if (dialogue is null)
        {
            return false;
        }

        dbContext.DialogueLessons.Remove(dialogue);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<AdminDialogueDetailResponse?> AddDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        DialogueLesson? dialogue = await dbContext.DialogueLessons
            .Include(item => item.DialogueTurns)
            .SingleOrDefaultAsync(item => item.Id == dialogueId, cancellationToken)
            .ConfigureAwait(false);

        if (dialogue is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (dialogue.DialogueTurns.Count == 0 ? 1 : dialogue.DialogueTurns.Max(turn => turn.SortOrder) + 1)
            : request.SortOrder;

        dialogue.AddDialogueTurn(Guid.NewGuid(), sortOrder, request.SpeakerRole, request.BaseText, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeleteDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueTurn? turn = await dbContext.DialogueTurns
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == turnId, cancellationToken)
            .ConfigureAwait(false);

        if (turn is not null)
        {
            dbContext.DialogueTurns.Remove(turn);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddPhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueLesson? dialogue = await dbContext.DialogueLessons
            .Include(item => item.UsefulPhrases)
            .SingleOrDefaultAsync(item => item.Id == dialogueId, cancellationToken)
            .ConfigureAwait(false);

        if (dialogue is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (dialogue.UsefulPhrases.Count == 0 ? 1 : dialogue.UsefulPhrases.Max(phrase => phrase.SortOrder) + 1)
            : request.SortOrder;

        dialogue.AddUsefulPhrase(Guid.NewGuid(), sortOrder, request.BaseText, request.UsageNote, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeletePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialoguePhrase? phrase = await dbContext.DialoguePhrases
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == phraseId, cancellationToken)
            .ConfigureAwait(false);

        if (phrase is not null)
        {
            dbContext.DialoguePhrases.Remove(phrase);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueLesson? dialogue = await dbContext.DialogueLessons
            .Include(item => item.Questions)
            .SingleOrDefaultAsync(item => item.Id == dialogueId, cancellationToken)
            .ConfigureAwait(false);

        if (dialogue is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (dialogue.Questions.Count == 0 ? 1 : dialogue.Questions.Max(question => question.SortOrder) + 1)
            : request.SortOrder;

        dialogue.AddQuestion(Guid.NewGuid(), sortOrder, request.Prompt, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeleteQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueQuestion? question = await dbContext.DialogueQuestions
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (question is not null)
        {
            dbContext.DialogueQuestions.Remove(question);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddAnswerAsync(
        Guid dialogueId,
        Guid questionId,
        AdminAddDialogueAnswerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueQuestion? question = await dbContext.DialogueQuestions
            .Include(item => item.Answers)
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken)
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
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeleteAnswerAsync(
        Guid dialogueId,
        Guid questionId,
        Guid answerId,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DialogueAnswer? answer = await dbContext.DialogueAnswers
            .SingleOrDefaultAsync(item => item.DialogueQuestionId == questionId && item.Id == answerId, cancellationToken)
            .ConfigureAwait(false);

        bool questionBelongsToDialogue = await dbContext.DialogueQuestions
            .AnyAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (answer is not null && questionBelongsToDialogue)
        {
            dbContext.DialogueAnswers.Remove(answer);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddDialogueTurnTranslationAsync(
        Guid dialogueId,
        Guid turnId,
        AdminAddDialogueTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        DialogueTurn? turn = await dbContext.DialogueTurns
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == turnId, cancellationToken)
            .ConfigureAwait(false);

        if (turn is null)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        DialogueTurnTranslation? existingTranslation = turn.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<DialogueTurnTranslation>().Remove(existingTranslation);
        }

        turn.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeleteDialogueTurnTranslationAsync(
        Guid dialogueId,
        Guid turnId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<DialogueTurnTranslation>(
            dialogueId,
            turnId,
            translationId,
            dbContext => dbContext.DialogueTurns.AnyAsync(item => item.DialogueLessonId == dialogueId && item.Id == turnId, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddPhraseTranslationAsync(
        Guid dialogueId,
        Guid phraseId,
        AdminAddDialogueTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        DialoguePhrase? phrase = await dbContext.DialoguePhrases
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == phraseId, cancellationToken)
            .ConfigureAwait(false);

        if (phrase is null)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        DialoguePhraseTranslation? existingTranslation = phrase.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<DialoguePhraseTranslation>().Remove(existingTranslation);
        }

        phrase.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeletePhraseTranslationAsync(
        Guid dialogueId,
        Guid phraseId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<DialoguePhraseTranslation>(
            dialogueId,
            phraseId,
            translationId,
            dbContext => dbContext.DialoguePhrases.AnyAsync(item => item.DialogueLessonId == dialogueId && item.Id == phraseId, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddQuestionTranslationAsync(
        Guid dialogueId,
        Guid questionId,
        AdminAddDialogueTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        DialogueQuestion? question = await dbContext.DialogueQuestions
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (question is null)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        DialogueQuestionTranslation? existingTranslation = question.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<DialogueQuestionTranslation>().Remove(existingTranslation);
        }

        question.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeleteQuestionTranslationAsync(
        Guid dialogueId,
        Guid questionId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<DialogueQuestionTranslation>(
            dialogueId,
            questionId,
            translationId,
            dbContext => dbContext.DialogueQuestions.AnyAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> AddAnswerTranslationAsync(
        Guid dialogueId,
        Guid questionId,
        Guid answerId,
        AdminAddDialogueTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        DialogueAnswer? answer = await dbContext.DialogueAnswers
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.DialogueQuestionId == questionId && item.Id == answerId, cancellationToken)
            .ConfigureAwait(false);

        bool questionBelongsToDialogue = await dbContext.DialogueQuestions
            .AnyAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken)
            .ConfigureAwait(false);

        if (answer is null || !questionBelongsToDialogue)
        {
            return null;
        }

        LanguageCode languageCode = LanguageCode.From(request.LanguageCode);
        DialogueAnswerTranslation? existingTranslation = answer.Translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);
        if (existingTranslation is not null)
        {
            dbContext.Set<DialogueAnswerTranslation>().Remove(existingTranslation);
        }

        answer.AddTranslation(Guid.NewGuid(), languageCode, request.Text, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminDialogueDetailResponse?> DeleteAnswerTranslationAsync(
        Guid dialogueId,
        Guid questionId,
        Guid answerId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        await DeleteTranslationAsync<DialogueAnswerTranslation>(
            dialogueId,
            answerId,
            translationId,
            async dbContext =>
            {
                bool questionBelongsToDialogue = await dbContext.DialogueQuestions
                    .AnyAsync(item => item.DialogueLessonId == dialogueId && item.Id == questionId, cancellationToken)
                    .ConfigureAwait(false);

                bool answerBelongsToQuestion = await dbContext.DialogueAnswers
                    .AnyAsync(item => item.DialogueQuestionId == questionId && item.Id == answerId, cancellationToken)
                    .ConfigureAwait(false);

                return questionBelongsToDialogue && answerBelongsToQuestion;
            },
            cancellationToken).ConfigureAwait(false);

        return await GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminBulkDialogueImportResponse> ImportDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Dialogues.Count == 0)
        {
            throw new InvalidOperationException("The import file must contain at least one dialogue.");
        }

        List<AdminBulkDialogueImportItemResult> results = [];
        int importedCount = 0;
        int skippedCount = 0;
        int failedCount = 0;

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        for (int index = 0; index < request.Dialogues.Count; index++)
        {
            AdminBulkDialogueImportItemRequest item = request.Dialogues[index];
            int rowNumber = index + 1;

            try
            {
                ValidateCompleteDialogueImport(item);

                string slug = item.Slug.Trim().ToLowerInvariant();
                DialogueLesson? existing = await dbContext.DialogueLessons
                    .SingleOrDefaultAsync(dialogue => dialogue.Slug == slug, cancellationToken)
                    .ConfigureAwait(false);

                Guid dialogueId = existing?.Id ?? Guid.NewGuid();
                bool created = existing is null;

                DateTime now = DateTime.UtcNow;
                DialogueLesson dialogue = new(
                    dialogueId,
                    item.Slug,
                    item.Title,
                    item.Description,
                    item.LearnerGoal,
                    ParseCefr(string.IsNullOrWhiteSpace(item.CefrLevel) ? "A1" : item.CefrLevel),
                    string.IsNullOrWhiteSpace(item.Category) ? "general" : item.Category,
                    ParseStatus(string.IsNullOrWhiteSpace(item.PublicationStatus) ? "Draft" : item.PublicationStatus),
                    item.SortOrder,
                    now);

                foreach (AdminBulkDialogueTurnImportRequest turnItem in item.DialogueTurns ?? [])
                {
                    int sortOrder = turnItem.SortOrder <= 0 ? dialogue.DialogueTurns.Count + 1 : turnItem.SortOrder;
                    DialogueTurn turn = dialogue.AddDialogueTurn(Guid.NewGuid(), sortOrder, turnItem.SpeakerRole, turnItem.BaseText, now);
                    foreach (AdminAddDialogueTranslationRequest translation in turnItem.Translations ?? [])
                    {
                        turn.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                    }
                }

                foreach (AdminBulkDialoguePhraseImportRequest phraseItem in item.UsefulPhrases ?? [])
                {
                    int sortOrder = phraseItem.SortOrder <= 0 ? dialogue.UsefulPhrases.Count + 1 : phraseItem.SortOrder;
                    DialoguePhrase phrase = dialogue.AddUsefulPhrase(Guid.NewGuid(), sortOrder, phraseItem.BaseText, phraseItem.UsageNote, now);
                    foreach (AdminAddDialogueTranslationRequest translation in phraseItem.Translations ?? [])
                    {
                        phrase.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                    }
                }

                foreach (AdminBulkDialogueQuestionImportRequest questionItem in item.Questions ?? [])
                {
                    int sortOrder = questionItem.SortOrder <= 0 ? dialogue.Questions.Count + 1 : questionItem.SortOrder;
                    DialogueQuestion question = dialogue.AddQuestion(Guid.NewGuid(), sortOrder, questionItem.Prompt, now);
                    foreach (AdminAddDialogueTranslationRequest translation in questionItem.Translations ?? [])
                    {
                        question.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                    }

                    foreach (AdminBulkDialogueAnswerImportRequest answerItem in questionItem.Answers ?? [])
                    {
                        int answerSortOrder = answerItem.SortOrder <= 0 ? question.Answers.Count + 1 : answerItem.SortOrder;
                        DialogueAnswer answer = question.AddAnswer(Guid.NewGuid(), answerSortOrder, answerItem.Text, answerItem.IsCorrect, answerItem.Feedback, now);
                        foreach (AdminAddDialogueTranslationRequest translation in answerItem.Translations ?? [])
                        {
                            answer.AddTranslation(Guid.NewGuid(), LanguageCode.From(translation.LanguageCode), translation.Text, now);
                        }
                    }
                }

                await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
                    await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                if (existing is not null)
                {
                    dbContext.DialogueLessons.Remove(existing);
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                dbContext.DialogueLessons.Add(dialogue);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                importedCount++;
                results.Add(new AdminBulkDialogueImportItemResult(
                    rowNumber,
                    item.Slug,
                    dialogue.Id,
                    created ? "Imported" : "Updated",
                    created ? "Dialogue was created." : "Dialogue was replaced with the imported content."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DomainRuleException)
            {
                failedCount++;
                results.Add(new AdminBulkDialogueImportItemResult(rowNumber, item.Slug, null, "Failed", ex.Message));
            }
            catch (DbUpdateException ex)
            {
                failedCount++;
                results.Add(new AdminBulkDialogueImportItemResult(rowNumber, item.Slug, null, "Failed", ex.InnerException?.Message ?? ex.Message));
            }
        }

        return new AdminBulkDialogueImportResponse(
            request.Dialogues.Count,
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

    private static void ValidateCompleteDialogueImport(AdminBulkDialogueImportItemRequest item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new InvalidOperationException("Dialogue title is required.");
        }

        if (string.IsNullOrWhiteSpace(item.Description))
        {
            throw new InvalidOperationException("Dialogue description is required.");
        }

        if (string.IsNullOrWhiteSpace(item.LearnerGoal))
        {
            throw new InvalidOperationException("Dialogue learner goal is required.");
        }

        if (item.DialogueTurns is null || item.DialogueTurns.Count == 0)
        {
            throw new InvalidOperationException("Each dialogue must include at least one dialogue turn.");
        }

        if (item.UsefulPhrases is null || item.UsefulPhrases.Count == 0)
        {
            throw new InvalidOperationException("Each dialogue must include at least one useful phrase.");
        }

        if (item.Questions is null || item.Questions.Count == 0)
        {
            throw new InvalidOperationException("Each dialogue must include at least one practice question.");
        }

        for (int index = 0; index < item.DialogueTurns.Count; index++)
        {
            AdminBulkDialogueTurnImportRequest turn = item.DialogueTurns[index];
            if (string.IsNullOrWhiteSpace(turn.BaseText))
            {
                throw new InvalidOperationException($"Dialogue turn {index + 1} must include German base text.");
            }

            ValidateCompleteDialogueTranslations(turn.Translations, $"Dialogue turn {index + 1} translations");
        }

        for (int index = 0; index < item.UsefulPhrases.Count; index++)
        {
            AdminBulkDialoguePhraseImportRequest phrase = item.UsefulPhrases[index];
            if (string.IsNullOrWhiteSpace(phrase.BaseText))
            {
                throw new InvalidOperationException($"Useful phrase {index + 1} must include German base text.");
            }

            ValidateCompleteDialogueTranslations(phrase.Translations, $"Useful phrase {index + 1} translations");
        }

        for (int questionIndex = 0; questionIndex < item.Questions.Count; questionIndex++)
        {
            AdminBulkDialogueQuestionImportRequest question = item.Questions[questionIndex];
            if (string.IsNullOrWhiteSpace(question.Prompt))
            {
                throw new InvalidOperationException($"Question {questionIndex + 1} must include German prompt text.");
            }

            ValidateCompleteDialogueTranslations(question.Translations, $"Question {questionIndex + 1} translations");

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
                AdminBulkDialogueAnswerImportRequest answer = question.Answers[answerIndex];
                if (string.IsNullOrWhiteSpace(answer.Text))
                {
                    throw new InvalidOperationException($"Question {questionIndex + 1} answer {answerIndex + 1} must include German text.");
                }

                ValidateCompleteDialogueTranslations(
                    answer.Translations,
                    $"Question {questionIndex + 1} answer {answerIndex + 1} translations");
            }
        }
    }

    private static void ValidateCompleteDialogueTranslations(
        IReadOnlyList<AdminAddDialogueTranslationRequest>? translations,
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
        Guid dialogueId,
        Guid ownerId,
        Guid translationId,
        Func<DarwinLinguaDbContext, Task<bool>> ownerBelongsToDialogue,
        CancellationToken cancellationToken)
        where TTranslation : DialogueTranslationBase
    {
        if (dialogueId == Guid.Empty || ownerId == Guid.Empty || translationId == Guid.Empty)
        {
            return;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        bool validOwner = await ownerBelongsToDialogue(dbContext).ConfigureAwait(false);
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

    private static AdminDialogueDetailResponse MapDetail(DialogueLesson dialogue) =>
        new(
            dialogue.Id,
            dialogue.Slug,
            dialogue.Title,
            dialogue.Description,
            dialogue.LearnerGoal,
            dialogue.CefrLevel.ToString(),
            dialogue.Category,
            dialogue.PublicationStatus.ToString(),
            dialogue.SortOrder,
            dialogue.CreatedAtUtc,
            dialogue.UpdatedAtUtc,
            dialogue.DialogueTurns
                .OrderBy(turn => turn.SortOrder)
                .Select(turn => new AdminDialogueTurnResponse(
                    turn.Id,
                    turn.SortOrder,
                    turn.SpeakerRole,
                    turn.BaseText,
                    MapTranslations(turn.Translations)))
                .ToArray(),
            dialogue.UsefulPhrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new AdminDialoguePhraseResponse(
                    phrase.Id,
                    phrase.SortOrder,
                    phrase.BaseText,
                    phrase.UsageNote,
                    MapTranslations(phrase.Translations)))
                .ToArray(),
            dialogue.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => new AdminDialogueQuestionResponse(
                    question.Id,
                    question.SortOrder,
                    question.Prompt,
                    MapTranslations(question.Translations),
                    question.Answers
                        .OrderBy(answer => answer.SortOrder)
                        .Select(answer => new AdminDialogueAnswerResponse(
                            answer.Id,
                            answer.SortOrder,
                            answer.Text,
                            answer.IsCorrect,
                            answer.Feedback,
                            MapTranslations(answer.Translations)))
                        .ToArray()))
                .ToArray());

    private static AdminDialogueTranslationResponse[] MapTranslations<TTranslation>(
        IEnumerable<TTranslation> translations)
        where TTranslation : DialogueTranslationBase =>
        translations
            .OrderBy(translation => translation.LanguageCode.Value)
            .Select(translation => new AdminDialogueTranslationResponse(
                translation.Id,
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
}
