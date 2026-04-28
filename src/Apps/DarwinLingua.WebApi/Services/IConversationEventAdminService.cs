using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IConversationEventAdminService
{
    Task<ConversationEventDetailModel> SaveAsync(
        AdminSaveConversationEventRequest request,
        CancellationToken cancellationToken);
}
