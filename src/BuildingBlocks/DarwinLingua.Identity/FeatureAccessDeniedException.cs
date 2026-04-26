namespace DarwinLingua.Identity;

public sealed class FeatureAccessDeniedException : InvalidOperationException
{
    public FeatureAccessDeniedException(string featureKey, string message)
        : base(message)
    {
        FeatureKey = featureKey;
    }

    public string FeatureKey { get; }
}
