using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Localization.Infrastructure.Seed;

/// <summary>
/// Seeds the stable language reference data required by the Phase 1 product.
/// </summary>
internal sealed class LocalizationReferenceDataSeeder : IDatabaseSeeder
{
    private static readonly LanguageSeedDefinition[] SeedDefinitions =
    [
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC001"),
            "en",
            "English",
            "English",
            IsActive: true,
            SupportsUserInterface: true,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC002"),
            "de",
            "German",
            "Deutsch",
            IsActive: true,
            SupportsUserInterface: true,
            SupportsMeanings: false),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC003"),
            "fa",
            "Persian",
            "فارسی",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC004"),
            "ru",
            "Russian",
            "Русский",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC005"),
            "ar",
            "Arabic",
            "العربية",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC006"),
            "pl",
            "Polish",
            "Polski",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC007"),
            "tr",
            "Turkish",
            "Türkçe",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC008"),
            "ro",
            "Romanian",
            "Română",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC009"),
            "sq",
            "Albanian",
            "Shqip",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC010"),
            "ckb",
            "Kurdish (Sorani)",
            "کوردی سۆرانی",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
        new(
            new Guid("0F6A5E4E-7A77-49E7-B637-5E07A46CC011"),
            "kmr",
            "Kurdish (Kurmanji)",
            "Kurmancî",
            IsActive: true,
            SupportsUserInterface: false,
            SupportsMeanings: true),
    ];

    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationReferenceDataSeeder"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The context factory used to access the database.</param>
    public LocalizationReferenceDataSeeder(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
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

        List<Language> existingLanguages = await dbContext.Languages
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (LanguageSeedDefinition seedDefinition in SeedDefinitions)
        {
            LanguageCode languageCode = LanguageCode.From(seedDefinition.Code);
            Language? existingLanguage = existingLanguages
                .SingleOrDefault(language => language.Code == languageCode);

            if (existingLanguage is null)
            {
                dbContext.Languages.Add(new Language(
                    seedDefinition.Id,
                    languageCode,
                    seedDefinition.EnglishName,
                    seedDefinition.NativeName,
                    seedDefinition.IsActive,
                    seedDefinition.SupportsUserInterface,
                    seedDefinition.SupportsMeanings));

                continue;
            }

            continue;
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Stores the static language seed definition values.
    /// </summary>
    /// <param name="id">The stable identifier of the language row.</param>
    /// <param name="code">The normalized language code.</param>
    /// <param name="englishName">The English display name.</param>
    /// <param name="nativeName">The native display name.</param>
    /// <param name="isActive">A value indicating whether the language is active.</param>
    /// <param name="supportsUserInterface">A value indicating whether the language is supported for UI localization.</param>
    /// <param name="supportsMeanings">A value indicating whether the language is supported for meanings.</param>
    private sealed record LanguageSeedDefinition(
        Guid Id,
        string Code,
        string EnglishName,
        string NativeName,
        bool IsActive,
        bool SupportsUserInterface,
        bool SupportsMeanings);
}
