using System.Globalization;
using DarwinLingua.Web.Localization;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.WebApi.Tests;

internal sealed class TestStringLocalizer : IStringLocalizer<SharedResource>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments] =>
        new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
