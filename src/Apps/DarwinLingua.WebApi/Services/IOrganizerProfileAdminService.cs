using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IOrganizerProfileAdminService
{
    Task<OrganizerProfileDetailModel> SaveAsync(
        AdminSaveOrganizerProfileRequest request,
        CancellationToken cancellationToken);
}
