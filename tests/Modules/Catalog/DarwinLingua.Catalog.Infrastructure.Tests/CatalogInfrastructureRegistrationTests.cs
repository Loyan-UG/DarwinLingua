using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies the catalog infrastructure registrations.
/// </summary>
public sealed class CatalogInfrastructureRegistrationTests
{
    /// <summary>
    /// Verifies that the topic repository, word repository, and seed workflow are registered.
    /// </summary>
    [Fact]
    public void AddCatalogInfrastructure_ShouldRegisterCatalogInfrastructureServices()
    {
        ServiceCollection services = new();

        services.AddCatalogInfrastructure();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ITopicRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IWordEntryRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IConversationEventRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOrganizerProfileRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDatabaseSeeder));
    }
}
