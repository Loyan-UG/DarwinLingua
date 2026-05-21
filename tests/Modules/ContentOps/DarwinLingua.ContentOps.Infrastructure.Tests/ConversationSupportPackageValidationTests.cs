using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ConversationSupportPackageValidationTests
{
    private static readonly string[] TargetLanguages =
    [
        "en",
        "fa",
        "ar",
        "tr",
        "ru",
        "ckb",
        "kmr",
        "pl",
        "ro",
        "sq"
    ];

    [Fact]
    public async Task ParseAsync_ShouldParseConversationSupportBaselinePackage()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();
        string packagePath = Path.Combine(
            FindRepositoryRoot(),
            "content",
            "generated",
            "conversation-support",
            "conversation-support-baseline-v1.json");

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            await File.ReadAllTextAsync(packagePath),
            CancellationToken.None);

        Assert.Equal(6, parsedPackage.ConversationStarterPacks.Count);
        Assert.Equal(8, parsedPackage.EventPreparationPacks.Count);

        foreach (ParsedConversationStarterPackModel pack in parsedPackage.ConversationStarterPacks)
        {
            Assert.NotEmpty(pack.Phrases);
            foreach (ParsedConversationStarterPhraseModel phrase in pack.Phrases)
            {
                Assert.Equal(
                    TargetLanguages.OrderBy(language => language, StringComparer.Ordinal).ToArray(),
                    phrase.Translations.Select(translation => translation.Language)
                        .OrderBy(language => language, StringComparer.Ordinal)
                        .ToArray());
            }
        }

        foreach (ParsedEventPreparationPackModel pack in parsedPackage.EventPreparationPacks)
        {
            Assert.NotEmpty(pack.OpeningPrompts);
            Assert.NotEmpty(pack.RoleplayPrompts);
            Assert.NotEmpty(pack.ReviewPrompts);
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "content"))
                && Directory.Exists(Path.Combine(directory.FullName, "src"))
                && Directory.Exists(Path.Combine(directory.FullName, "tests")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the DarwinLingua repository root.");
    }
}
