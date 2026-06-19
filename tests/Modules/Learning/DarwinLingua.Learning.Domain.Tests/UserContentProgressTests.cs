using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Learning.Domain.Tests;

public sealed class UserContentProgressTests
{
    [Fact]
    public void Constructor_ShouldRejectUnsupportedOwnerType()
    {
        Assert.Throws<DomainRuleException>(() => new UserContentProgress(
            Guid.NewGuid(),
            "user-1",
            "unsupported",
            "a1-lesson",
            DateTime.UtcNow));
    }

    [Fact]
    public void ApplyState_ShouldTrackViewedState()
    {
        UserContentProgress progress = new(
            Guid.NewGuid(),
            "user-1",
            "course-lesson",
            "a1-lesson",
            DateTime.UtcNow);

        progress.ApplyState("viewed", DateTime.UtcNow.AddMinutes(1));

        Assert.Equal("viewed", progress.State);
        Assert.Equal(1, progress.ViewCount);
        Assert.NotNull(progress.FirstViewedAtUtc);
        Assert.NotNull(progress.LastViewedAtUtc);
    }

    [Fact]
    public void ApplyState_ShouldTrackCompletedStateWithoutDuplicatingContent()
    {
        UserContentProgress progress = new(
            Guid.NewGuid(),
            "user-1",
            "grammar-topic",
            "a1-articles",
            DateTime.UtcNow);

        progress.ApplyState("completed", DateTime.UtcNow.AddMinutes(1));

        Assert.Equal("completed", progress.State);
        Assert.Equal("grammar-topic", progress.ContentOwnerType);
        Assert.Equal("a1-articles", progress.ContentOwnerSlug);
        Assert.NotNull(progress.CompletedAtUtc);
    }

    [Fact]
    public void ApplyState_ShouldClearCompletedAt_WhenStateChangesAwayFromCompleted()
    {
        UserContentProgress progress = new(
            Guid.NewGuid(),
            "user-1",
            "course-lesson",
            "a1-lesson",
            DateTime.UtcNow);

        progress.ApplyState("completed", DateTime.UtcNow.AddMinutes(1));
        progress.ApplyState("needs-review", DateTime.UtcNow.AddMinutes(2));

        Assert.Equal("needs-review", progress.State);
        Assert.Null(progress.CompletedAtUtc);
    }

    [Fact]
    public void ApplyState_ShouldRejectUnsupportedState()
    {
        UserContentProgress progress = new(
            Guid.NewGuid(),
            "user-1",
            "expression",
            "guten-morgen",
            DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => progress.ApplyState("mastered", DateTime.UtcNow));
    }
}
