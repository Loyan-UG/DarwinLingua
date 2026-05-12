using DarwinLingua.Learning.Domain.Entities;

namespace DarwinLingua.Learning.Application.Abstractions;

public interface IUserContentProgressRepository
{
    Task<UserContentProgress?> GetByUserAndContentAsync(
        string userId,
        string contentOwnerType,
        string contentOwnerSlug,
        CancellationToken cancellationToken);

    Task AddAsync(UserContentProgress progress, CancellationToken cancellationToken);

    Task UpdateAsync(UserContentProgress progress, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserContentProgress>> GetUserProgressAsync(
        string userId,
        int recentItemCount,
        CancellationToken cancellationToken);
}
