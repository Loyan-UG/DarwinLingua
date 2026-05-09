using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
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

        bool exists = await dbContext.TalkTopics
            .AsNoTracking()
            .AnyAsync(topic => topic.Slug == SampleSlug, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
        {
            return;
        }

        Dictionary<string, Guid> topicIdsByKey = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Key == "technology-and-it" || topic.Key == "culture-and-media")
            .ToDictionaryAsync(topic => topic.Key, topic => topic.Id, cancellationToken)
            .ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        TalkTopic topic = new(
            Guid.NewGuid(),
            SampleSlug,
            "gibt-es-ausserirdische",
            "Gibt es Ausserirdische?",
            "Ein einfacher Talk Topic ueber das Weltall, Sterne und die Frage, ob es Leben auf anderen Planeten gibt.",
            CefrLevel.A1,
            "science-and-imagination",
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

        bool primaryTopicAssigned = false;
        foreach ((string key, Guid topicId) in topicIdsByKey.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            topic.AddTopic(Guid.NewGuid(), topicId, !primaryTopicAssigned, now);
            primaryTopicAssigned = true;
        }

        topic.AddArticleTranslation(Guid.NewGuid(), LanguageCode.From("en"), SampleArticleEnglish, now);
        topic.AddWarmupQuestion(Guid.NewGuid(), 10, "Schaust du gern in den Nachthimmel?", now)
            .AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "Do you like looking at the night sky?", now);
        topic.AddWarmupQuestion(Guid.NewGuid(), 20, "Magst du Filme ueber Ausserirdische?", now)
            .AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "Do you like films about aliens?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Opinion, 10, "Glaubst du, dass es Ausserirdische gibt?", now)
            .AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "Do you believe aliens exist?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Imagination, 20, "Wie koennten Ausserirdische aussehen?", now)
            .AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "What might aliens look like?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Debate, 30, "Sollten Menschen nach Ausserirdischen suchen?", now)
            .AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "Should humans search for aliens?", now);
        topic.AddDiscussionQuestion(Guid.NewGuid(), TalkTopicQuestionType.Prediction, 40, "Was wuerde sich auf der Erde aendern, wenn wir Kontakt haetten?", now)
            .AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "What would change on Earth if we had contact?", now);

        topic.AddVocabularyItem(Guid.NewGuid(), "der Himmel", "der-himmel", CefrLevel.A1, 10, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Stern", "der-stern", CefrLevel.A1, 20, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Planet", "der-planet", CefrLevel.A2, 30, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "das Weltall", "das-weltall", CefrLevel.A2, 40, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "ausserirdisch", "ausserirdisch", CefrLevel.B1, 50, now);
        topic.AddVocabularyItem(Guid.NewGuid(), "der Kontakt", "der-kontakt", CefrLevel.A2, 60, now);

        topic.AddSpeakingGoal(Guid.NewGuid(), TalkTopicSpeakingGoal.ExpressOpinion, 10, now);
        topic.AddSpeakingGoal(Guid.NewGuid(), TalkTopicSpeakingGoal.GiveReasons, 20, now);
        topic.AddSpeakingGoal(Guid.NewGuid(), TalkTopicSpeakingGoal.ImaginePossibilities, 30, now);

        dbContext.TalkTopics.Add(topic);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Seeded baseline Talk Topic {Slug}.", SampleSlug);
    }

    private const string SampleArticle = """
        Am Abend ist der Himmel oft dunkel. Viele Menschen schauen dann nach oben. Sie sehen den Mond und viele Sterne. Manche Sterne sind sehr hell. Andere Sterne sind klein und weit weg. Fuer viele Menschen ist der Nachthimmel schoen. Er ist ruhig, gross und ein bisschen geheimnisvoll.

        Im Weltall gibt es viele Sterne und viele Planeten. Die Erde ist auch ein Planet. Auf der Erde leben Menschen, Tiere und Pflanzen. Wir haben Wasser, Luft, Licht und Waerme. Darum koennen wir hier leben. Aber die Erde ist nicht der einzige Planet. Es gibt sehr viele andere Planeten. Einige sind kalt. Einige sind heiss. Einige sind sehr gross. Einige sind sehr klein.

        Viele Menschen fragen: Gibt es Leben auf anderen Planeten? Gibt es Ausserirdische? Niemand weiss die Antwort sicher. Wissenschaftler suchen mit grossen Teleskopen und Computern. Sie hoeren Signale aus dem Weltall. Sie suchen Wasser auf anderen Planeten. Wasser ist wichtig, weil Leben oft Wasser braucht.

        Ausserirdische muessen nicht wie Menschen aussehen. Vielleicht sind sie klein. Vielleicht sind sie gross. Vielleicht haben sie andere Augen oder andere Koerper. Vielleicht sind sie ganz einfach und leben wie kleine Pflanzen oder Tiere. Vielleicht sind sie sehr klug und bauen eigene Staedte. Wir wissen es nicht. Darum ist die Frage spannend.

        Manche Menschen denken, Ausserirdische waeren freundlich. Sie koennten uns neue Ideen geben. Vielleicht koennten wir von ihnen lernen. Andere Menschen sind vorsichtig. Sie sagen: Wir kennen sie nicht. Wir wissen nicht, ob ein Kontakt gut oder gefaehrlich ist. Beide Meinungen sind wichtig fuer ein Gespraech.

        In einer Gruppe kann man gut ueber dieses Thema sprechen. Man kann sagen: Ich glaube, es gibt Ausserirdische, weil das Weltall sehr gross ist. Oder man kann sagen: Ich bin nicht sicher, weil wir noch keinen klaren Beweis haben. Man kann auch fragen: Wie wuerde die Welt reagieren? Wuerden Menschen Angst haben? Wuerden sie neugierig sein?

        Das Thema hilft beim Sprechen. Man kann die eigene Meinung sagen, Gruende nennen und neue Ideen vorstellen. Man kann freundlich widersprechen und andere Fragen stellen. Am Ende muss niemand die richtige Antwort haben. Wichtig ist, dass alle sprechen, zuhoeren und zusammen ueber eine grosse Frage nachdenken.
        """;

    private const string SampleArticleEnglish = """
        In the evening the sky is often dark. Many people look up and see the moon and many stars. The universe has many stars and planets. Earth is also a planet, and people, animals, and plants live here. Many people ask whether life exists on other planets. Scientists search for signals and water, but nobody knows the answer for sure. Aliens might not look like people. They might be simple, or they might be very intelligent. Some people think aliens would be friendly. Other people are careful because contact could also be difficult. This topic helps a group express opinions, give reasons, imagine possibilities, disagree politely, and ask follow-up questions.
        """;
}
