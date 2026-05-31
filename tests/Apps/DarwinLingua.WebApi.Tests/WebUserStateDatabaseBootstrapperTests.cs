using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebUserStateDatabaseBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_RequiresConfiguredPostgresProvider()
    {
        DbContextOptions<WebIdentityDbContext> options = new DbContextOptionsBuilder<WebIdentityDbContext>()
            .Options;

        await using WebIdentityDbContext dbContext = new(options);
        WebUserStateDatabaseBootstrapper bootstrapper = new(dbContext);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bootstrapper.InitializeAsync(CancellationToken.None));
        Assert.Contains("provider", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
