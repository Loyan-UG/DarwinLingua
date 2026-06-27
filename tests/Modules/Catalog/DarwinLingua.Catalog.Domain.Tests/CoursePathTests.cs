using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Tests;

public sealed class CoursePathTests
{
    [Fact]
    public void UpdateDetails_ShouldPreserveExistingTargetLearningLanguage_WhenTargetIsNotProvided()
    {
        CoursePath path = new(
            Guid.NewGuid(),
            "en-a1-start",
            "A1 Start",
            "Start learning everyday English.",
            CefrLevel.A1,
            "A1",
            PublicationStatus.Active,
            10,
            DateTime.UtcNow,
            targetLearningLanguageCode: "en");

        path.UpdateDetails(
            "A1 Start updated",
            "Updated description.",
            CefrLevel.A1,
            "A1",
            PublicationStatus.Active,
            20,
            DateTime.UtcNow);

        Assert.Equal("en", path.TargetLearningLanguageCode);
    }
}
