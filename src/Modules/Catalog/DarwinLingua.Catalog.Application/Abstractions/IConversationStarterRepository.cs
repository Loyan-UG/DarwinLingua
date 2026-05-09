using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IConversationStarterRepository
{
    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksForDialogueAsync(
        string dialogueSlug,
        CancellationToken cancellationToken);

    Task<ConversationStarterPackDetailModel?> GetPublishedStarterPackBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
