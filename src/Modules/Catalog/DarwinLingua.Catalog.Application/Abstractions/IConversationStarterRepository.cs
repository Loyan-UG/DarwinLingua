using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IConversationStarterRepository
{
    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksForDialogueAsync(
        string dialogueSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<ConversationStarterPackDetailModel?> GetPublishedStarterPackBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
