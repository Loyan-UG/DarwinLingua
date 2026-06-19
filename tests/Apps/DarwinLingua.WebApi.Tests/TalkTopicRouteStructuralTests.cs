namespace DarwinLingua.WebApi.Tests;

using System.Xml.Linq;
using Xunit;

public sealed class TalkTopicRouteStructuralTests
{
    [Fact]
    public void WebTalkTopicDetail_ShouldOnlyLinkResolvedVocabularyWithWordRouteParameter()
    {
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "TalkTopics", "Detail.cshtml"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "TalkTopics", "Index.cshtml"));

        Assert.Contains("item.IsResolved && !string.IsNullOrWhiteSpace(item.WordSlug)", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-route-wordSlug=\"@item.WordSlug\"", detailSource, StringComparison.Ordinal);
        Assert.DoesNotContain("asp-route-slug=\"@wordSlug\"", detailSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LemmaUrlSlug.FromLemma(item.Lemma)", detailSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(Model.TalkTopic.ContentType, \"article\", StringComparison.Ordinal)", detailSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(talkTopic.ContentType, \"article\", StringComparison.Ordinal)", indexSource, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("SharedResource.en.resx")]
    [InlineData("SharedResource.de.resx")]
    public void SharedResources_ShouldCoverTalkTopicLabelsAndEnums(string resourceFileName)
    {
        string resourcePath = ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", resourceFileName);
        XDocument document = XDocument.Load(resourcePath);
        HashSet<string> resourceKeys = document.Root!
            .Elements("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal)!;

        string[] requiredKeys =
        [
            "Talk Topics",
            "All Talk Topics",
            "Open Talk Topic",
            "Reading topics for real conversation",
            "Read a short German text, review important words, then discuss opinions and ideas together.",
            "All content types",
            "Content type",
            "German text",
            "Warm-up questions",
            "Discussion questions",
            "Important words",
            "Talk Topics are being prepared",
            "Reading-based conversation materials will appear here when they are ready.",
            "Sensitive topic",
            "This topic may need a careful and respectful group conversation.",
            "Recommended for moderated groups only.",
            "{0} min read",
            "{0} min discussion",
            "Article",
            "book-summary",
            "movie-summary",
            "story",
            "fact-sheet",
            "opinion-text",
            "interview",
            "debate-text",
            "opinion",
            "personal-experience",
            "prediction",
            "comparison",
            "imagination",
            "debate",
            "ethics",
            "comprehension",
            "express-opinion",
            "give-reasons",
            "agree-disagree",
            "ask-follow-up-questions",
            "compare-options",
            "make-predictions",
            "describe-experiences",
            "imagine-possibilities",
            "debate-politely",
            "summarize-position",
        ];

        foreach (string requiredKey in requiredKeys)
        {
            Assert.Contains(requiredKey, resourceKeys);
        }
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine(new[] { directory }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository file '{string.Join(Path.DirectorySeparatorChar, segments)}'.");
    }
}
