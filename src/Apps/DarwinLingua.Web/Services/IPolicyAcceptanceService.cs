namespace DarwinLingua.Web.Services;

public interface IPolicyAcceptanceService
{
    Task RecordRegistrationAcceptancesAsync(
        string userId,
        string? culture,
        CancellationToken cancellationToken);
}
