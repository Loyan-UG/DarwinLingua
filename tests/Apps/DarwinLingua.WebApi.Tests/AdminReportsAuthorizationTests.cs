using DarwinLingua.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminReportsAuthorizationTests
{
    [Fact]
    public void ReportsController_ShouldRequireAdminPolicy()
    {
        AuthorizeAttribute attribute = Assert.Single(
            typeof(ReportsController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal("Admin", attribute.Policy);
    }
}
