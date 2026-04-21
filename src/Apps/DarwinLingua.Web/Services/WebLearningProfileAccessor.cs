using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Services;

public interface IWebLearningProfileAccessor
{
    Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken);
}

internal sealed class WebLearningProfileAccessor(IUserLearningProfileService userLearningProfileService) : IWebLearningProfileAccessor
{
    public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) =>
        userLearningProfileService.GetCurrentProfileAsync(cancellationToken);
}
