using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IConversationEventAdminService
{
    Task<ConversationEventDetailModel> SaveAsync(
        AdminSaveConversationEventRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizerManagedConversationEventModel>> GetByOrganizerProfileSlugAsync(
        string organizerProfileSlug,
        CancellationToken cancellationToken);

    Task<OrganizerManagedConversationEventModel> SetPublicationStatusAsync(
        string slug,
        AdminSetConversationEventPublicationStatusRequest request,
        CancellationToken cancellationToken);
}
