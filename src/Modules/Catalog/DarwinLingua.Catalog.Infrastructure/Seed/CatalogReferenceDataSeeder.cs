using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DarwinLingua.Catalog.Infrastructure.Seed;

/// <summary>
/// Seeds the stable Phase 1 topic reference data into the shared SQLite database.
/// </summary>
internal sealed class CatalogReferenceDataSeeder : IDatabaseSeeder
{
    private static readonly TopicSeedDefinition[] SeedDefinitions =
    [
        new(new Guid("6F1464F0-9807-420F-A86E-41731D11A001"), "everyday-life", 10, true,
            [new("de", "Alltag"), new("ar", "الحياة اليومية"), new("ckb", "ژیانی ڕۆژانە"), new("en", "Everyday Life"),
             new("fa", "زندگی روزمره"), new("kmr", "Jiyana rojane"), new("pl", "Zycie codzienne"),
             new("ro", "Viata de zi cu zi"), new("ru", "Повседневная жизнь"), new("sq", "Jeta e perditshme"),
             new("tr", "Gunluk yasam")]),
        new(new Guid("6F1464F0-9807-420F-A86E-41731D11A002"), "housing", 20, true,
            [new("de", "Wohnen"), new("ar", "السكن"), new("ckb", "نیشتەجێبوون"), new("en", "Housing"),
             new("fa", "مسکن"), new("kmr", "Xani u rûniştin"), new("pl", "Mieszkanie"),
             new("ro", "Locuire"), new("ru", "Жилье"), new("sq", "Banimi"), new("tr", "Konut")]),
        new(new Guid("6F1464F0-9807-420F-A86E-41731D11A003"), "shopping", 30, true,
            [new("de", "Einkaufen"), new("ar", "التسوق"), new("ckb", "کڕین"), new("en", "Shopping"),
             new("fa", "خرید"), new("kmr", "Kirrin"), new("pl", "Zakupy"), new("ro", "Cumparaturi"),
             new("ru", "Покупки"), new("sq", "Blerje"), new("tr", "Alisveris")]),
        new(new Guid("6F1464F0-9807-420F-A86E-41731D11A004"), "work-and-jobs", 40, true,
            [new("de", "Arbeit und Beruf"), new("ar", "العمل والوظائف"), new("ckb", "کار و پیشە"), new("en", "Work and Jobs"),
             new("fa", "کار و شغل"), new("kmr", "Kar û pîşe"), new("pl", "Praca i zawody"),
             new("ro", "Munca si joburi"), new("ru", "Работа и профессии"), new("sq", "Puna dhe vendet e punes"),
             new("tr", "Is ve meslekler")]),
        new(new Guid("6F1464F0-9807-420F-A86E-41731D11A005"), "appointments-and-health", 50, true,
            [new("de", "Termine und Gesundheit"), new("ar", "المواعيد والصحة"), new("ckb", "کات و تەندروستی"), new("en", "Appointments and Health"),
             new("fa", "قرارها و سلامت"), new("kmr", "Hevdîtin û tenduristî"), new("pl", "Terminy i zdrowie"),
             new("ro", "Programari si sanatate"), new("ru", "Встречи и здоровье"), new("sq", "Takime dhe shendet"),
             new("tr", "Randevular ve saglik")]),
    ];

    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogReferenceDataSeeder"/> class.
    /// </summary>
    public CatalogReferenceDataSeeder(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<Topic> existingTopics = await dbContext.Topics
            .Include(topic => topic.Localizations)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime timestampUtc = DateTime.UtcNow;

        foreach (TopicSeedDefinition seedDefinition in SeedDefinitions)
        {
            Topic? existingTopic = existingTopics.SingleOrDefault(topic => topic.Key == seedDefinition.Key);

            if (existingTopic is null)
            {
                Topic newTopic = new(
                    seedDefinition.Id,
                    seedDefinition.Key,
                    seedDefinition.SortOrder,
                    seedDefinition.IsSystem,
                    timestampUtc);

                foreach (TopicLocalizationSeedDefinition localization in seedDefinition.Localizations)
                {
                    newTopic.AddOrUpdateLocalization(
                        CreateLocalizationId(seedDefinition.Id, localization.LanguageCode),
                        LanguageCode.From(localization.LanguageCode),
                        localization.DisplayName,
                        timestampUtc);
                }

                dbContext.Topics.Add(newTopic);
                continue;
            }

            foreach (TopicLocalizationSeedDefinition localization in seedDefinition.Localizations)
            {
                LanguageCode languageCode = LanguageCode.From(localization.LanguageCode);
                if (existingTopic.FindLocalization(languageCode) is not null)
                {
                    continue;
                }

                existingTopic.AddOrUpdateLocalization(
                    CreateLocalizationId(existingTopic.Id, localization.LanguageCode),
                    languageCode,
                    localization.DisplayName,
                    timestampUtc);
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed record TopicSeedDefinition(
        Guid Id,
        string Key,
        int SortOrder,
        bool IsSystem,
        IReadOnlyList<TopicLocalizationSeedDefinition> Localizations);

    private sealed record TopicLocalizationSeedDefinition(
        string LanguageCode,
        string DisplayName);

    private static Guid CreateLocalizationId(Guid topicId, string languageCode)
    {
        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes($"topic:{topicId:D}:{languageCode.Trim().ToLowerInvariant()}"));
        return new Guid(bytes);
    }
}
