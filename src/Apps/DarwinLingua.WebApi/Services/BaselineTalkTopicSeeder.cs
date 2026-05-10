using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IBaselineTalkTopicSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}

internal sealed class BaselineTalkTopicSeeder(
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
    ILogger<BaselineTalkTopicSeeder> logger) : IBaselineTalkTopicSeeder
{
    private const string SampleSlug = "a1-gibt-es-ausserirdische";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        TalkTopic[] existingSamples = await dbContext.TalkTopics
            .Where(topic => topic.Slug == SampleSlug)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        dbContext.TalkTopics.RemoveRange(existingSamples);

        Dictionary<string, Guid> topicIdsByKey = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Key == "everyday-life")
            .ToDictionaryAsync(topic => topic.Key, topic => topic.Id, cancellationToken)
            .ConfigureAwait(false);

        if (topicIdsByKey.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        TalkTopic topic = new(
            Guid.NewGuid(),
            SampleSlug,
            "gibt-es-ausserirdische",
            "Gibt es Außerirdische?",
            "Ein einfacher Talk Topic über Sterne, Planeten und die Frage, ob es Leben im Weltall gibt.",
            CefrLevel.A1,
            "space",
            TalkTopicContentType.Article,
            SampleArticle,
            5,
            20,
            false,
            null,
            false,
            PublicationStatus.Active,
            10,
            now);

        topic.AddTopic(Guid.NewGuid(), topicIdsByKey["everyday-life"], true, now);

        topic.AddWarmupQuestion(Guid.NewGuid(), 10, "Schaust du gern in den Himmel?", now);
        topic.AddWarmupQuestion(Guid.NewGuid(), 20, "Magst du Sterne und Planeten?", now);
        topic.AddWarmupQuestion(Guid.NewGuid(), 30, "Kennst du einen Film über das Weltall?", now);

        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Opinion, 10, "Glaubst du, dass es Leben auf anderen Planeten gibt?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Opinion, 20, "Findest du das Thema spannend oder komisch?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Imagination, 30, "Wie könnten Außerirdische aussehen?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Imagination, 40, "Was würdest du einem Besucher aus dem Weltall zeigen?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Prediction, 50, "Werden Menschen einmal Leben auf einem anderen Planeten finden?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Prediction, 60, "Was passiert, wenn es einen klaren Kontakt gibt?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Comparison, 70, "Ist das Leben auf der Erde vielleicht besonders?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Comparison, 80, "Was ist anders: ein Film über Aliens oder echte Forschung?", now);

        topic.AddVocabularyItem(Guid.NewGuid(), "der Himmel", "der-himmel", CefrLevel.A1, 10, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Stern", "der-stern", CefrLevel.A1, 20, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Mond", "der-mond", CefrLevel.A1, 30, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "die Erde", "die-erde", CefrLevel.A1, 40, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Planet", "der-planet", CefrLevel.A2, 50, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "das Weltall", "das-weltall", CefrLevel.A2, 60, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "das Leben", "das-leben", CefrLevel.A1, 70, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "das Wasser", "das-wasser", CefrLevel.A1, 80, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "suchen", "suchen", CefrLevel.A1, 90, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "finden", "finden", CefrLevel.A1, 100, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "glauben", "glauben", CefrLevel.A1, 110, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Kontakt", "der-kontakt", CefrLevel.A2, 120, now);

        topic.AddSpeakingGoal(Guid.NewGuid(), TalkTopicSpeakingGoal.ExpressOpinion, 10, now);
        topic.AddSpeakingGoal(Guid.NewGuid(), TalkTopicSpeakingGoal.GiveReasons, 20, now);
        topic.AddSpeakingGoal(Guid.NewGuid(), TalkTopicSpeakingGoal.ImaginePossibilities, 30, now);

        dbContext.TalkTopics.Add(topic);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Seeded baseline Talk Topic {Slug}.", SampleSlug);
    }

    private const string SampleArticle = """
        Am Abend ist der Himmel dunkel. Viele Menschen sehen dann den Mond und viele Sterne. Sie fragen sich: Gibt es Leben auf anderen Planeten? Die Erde ist ein Planet. Hier gibt es Wasser, Luft, Tiere, Pflanzen und Menschen. Vielleicht gibt es im Weltall noch andere Orte mit Wasser und Licht.

        Niemand kennt die sichere Antwort. Wissenschaftler suchen mit Teleskopen und Computern. Sie suchen Zeichen von Leben. Vielleicht ist dieses Leben sehr klein. Vielleicht sieht es ganz anders aus als wir. Vielleicht gibt es auch kluge Wesen, die sprechen und bauen können.

        In einer Gruppe kann man gut darüber sprechen. Eine Person sagt: Ja, ich glaube daran, denn das Weltall ist sehr groß. Eine andere Person sagt: Ich bin nicht sicher, denn wir haben noch keinen Beweis. Wichtig ist, dass alle langsam sprechen und freundlich nachfragen.

        Das Thema ist gut für Fantasie. Wie sieht ein Besucher aus dem Weltall aus? Was möchte er wissen? Was zeigen wir ihm zuerst: unsere Stadt, unsere Musik oder unser Essen? Am Ende gibt es keine richtige Lösung. Es zählt das Gespräch.
        """;
}
