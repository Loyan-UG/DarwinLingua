using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record AdminExpressionsPageViewModel(
    IReadOnlyList<ExpressionListItemModel> Expressions);

