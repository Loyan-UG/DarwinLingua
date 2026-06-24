using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ConversationStarterQueryService(IConversationStarterRepository conversationStarterRepository) : IConversationStarterQueryService
{
    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        conversationStarterRepository.GetPublishedStarterPacksAsync(filter, targetLearningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksForDialogueAsync(
        string dialogueSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        conversationStarterRepository.GetPublishedStarterPacksForDialogueAsync(dialogueSlug, targetLearningLanguageCode, cancellationToken);

    public Task<ConversationStarterPackDetailModel?> GetPublishedStarterPackBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        conversationStarterRepository.GetPublishedStarterPackBySlugAsync(
            slug,
            targetLearningLanguageCode,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken);
}
