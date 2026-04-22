using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Services;

public interface IWebLearningProfileAccessor
{
    Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken);
}

internal sealed class WebLearningProfileAccessor(IWebUserPreferenceService webUserPreferenceService) : IWebLearningProfileAccessor
{
    public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) =>
        webUserPreferenceService.GetProfileAsync(cancellationToken);
}
