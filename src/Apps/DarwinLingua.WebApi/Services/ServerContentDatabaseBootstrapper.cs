using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Creates the server-content database schema and imports configured bootstrap metadata.
/// </summary>
public sealed class ServerContentDatabaseBootstrapper(
    ServerContentDbContext dbContext,
    IOptions<ServerContentOptions> options) : IServerContentDatabaseBootstrapper
{
    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(options);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        Dictionary<string, ClientProductEntity> productsByKey = await dbContext.ClientProducts
            .Include(product => product.ContentStreams)
            .ThenInclude(stream => stream.PublishedPackages)
            .ToDictionaryAsync(product => product.Key, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        foreach (ClientProductOptions configuredProduct in options.Value.ClientProducts)
        {
            if (!productsByKey.TryGetValue(configuredProduct.Key, out ClientProductEntity? productEntity))
            {
                productEntity = new ClientProductEntity
                {
                    Id = Guid.NewGuid(),
                    Key = configuredProduct.Key.Trim(),
                    CreatedAtUtc = now,
                };

                dbContext.ClientProducts.Add(productEntity);
                productsByKey[productEntity.Key] = productEntity;
            }

            productEntity.DisplayName = configuredProduct.DisplayName.Trim();
            productEntity.LearningLanguageCode = configuredProduct.LearningLanguageCode.Trim();
            productEntity.DefaultUiLanguageCode = configuredProduct.DefaultUiLanguageCode.Trim();
            productEntity.IsActive = configuredProduct.IsActive;
            productEntity.UpdatedAtUtc = now;
        }

        foreach (PublishedPackageOptions configuredPackage in options.Value.Packages)
        {
            ClientProductEntity product = productsByKey[configuredPackage.ClientProductKey];

            ContentStreamEntity? stream = product.ContentStreams.FirstOrDefault(existingStream =>
                existingStream.ContentAreaKey.Equals(configuredPackage.ContentAreaKey, StringComparison.OrdinalIgnoreCase) &&
                existingStream.SliceKey.Equals(configuredPackage.SliceKey, StringComparison.OrdinalIgnoreCase));

            if (stream is null)
            {
                stream = new ContentStreamEntity
                {
                    Id = Guid.NewGuid(),
                    ClientProductId = product.Id,
                    ClientProduct = product,
                    ContentAreaKey = configuredPackage.ContentAreaKey.Trim(),
                    SliceKey = configuredPackage.SliceKey.Trim(),
                    LearningLanguageCode = product.LearningLanguageCode,
                    SchemaVersion = configuredPackage.SchemaVersion,
                    IsActive = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };

                dbContext.ContentStreams.Add(stream);
                product.ContentStreams.Add(stream);
            }
            else
            {
                stream.SchemaVersion = configuredPackage.SchemaVersion;
                stream.IsActive = true;
                stream.LearningLanguageCode = product.LearningLanguageCode;
                stream.UpdatedAtUtc = now;
            }

            PublishedPackageEntity? package = stream.PublishedPackages.FirstOrDefault(existingPackage =>
                existingPackage.PackageId.Equals(configuredPackage.PackageId, StringComparison.OrdinalIgnoreCase));

            if (package is null)
            {
                package = new PublishedPackageEntity
                {
                    Id = Guid.NewGuid(),
                    PackageId = configuredPackage.PackageId.Trim(),
                    ContentStreamId = stream.Id,
                    ContentStream = stream,
                    CreatedAtUtc = configuredPackage.CreatedAtUtc == default ? now : configuredPackage.CreatedAtUtc,
                };

                dbContext.PublishedPackages.Add(package);
                stream.PublishedPackages.Add(package);
            }

            package.PackageType = configuredPackage.PackageType.Trim();
            package.Version = configuredPackage.Version.Trim();
            package.SchemaVersion = configuredPackage.SchemaVersion;
            package.MinimumAppSchemaVersion = configuredPackage.MinimumAppSchemaVersion;
            package.Checksum = configuredPackage.Checksum.Trim();
            package.EntryCount = configuredPackage.EntryCount;
            package.WordCount = configuredPackage.WordCount;
            package.RelativeDownloadPath = configuredPackage.RelativeDownloadPath.Trim();
            package.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
