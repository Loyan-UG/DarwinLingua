namespace DarwinLingua.Web.Services;

public interface IStripeBillingWebhookHandler
{
    Task HandleAsync(string payload, CancellationToken cancellationToken);
}
