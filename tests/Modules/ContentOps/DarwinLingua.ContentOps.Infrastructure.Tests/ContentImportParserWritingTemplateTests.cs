using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserWritingTemplateTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseWritingTemplateContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection().AddContentOpsInfrastructure().BuildServiceProvider();
        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "writing-template-contract-test",
              "packageName": "Writing Template Contract Test",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "writingTemplates": [
                {
                  "slug": "a2-school-absence-email",
                  "title": "School absence email",
                  "shortDescription": "A short email to explain a child absence.",
                  "cefrLevel": "A2",
                  "category": "email-to-school",
                  "situation": "Informing a school about absence",
                  "register": "formal",
                  "templateText": "Sehr geehrte Damen und Herren, {{child-name}} ist heute krank.",
                  "explanation": "Use a formal greeting and a clear reason.",
                  "replaceableVariables": ["child-name"],
                  "sampleFilledVersion": "Sehr geehrte Damen und Herren, Sara ist heute krank.",
                  "linkedGrammarTopicSlugs": ["a2-word-order"],
                  "linkedWordSlugs": ["krank"],
                  "linkedExpressionSlugs": ["sehr-geehrte-damen-und-herren"],
                  "linkedExerciseSlugs": ["a2-formal-email-practice"],
                  "sortOrder": 10
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedWritingTemplateModel template = Assert.Single(parsedPackage.WritingTemplates);
        Assert.Equal("a2-school-absence-email", template.Slug);
        Assert.Equal("child-name", Assert.Single(template.ReplaceableVariables));
        Assert.Equal("a2-formal-email-practice", Assert.Single(template.LinkedExerciseSlugs));
    }
}
