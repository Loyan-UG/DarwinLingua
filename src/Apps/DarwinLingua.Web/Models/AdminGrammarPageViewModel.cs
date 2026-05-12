using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record AdminGrammarPageViewModel(
    IReadOnlyList<GrammarTopicListItemModel> GrammarTopics);
