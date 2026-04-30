namespace DarwinLingua.Web.Services;

public interface IStripeBillingReconciliationService
{
    Task<StripeBillingReconciliationResult> ReconcileSubscriptionAsync(
        string subscriptionId,
        string updatedBy,
        CancellationToken cancellationToken);
}
