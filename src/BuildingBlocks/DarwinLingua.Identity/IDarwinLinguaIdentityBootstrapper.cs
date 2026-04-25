namespace DarwinLingua.Identity;

public interface IDarwinLinguaIdentityBootstrapper
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
